using System.Security.Claims;
using Core.Security.Constants;

namespace Core.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static List<string>? Claims(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        return claimsPrincipal.FindAll(claimType).Select(c => c.Value).ToList();
    }

    public static List<string>? ClaimRoles(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.Claims(ClaimTypes.Role);
    }

    public static Guid? GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        string? value = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out Guid userId) ? userId : null;
    }

    public static Guid? GetCustomerId(this ClaimsPrincipal claimsPrincipal)
    {
        string? value = claimsPrincipal.FindFirst(CustomClaimTypes.CustomerId)?.Value;
        return Guid.TryParse(value, out Guid customerId) ? customerId : null;
    }

    public static Guid? GetBranchId(this ClaimsPrincipal claimsPrincipal)
    {
        string? value = claimsPrincipal.FindFirst(CustomClaimTypes.BranchId)?.Value;
        return Guid.TryParse(value, out Guid branchId) ? branchId : null;
    }
}
