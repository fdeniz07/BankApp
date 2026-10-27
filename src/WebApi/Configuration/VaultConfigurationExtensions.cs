using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace WebApi.Configuration;

/// <summary>
/// Lab: optionally overlay TokenOptions:SecurityKey from Vault KV v2 (secret/bankapp).
/// On failure, keeps existing configuration (Compose /.env fallback).
/// </summary>
public static class VaultConfigurationExtensions
{
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

        if (string.IsNullOrWhiteSpace(vault.Address) || string.IsNullOrWhiteSpace(vault.Token))
        {
            Log.Warning("Vault enabled but Address/Token missing; keeping existing TokenOptions.");
            return;
        }

        string mount = vault.MountPath.Trim('/');
        string path = vault.SecretPath.Trim('/');
        string url = $"{vault.Address.TrimEnd('/')}/v1/{mount}/data/{path}";

        try
        {
            using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(5) };
            client.DefaultRequestHeaders.Add("X-Vault-Token", vault.Token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning(
                    "Vault read failed ({StatusCode}) at {Url}; keeping existing TokenOptions.",
                    (int)response.StatusCode,
                    url);
                return;
            }

            await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!doc.RootElement.TryGetProperty("data", out JsonElement dataWrapper)
                || !dataWrapper.TryGetProperty("data", out JsonElement data))
            {
                Log.Warning("Vault response missing data.data; keeping existing TokenOptions.");
                return;
            }

            Dictionary<string, string?> overlay = new(StringComparer.OrdinalIgnoreCase);

            if (data.TryGetProperty("jwt_security_key", out JsonElement jwtEl))
            {
                string? jwtKey = jwtEl.GetString();
                if (!string.IsNullOrWhiteSpace(jwtKey))
                    overlay["TokenOptions:SecurityKey"] = jwtKey;
            }

            if (overlay.Count == 0)
            {
                Log.Warning("Vault secret/bankapp has no jwt_security_key; keeping existing TokenOptions.");
                return;
            }

            configuration.AddInMemoryCollection(overlay!);
            Log.Information(
                "Vault overlay applied from {Url} (TokenOptions:SecurityKey).",
                url);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Vault unreachable; keeping existing TokenOptions from configuration.");
        }
    }
}
