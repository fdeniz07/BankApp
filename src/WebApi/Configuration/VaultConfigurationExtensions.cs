using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace WebApi.Configuration;

/// <summary>
/// Lab/prod-shaped: AppRole login + KV v2 reads for JWT / database / messaging.
/// Overlays TokenOptions, ConnectionStrings password, and RabbitMq credentials.
/// Observability secrets stay out of the WebApi policy (not read here).
/// </summary>
public static class VaultConfigurationExtensions
{
    private static readonly Regex PasswordInConnectionString = new(
        @"(?i)(Password|Pwd)\s*=\s*([^;]*)",
        RegexOptions.Compiled);

    public static async Task ApplyVaultSecretsAsync(
        this ConfigurationManager configuration,
        CancellationToken cancellationToken = default)
    {
        VaultOptions vault = configuration.GetSection(VaultOptions.SectionName).Get<VaultOptions>()
            ?? new VaultOptions();

        if (!vault.Enabled)
        {
            Log.Information("Vault secret overlay disabled (Vault:Enabled=false).");
            return;
        }

        try
        {
            await ApplyVaultSecretsCoreAsync(configuration, vault, cancellationToken);
        }
        catch (Exception ex) when (ex is not VaultFailClosedException)
        {
            if (vault.FailClosed)
            {
                Log.Error(ex, "Vault overlay failed and FailClosed=true; aborting startup.");
                throw new VaultFailClosedException("Vault secret overlay failed (FailClosed=true).", ex);
            }

            Log.Warning(ex, "Vault unreachable or login failed; keeping existing configuration.");
        }
    }

    private static async Task ApplyVaultSecretsCoreAsync(
        ConfigurationManager configuration,
        VaultOptions vault,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(vault.Address))
        {
            FailOrWarn(vault, "Vault enabled but Address missing.");
            return;
        }

        string? clientToken = await ResolveClientTokenAsync(vault, cancellationToken);
        if (string.IsNullOrWhiteSpace(clientToken))
        {
            FailOrWarn(vault, "Vault enabled but AppRole/Token credentials missing or login failed.");
            return;
        }

        string mount = vault.MountPath.Trim('/');
        string jwtPath = FirstNonEmpty(vault.JwtSecretPath, vault.SecretPath, "bankapp/jwt");
        string databasePath = vault.DatabaseSecretPath.Trim('/');
        string messagingPath = vault.MessagingSecretPath.Trim('/');
        string cachePath = vault.CacheSecretPath.Trim('/');

        Dictionary<string, string?> overlay = new(StringComparer.OrdinalIgnoreCase);

        JsonElement? jwtData = await ReadKvDataAsync(vault.Address, mount, jwtPath, clientToken, cancellationToken);
        if (jwtData is null)
        {
            FailOrWarn(vault, $"Vault read failed for {mount}/data/{jwtPath}.");
            return;
        }

