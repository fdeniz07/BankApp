using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;

public static class ApprovalAuthorityEvaluator
{
    /// <summary>
    /// Returns null when unlimited; otherwise the max amount the actor may approve.
    /// </summary>
    public static decimal? ResolveMaxAmount(
        ApprovalAuthorityOptions options,
        bool isBankManager,
        bool isSuperAdmin,
        StaffPosition? position)
    {
        if (!options.Enabled)
            return null;

        if (IsUnlimited(options, isBankManager, isSuperAdmin))
            return null;

        if (position is null)
            return 0m;

        if (options.LimitsByPosition.TryGetValue(position.Name, out decimal limit))
            return limit;

        return 0m;
    }

    public static bool IsUnlimited(
        ApprovalAuthorityOptions options,
        bool isBankManager,
        bool isSuperAdmin)
    {
        foreach (string role in options.UnlimitedRoles ?? [])
        {
            if (string.Equals(role, "BankManager", StringComparison.OrdinalIgnoreCase) && isBankManager)
                return true;
            if (string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase) && isSuperAdmin)
                return true;
        }

        return false;
    }
}
