using System.Security.Claims;
using Application.Services.Repositories;
using Core.Security.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WebApi.Authentication;

/// <summary>
/// Maps Keycloak <c>role</c> → <see cref="ClaimTypes.Role"/> and enriches BankApp user claims from SQL by email.
/// </summary>
public static class KeycloakTokenEnricher
{
    public const string RoleClaim = "role";

    public static async Task EnrichAsync(TokenValidatedContext context, CancellationToken cancellationToken)
    {
        ClaimsIdentity? identity = context.Principal?.Identity as ClaimsIdentity;
        if (identity is null)
            return;

        foreach (Claim role in context.Principal!.FindAll(RoleClaim).ToList())
        {
            if (!identity.HasClaim(ClaimTypes.Role, role.Value))
                identity.AddClaim(new Claim(ClaimTypes.Role, role.Value));
        }

        string? email = context.Principal.FindFirst("email")?.Value
            ?? context.Principal.FindFirst("preferred_username")?.Value;
        if (string.IsNullOrWhiteSpace(email))
            return;

        IApplicationUserRepository users =
            context.HttpContext.RequestServices.GetRequiredService<IApplicationUserRepository>();
        ApplicationUser? user = await users.GetAsync(
            u => u.Email == email && !u.IsDeleted,
            cancellationToken: cancellationToken);
        if (user is null)
            return;

        Claim? existingSub = identity.FindFirst(ClaimTypes.NameIdentifier);
        if (existingSub is not null)
            identity.RemoveClaim(existingSub);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

        if (user.BranchId.HasValue && identity.FindFirst(CustomClaimTypes.BranchId) is null)
            identity.AddClaim(new Claim(CustomClaimTypes.BranchId, user.BranchId.Value.ToString()));

        if (user.CustomerId.HasValue && identity.FindFirst(CustomClaimTypes.CustomerId) is null)
            identity.AddClaim(new Claim(CustomClaimTypes.CustomerId, user.CustomerId.Value.ToString()));

        string displayName = string.Join(
            ' ',
            new[] { user.FirstName, user.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (!string.IsNullOrWhiteSpace(displayName) && identity.FindFirst(CustomClaimTypes.DisplayName) is null)
            identity.AddClaim(new Claim(CustomClaimTypes.DisplayName, displayName));
    }
}
