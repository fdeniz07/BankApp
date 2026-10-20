namespace Application.Common.CustomerNumbers;

public static class CustomerNumberGenerator
{
    public static string NormalizeLocale(string? locale)
    {
        string code = (locale ?? "tr").Trim().ToLowerInvariant();
        return code switch
        {
            "en" => "EN",
            "de" => "DE",
            _ => "TR"
        };
    }

    public static string NextIndividual(string? locale = null) =>
        $"IND-{NormalizeLocale(locale)}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    public static string NextCorporate(string? locale = null) =>
        $"CORP-{NormalizeLocale(locale)}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
