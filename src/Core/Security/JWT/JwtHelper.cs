using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.Security.Encryption;
using Core.Security.Entities;
using Core.Security.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Core.Security.JWT;

public class JwtHelper : ITokenHelper
{
    private readonly TokenOptions _tokenOptions;
    private DateTime _accessTokenExpiration;

    public JwtHelper(IConfiguration configuration)
    {
        _tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>()
            ?? throw new InvalidOperationException("TokenOptions configuration is missing.");
    }

    public AccessToken CreateToken(
        User user,
        IList<OperationClaim> operationClaims,
        Guid? customerId = null,
        string? displayName = null,
        string? customerCategory = null,
        int? expirationMinutes = null)
    {
        int minutes = expirationMinutes is > 0
            ? expirationMinutes.Value
            : _tokenOptions.AccessTokenExpiration;
        _accessTokenExpiration = DateTime.UtcNow.AddMinutes(minutes);
        SecurityKey securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
        SigningCredentials signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
        JwtSecurityToken jwt = CreateJwtSecurityToken(
            _tokenOptions,
            user,
            signingCredentials,
            operationClaims,
            customerId,
            displayName,
            customerCategory);
        JwtSecurityTokenHandler tokenHandler = new();
        string token = tokenHandler.WriteToken(jwt);

        return new AccessToken
        {
            Token = token,
            Expiration = _accessTokenExpiration
        };
    }

    private JwtSecurityToken CreateJwtSecurityToken(
        TokenOptions tokenOptions,
        User user,
        SigningCredentials signingCredentials,
        IList<OperationClaim> operationClaims,
        Guid? customerId,
        string? displayName,
        string? customerCategory)
    {
        return new JwtSecurityToken(
            issuer: tokenOptions.Issuer,
            audience: tokenOptions.Audience,
            expires: _accessTokenExpiration,
            notBefore: DateTime.UtcNow,
            claims: SetClaims(user, operationClaims, customerId, displayName, customerCategory),
            signingCredentials: signingCredentials);
    }

    private static IEnumerable<Claim> SetClaims(
        User user,
        IList<OperationClaim> operationClaims,
        Guid? customerId,
        string? displayName,
        string? customerCategory)
    {
        List<Claim> claims = [];
        claims.AddNameIdentifier(user.Id.ToString());
        claims.AddEmail(user.Email);
        claims.AddRoles(operationClaims.Select(c => c.Name).ToArray());

        if (customerId.HasValue)
            claims.AddCustomerId(customerId.Value);

        if (!string.IsNullOrWhiteSpace(displayName))
            claims.AddDisplayName(displayName);

        if (!string.IsNullOrWhiteSpace(customerCategory))
            claims.AddCustomerCategory(customerCategory);

        return claims;
    }
}
