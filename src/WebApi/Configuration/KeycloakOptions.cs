namespace WebApi.Configuration;

/// <summary>Lab: accept Keycloak JWTs alongside local TokenOptions JWTs.</summary>
public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";
    public const string AuthenticationScheme = "Keycloak";

    public bool Enabled { get; set; }

    /// <summary>OIDC metadata / JWKS base (e.g. http://localhost:8180/realms/bankapp or http://keycloak:8080/realms/bankapp).</summary>
    public string Authority { get; set; } = "http://localhost:8180/realms/bankapp";

    /// <summary>Accepted token iss values (host-issued tokens often use localhost even when JWKS is fetched in-network).</summary>
    public string[] ValidIssuers { get; set; } =
    [
        "http://localhost:8180/realms/bankapp",
        "http://keycloak:8080/realms/bankapp"
    ];

    /// <summary>Issuer substring used to select the Keycloak bearer scheme.</summary>
    public string RealmPathMarker { get; set; } = "/realms/bankapp";
}
