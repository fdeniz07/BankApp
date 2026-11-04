using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;

public sealed class ApprovalAuthorityLimit
{
    /// <summary>Null = unlimited (disabled authority or unlimited role).</summary>
    public decimal? AppliedMax { get; init; }

    public decimal? PositionMax { get; init; }
    public decimal? BranchMax { get; init; }

    public string? PositionCurrencyCode { get; init; }
    public string? BranchCurrencyCode { get; init; }
    public string? AppliedCurrencyCode { get; init; }
}

public static class ApprovalAuthorityEvaluator
{
    public const string DefaultCurrencyCode = "TRY";

    public static string NormalizeCurrencyCode(string? code) =>
        string.IsNullOrWhiteSpace(code)
            ? DefaultCurrencyCode
            : code.Trim().ToUpperInvariant();

    /// <summary>
    /// Effective max = min(position, branch) among caps that match
    /// <paramref name="applicationCurrencyCode"/> (no FX). If DoA is enabled and
    /// no cap matches the application currency, AppliedMax = 0 (deny).
    /// Unlimited roles bypass AppliedMax.
    /// </summary>
    public static ApprovalAuthorityLimit Resolve(
        ApprovalAuthorityOptions options,
        bool isBankManager,
        bool isSuperAdmin,
        StaffPosition? position,
        decimal? branchMaxApprovalAmount,
        string? branchMaxApprovalCurrencyCode = null,
        string? applicationCurrencyCode = null)
    {
        if (!options.Enabled)
            return new ApprovalAuthorityLimit();

        string appCc = NormalizeCurrencyCode(applicationCurrencyCode);
        string positionCc = NormalizeCurrencyCode(options.CurrencyCode);

        decimal? rawBranchMax = branchMaxApprovalAmount is > 0 ? branchMaxApprovalAmount : null;
        string? branchCc = rawBranchMax.HasValue
            ? NormalizeCurrencyCode(branchMaxApprovalCurrencyCode)
            : null;

        if (IsUnlimited(options, isBankManager, isSuperAdmin))
            return new ApprovalAuthorityLimit
            {
                PositionMax = null,
                PositionCurrencyCode = null,
                BranchMax = rawBranchMax,
                BranchCurrencyCode = branchCc,
                AppliedMax = null,
                AppliedCurrencyCode = null
            };

        decimal? positionMax = null;
        if (string.Equals(appCc, positionCc, StringComparison.OrdinalIgnoreCase))
        {
            if (position is null)
                positionMax = 0m;
            else if (options.LimitsByPosition.TryGetValue(position.Name, out decimal limit))
                positionMax = limit;
            else
                positionMax = 0m;
        }

        decimal? branchMax = null;
        if (rawBranchMax.HasValue &&
            branchCc is not null &&
            string.Equals(appCc, branchCc, StringComparison.OrdinalIgnoreCase))
        {
            branchMax = rawBranchMax;
        }

        decimal? applied;
        if (!positionMax.HasValue && !branchMax.HasValue)
            applied = 0m;
        else if (positionMax.HasValue && branchMax.HasValue)
            applied = Math.Min(positionMax.Value, branchMax.Value);
        else
            applied = positionMax ?? branchMax;

        return new ApprovalAuthorityLimit
        {
            PositionMax = positionMax,
            PositionCurrencyCode = positionMax.HasValue ? positionCc : null,
            BranchMax = branchMax,
            BranchCurrencyCode = branchMax.HasValue ? branchCc : null,
            AppliedMax = applied,
            AppliedCurrencyCode = applied.HasValue ? appCc : null
        };
    }

    public static decimal? ResolveMaxAmount(
        ApprovalAuthorityOptions options,
        bool isBankManager,
        bool isSuperAdmin,
        StaffPosition? position,
        decimal? branchMaxApprovalAmount = null,
        string? branchMaxApprovalCurrencyCode = null,
        string? applicationCurrencyCode = null) =>
        Resolve(
            options,
            isBankManager,
            isSuperAdmin,
            position,
            branchMaxApprovalAmount,
            branchMaxApprovalCurrencyCode,
            applicationCurrencyCode).AppliedMax;

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
