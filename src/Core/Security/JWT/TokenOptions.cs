namespace Core.Security.JWT;

public class TokenOptions
{
    public string Audience { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    /// <summary>Access token lifetime in minutes (default session).</summary>
    public int AccessTokenExpiration { get; set; }
    /// <summary>Access token lifetime in minutes when "Remember me" is selected.</summary>
    public int RememberMeAccessTokenExpiration { get; set; } = 10080; // 7 days
    public string SecurityKey { get; set; } = default!;
}
