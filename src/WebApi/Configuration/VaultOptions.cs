namespace WebApi.Configuration;

public sealed class VaultOptions
{
    public const string SectionName = "Vault";

    public bool Enabled { get; set; }

    /// <summary>Base URL, e.g. http://vault:8200</summary>
    public string Address { get; set; } = "http://localhost:8200";

    public string Token { get; set; } = string.Empty;

    /// <summary>KV v2 mount, usually "secret" in Vault lab.</summary>
    public string MountPath { get; set; } = "secret";

    /// <summary>Secret name under the mount, e.g. bankapp → secret/data/bankapp</summary>
    public string SecretPath { get; set; } = "bankapp";
}