        ApplyJwtOverlay(jwtData.Value, overlay);
        if (!overlay.ContainsKey("TokenOptions:SecurityKey"))
        {
            FailOrWarn(vault, $"Vault path {jwtPath} has no jwt_security_key.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(databasePath))
        {
            JsonElement? dbData = await ReadKvDataAsync(
                vault.Address, mount, databasePath, clientToken, cancellationToken);
            if (dbData is null)
            {
                FailOrWarn(vault, $"Vault read failed for {mount}/data/{databasePath}.");
                return;
            }

            ApplyDatabaseOverlay(configuration, dbData.Value, overlay);
        }

        if (!string.IsNullOrWhiteSpace(messagingPath))
        {
            JsonElement? mqData = await ReadKvDataAsync(
                vault.Address, mount, messagingPath, clientToken, cancellationToken);
            if (mqData is null)
            {
                FailOrWarn(vault, $"Vault read failed for {mount}/data/{messagingPath}.");
                return;
            }

            ApplyMessagingOverlay(mqData.Value, overlay);
        }

        if (!string.IsNullOrWhiteSpace(cachePath))
        {
            JsonElement? cacheData = await ReadKvDataAsync(
                vault.Address, mount, cachePath, clientToken, cancellationToken);
            if (cacheData is null)
            {
                FailOrWarn(vault, $"Vault read failed for {mount}/data/{cachePath}.");
                return;
            }

            ApplyCacheOverlay(configuration, cacheData.Value, overlay);
        }

        configuration.AddInMemoryCollection(overlay!);

        Log.Information(
            "Vault overlay applied via AppRole/token (JWT{Previous}, SQL={Sql}, RabbitMq={Mq}, Redis={Redis}).",
            overlay.ContainsKey("TokenOptions:SecurityKeyPrevious") ? " + Previous" : string.Empty,
            overlay.ContainsKey("ConnectionStrings:BankingConnectionString"),
            overlay.ContainsKey("RabbitMq:Password") || overlay.ContainsKey("RabbitMq:UserName"),
            overlay.ContainsKey("Redis:ConnectionString"));
    }

    private static void FailOrWarn(VaultOptions vault, string message)
    {
        if (vault.FailClosed)
            throw new VaultFailClosedException(message);

        Log.Warning("{Message} Keeping existing configuration.", message);
    }

    private static async Task<string?> ResolveClientTokenAsync(
        VaultOptions vault,
        CancellationToken cancellationToken)
    {
        string roleId = ReadCredential(vault.RoleId, vault.RoleIdFile);
        string secretId = ReadCredential(vault.SecretId, vault.SecretIdFile);

        if (!string.IsNullOrWhiteSpace(roleId) && !string.IsNullOrWhiteSpace(secretId))
        {
            string? token = await LoginAppRoleAsync(vault.Address, roleId, secretId, cancellationToken);
            if (!string.IsNullOrWhiteSpace(token))
                return token;

            Log.Warning("Vault AppRole login failed.");
            return null;
        }

        if (!string.IsNullOrWhiteSpace(vault.Token))
        {
            Log.Warning("Vault using static Token (prefer AppRole in Compose lab).");
            return vault.Token;
        }

        return null;
    }

    private static string ReadCredential(string inline, string filePath)
    {
        if (!string.IsNullOrWhiteSpace(inline))
            return inline.Trim();

        if (string.IsNullOrWhiteSpace(filePath))
            return string.Empty;

        try
        {
            if (!File.Exists(filePath))
            {
                Log.Warning("Vault credential file missing: {Path}", filePath);
                return string.Empty;
            }

            return File.ReadAllText(filePath).Trim();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Vault credential file unreadable: {Path}", filePath);
            return string.Empty;
        }
    }

    private static async Task<string?> LoginAppRoleAsync(
        string address,
        string roleId,
        string secretId,
        CancellationToken cancellationToken)
    {
        string url = $"{address.TrimEnd('/')}/v1/auth/approle/login";
        using HttpClient client = CreateHttpClient();
        using StringContent body = new(
            JsonSerializer.Serialize(new { role_id = roleId, secret_id = secretId }),
            Encoding.UTF8,
            "application/json");

        using HttpResponseMessage response = await client.PostAsync(url, body, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Log.Warning(
                "Vault AppRole login HTTP {StatusCode} at {Url}",
                (int)response.StatusCode,
                url);
            return null;
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (doc.RootElement.TryGetProperty("auth", out JsonElement auth)
            && auth.TryGetProperty("client_token", out JsonElement tokenEl))
        {
            return tokenEl.GetString();
        }

        return null;
    }

    private static async Task<JsonElement?> ReadKvDataAsync(
        string address,
        string mount,
        string secretPath,
        string clientToken,
        CancellationToken cancellationToken)
    {
        string path = secretPath.Trim('/');
        string url = $"{address.TrimEnd('/')}/v1/{mount}/data/{path}";

        using HttpClient client = CreateHttpClient();
        client.DefaultRequestHeaders.Add("X-Vault-Token", clientToken);

        using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Log.Warning(
                "Vault KV read failed ({StatusCode}) at {Url}",
                (int)response.StatusCode,
                url);
            return null;
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using JsonDocument doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!doc.RootElement.TryGetProperty("data", out JsonElement dataWrapper)
            || !dataWrapper.TryGetProperty("data", out JsonElement data))
        {
            Log.Warning("Vault response missing data.data at {Url}", url);
            return null;
        }

        return data.Clone();
    }

    private static void ApplyJwtOverlay(JsonElement data, Dictionary<string, string?> overlay)
    {
        if (data.TryGetProperty("jwt_security_key", out JsonElement jwtEl))
        {
            string? jwtKey = jwtEl.GetString();
            if (!string.IsNullOrWhiteSpace(jwtKey))
                overlay["TokenOptions:SecurityKey"] = jwtKey;
        }

        if (data.TryGetProperty("jwt_security_key_previous", out JsonElement prevEl))
        {
            string? prevKey = prevEl.GetString();
            if (!string.IsNullOrWhiteSpace(prevKey))
                overlay["TokenOptions:SecurityKeyPrevious"] = prevKey;
        }
    }

    private static void ApplyDatabaseOverlay(
        IConfiguration configuration,
        JsonElement data,
        Dictionary<string, string?> overlay)
    {
        if (!data.TryGetProperty("mssql_sa_password", out JsonElement pwdEl))
            return;

        string? password = pwdEl.GetString();
        if (string.IsNullOrWhiteSpace(password))
            return;

        string? current = configuration.GetConnectionString("BankingConnectionString");
        if (string.IsNullOrWhiteSpace(current))
        {
            Log.Warning("Vault has mssql_sa_password but ConnectionStrings:BankingConnectionString is empty.");
            return;
        }

        overlay["ConnectionStrings:BankingConnectionString"] = PatchConnectionStringPassword(current, password);
    }

    private static void ApplyMessagingOverlay(JsonElement data, Dictionary<string, string?> overlay)
    {
        if (data.TryGetProperty("rabbitmq_username", out JsonElement userEl))
        {
            string? user = userEl.GetString();
            if (!string.IsNullOrWhiteSpace(user))
                overlay["RabbitMq:UserName"] = user;
        }

        if (data.TryGetProperty("rabbitmq_password", out JsonElement pwdEl))
        {
            string? password = pwdEl.GetString();
            if (!string.IsNullOrWhiteSpace(password))
                overlay["RabbitMq:Password"] = password;
        }
    }

    private static void ApplyCacheOverlay(
        IConfiguration configuration,
        JsonElement data,
        Dictionary<string, string?> overlay)
    {
        if (!data.TryGetProperty("redis_password", out JsonElement pwdEl))
            return;

        string? password = pwdEl.GetString();
        if (string.IsNullOrWhiteSpace(password))
            return;

        string? current = configuration["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(current))
            current = "redis:6379";

        overlay["Redis:ConnectionString"] = PatchRedisConnectionStringPassword(current, password);
    }

    internal static string PatchRedisConnectionStringPassword(string connectionString, string password)
    {
        string trimmed = connectionString.Trim();
        if (trimmed.Contains("password=", StringComparison.OrdinalIgnoreCase))
        {
            return Regex.Replace(
                trimmed,
                "(?i)password=[^,]*",
                $"password={password}");
        }

        return trimmed + (trimmed.EndsWith(',') ? string.Empty : ",") + $"password={password}";
    }

    internal static string PatchConnectionStringPassword(string connectionString, string password)
    {
        if (PasswordInConnectionString.IsMatch(connectionString))
            return PasswordInConnectionString.Replace(connectionString, $"$1={password}");

        string trimmed = connectionString.TrimEnd();
        if (!trimmed.EndsWith(';'))
            trimmed += ";";
        return trimmed + $"Password={password};";
    }

    private static HttpClient CreateHttpClient()
    {
        HttpClient client = new() { Timeout = TimeSpan.FromSeconds(5) };
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private static string FirstNonEmpty(params string[] values)
    {
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim('/');
        }

        return string.Empty;
    }
}

public sealed class VaultFailClosedException : Exception
{
    public VaultFailClosedException(string message)
        : base(message)
    {
    }

    public VaultFailClosedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
