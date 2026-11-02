using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;

public sealed class ApprovalAuthorityLimit
{
    /// <summary>Null = unlimited (disabled authority or unlimited role).</summary>
    public decimal? AppliedMax { get; init; }

    public decimal? PositionMax { get; init; }
    public decimal? BranchMax { get; init; }
}

public static class ApprovalAuthorityEvaluator
{
    /// <summary>
    /// Effective max = min(position, branch) among values that are set.
    /// Unlimited roles bypass both position and branch caps.
    /// </summary>
    public static ApprovalAuthorityLimit Resolve(
        ApprovalAuthorityOptions options,
        bool isBankManager,
        bool isSuperAdmin,
        StaffPosition? position,
        decimal? branchMaxApprovalAmount)
    {
        if (!options.Enabled)
            return new ApprovalAuthorityLimit();

        decimal? branchMax = branchMaxApprovalAmount is > 0 ? branchMaxApprovalAmount : null;

        if (IsUnlimited(options, isBankManager, isSuperAdmin))
            return new ApprovalAuthorityLimit
            {
                PositionMax = null,
                BranchMax = branchMax,
                AppliedMax = null
            };

        decimal? positionMax = null;
        if (position is null)
            positionMax = 0m;
        else if (options.LimitsByPosition.TryGetValue(position.Name, out decimal limit))
            positionMax = limit;
        else
            positionMax = 0m;

        decimal? applied = positionMax;
        if (branchMax.HasValue)
            applied = applied.HasValue ? Math.Min(applied.Value, branchMax.Value) : branchMax;

        return new ApprovalAuthorityLimit
        {
            PositionMax = positionMax,
            BranchMax = branchMax,
            AppliedMax = applied
        };
    }

    /// <summary>Returns null when unlimited; otherwise the max amount the actor may approve.</summary>
    public static decimal? ResolveMaxAmount(
        ApprovalAuthorityOptions options,
        bool isBankManager,
        bool isSuperAdmin,
        StaffPosition? position,
        decimal? branchMaxApprovalAmount = null) =>
        Resolve(options, isBankManager, isSuperAdmin, position, branchMaxApprovalAmount).AppliedMax;

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
