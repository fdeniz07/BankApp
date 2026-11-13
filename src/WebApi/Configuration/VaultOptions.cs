namespace WebApi.Configuration;

public sealed class VaultOptions
{
    public const string SectionName = "Vault";

    public bool Enabled { get; set; }

    /// <summary>Base URL, e.g. http://vault:8200</summary>
    public string Address { get; set; } = "http://localhost:8200";

    /// <summary>
    /// Optional static token (lab/debug only). Prefer AppRole via RoleId/SecretId files.
    /// Compose does not inject the root token into WebApi.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>AppRole role_id (alternative to <see cref="RoleIdFile"/>).</summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>AppRole secret_id (alternative to <see cref="SecretIdFile"/>).</summary>
    public string SecretId { get; set; } = string.Empty;

    /// <summary>File containing role_id (Compose volume).</summary>
    public string RoleIdFile { get; set; } = string.Empty;

    /// <summary>File containing secret_id (Compose volume).</summary>
    public string SecretIdFile { get; set; } = string.Empty;

    /// <summary>KV v2 mount, usually "secret" in Vault lab.</summary>
    public string MountPath { get; set; } = "secret";

    /// <summary>KV path for JWT HMAC keys (under mount), e.g. bankapp/jwt.</summary>
    public string JwtSecretPath { get; set; } = "bankapp/jwt";

    /// <summary>KV path for database credentials, e.g. bankapp/database.</summary>
    public string DatabaseSecretPath { get; set; } = "bankapp/database";

    /// <summary>KV path for messaging credentials, e.g. bankapp/messaging.</summary>
    public string MessagingSecretPath { get; set; } = "bankapp/messaging";

    /// <summary>KV path for cache (Redis) credentials, e.g. bankapp/cache.</summary>
    public string CacheSecretPath { get; set; } = "bankapp/cache";

    /// <summary>
    /// Legacy single path (flat secret/bankapp). Used only if JwtSecretPath is empty.
    /// </summary>
    public string SecretPath { get; set; } = "bankapp/jwt";

    /// <summary>
    /// When true, startup fails if Vault login or required secret reads fail.
    /// Compose lab sets true; local Development keeps false.
    /// </summary>
    public bool FailClosed { get; set; }
}
