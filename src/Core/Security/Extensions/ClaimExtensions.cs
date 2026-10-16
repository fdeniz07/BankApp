using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core.Security.Extensions;

public static class ClaimExtensions
{
    public static void AddEmail(this ICollection<Claim> claims, string email)
    {
        claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
    }

    public static void AddNameIdentifier(this ICollection<Claim> claims, string nameIdentifier)
    {
        claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifier));
    }

    public static void AddRoles(this ICollection<Claim> claims, string[] roles)
    {
        foreach (string role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
    }

    public static void AddCustomerId(this ICollection<Claim> claims, Guid customerId)
    {
        claims.Add(new Claim(Constants.CustomClaimTypes.CustomerId, customerId.ToString()));
    }

    public static void AddDisplayName(this ICollection<Claim> claims, string displayName)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            claims.Add(new Claim(Constants.CustomClaimTypes.DisplayName, displayName));
    }

    public static void AddCustomerCategory(this ICollection<Claim> claims, string customerCategory)
    {
        if (!string.IsNullOrWhiteSpace(customerCategory))
            claims.Add(new Claim(Constants.CustomClaimTypes.CustomerCategory, customerCategory));
    }
}
