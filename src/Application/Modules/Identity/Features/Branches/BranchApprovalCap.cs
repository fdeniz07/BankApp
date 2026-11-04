using Core.CrossCuttingConcerns.Exceptions.Types;

namespace Application.Modules.Identity.Features.Branches;

internal static class BranchApprovalCap
{
    private static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase) { "TRY", "EUR", "USD" };

    public static (decimal? Amount, string? CurrencyCode) Normalize(decimal? amount, string? currencyCode)
    {
        if (amount is not > 0)
            return (null, null);

        string code = string.IsNullOrWhiteSpace(currencyCode)
            ? "TRY"
            : currencyCode.Trim().ToUpperInvariant();

        if (!Allowed.Contains(code))
            throw new BusinessException($"Unsupported approval currency '{code}'. Use TRY, EUR, or USD.");

        return (amount, code);
    }
}
