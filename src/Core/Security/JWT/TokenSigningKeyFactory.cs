using Core.Security.Encryption;
using Microsoft.IdentityModel.Tokens;

namespace Core.Security.JWT;

public static class TokenSigningKeyFactory
{
    /// <summary>Signing key = current only.</summary>
    public static SecurityKey CreateSigningKey(TokenOptions options) =>
        SecurityKeyHelper.CreateSecurityKey(options.SecurityKey);

    /// <summary>Validation keys = current + optional previous (dual-key rotation window).</summary>
    public static IEnumerable<SecurityKey> CreateValidationKeys(TokenOptions options)
    {
        yield return SecurityKeyHelper.CreateSecurityKey(options.SecurityKey);

        if (!string.IsNullOrWhiteSpace(options.SecurityKeyPrevious)
            && !string.Equals(options.SecurityKeyPrevious, options.SecurityKey, StringComparison.Ordinal))
        {
            yield return SecurityKeyHelper.CreateSecurityKey(options.SecurityKeyPrevious);
        }
    }
}
