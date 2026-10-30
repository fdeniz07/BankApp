namespace Domain.Constants;

/// <summary>Demo / lab mapping: branch code → product catalog locale (tr|en|de).</summary>
public static class BranchLocaleMap
{
    public const string DemoPassword = "Demo123!";

    private static readonly Dictionary<string, string> LocaleByCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["IST-01"] = "tr",
        ["ANK-01"] = "tr",
        ["BER-01"] = "de",
        ["NYC-01"] = "en",
        ["CHI-01"] = "en"
    };

    public static string? ResolveLocale(string? branchCode)
    {
        if (string.IsNullOrWhiteSpace(branchCode))
            return null;

        return LocaleByCode.TryGetValue(branchCode.Trim(), out string? locale) ? locale : null;
    }

    public static string? ResolveBranchCodeForLocale(string locale) =>
        locale.Trim().ToLowerInvariant() switch
        {
            "tr" => "IST-01",
            "en" => "NYC-01",
            "de" => "BER-01",
            _ => null
        };
}
