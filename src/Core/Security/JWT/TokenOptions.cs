namespace Core.Security.JWT;

public class TokenOptions
{
    public string Audience { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    /// <summary>Access token lifetime in minutes (default session).</summary>
    public int AccessTokenExpiration { get; set; }
    /// <summary>Access token lifetime in minutes when "Remember me" is selected.</summary>
    public int RememberMeAccessTokenExpiration { get; set; } = 10080; // 7 days
    /// <summary>Active HMAC key used to sign new tokens.</summary>
    public string SecurityKey { get; set; } = default!;
    /// <summary>
    /// Previous HMAC key (lab rotation window). Validation accepts current or previous;
    /// signing always uses <see cref="SecurityKey"/>.
    /// </summary>
    public string? SecurityKeyPrevious { get; set; }
}
