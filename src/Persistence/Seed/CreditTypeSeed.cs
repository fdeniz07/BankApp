using Domain.Entities;
using Domain.Enums;

namespace Persistence.Seed;

public static class CreditTypeSeed
{
    private static readonly (string Locale, string Name, string Code, CreditCategory Category, CreditRiskLevel RiskLevel, string Description, string CurrencyCode, bool RequiresCollateral, bool RequiresGuarantor)[] Catalog =
    [
        // TR → TRY
        ("tr", "İhtiyaç Kredisi", "CT-TR-IND-001", CreditCategory.Individual, CreditRiskLevel.Medium, "Bireysel ihtiyaç finansmanı ürünü", "TRY", false, false),
        ("tr", "Konut Kredisi", "CT-TR-IND-002", CreditCategory.Individual, CreditRiskLevel.Low, "Bireysel konut finansmanı ürünü", "TRY", true, false),
        ("tr", "Taşıt Kredisi", "CT-TR-IND-003", CreditCategory.Individual, CreditRiskLevel.Medium, "Bireysel taşıt finansmanı ürünü", "TRY", true, false),
        ("tr", "KOBİ İşletme Kredisi", "CT-TR-CORP-001", CreditCategory.Corporate, CreditRiskLevel.Medium, "KOBİ işletme sermayesi kredisi", "TRY", false, true),
        ("tr", "Yatırım Kredisi", "CT-TR-CORP-002", CreditCategory.Corporate, CreditRiskLevel.High, "Kurumsal yatırım finansmanı", "TRY", true, true),
        ("tr", "Ticari Kredi", "CT-TR-CORP-003", CreditCategory.Corporate, CreditRiskLevel.Medium, "Genel amaçlı ticari kredi", "TRY", false, true),
        // EN → USD
        ("en", "Personal Loan", "CT-EN-IND-001", CreditCategory.Individual, CreditRiskLevel.Medium, "Unsecured personal financing product", "USD", false, false),
        ("en", "Home Mortgage", "CT-EN-IND-002", CreditCategory.Individual, CreditRiskLevel.Low, "Residential mortgage financing", "USD", true, false),
        ("en", "Auto Loan", "CT-EN-IND-003", CreditCategory.Individual, CreditRiskLevel.Medium, "Vehicle financing for individuals", "USD", true, false),
        ("en", "SME Working Capital", "CT-EN-CORP-001", CreditCategory.Corporate, CreditRiskLevel.Medium, "Short-term SME working capital", "USD", false, true),
        ("en", "Corporate Investment Loan", "CT-EN-CORP-002", CreditCategory.Corporate, CreditRiskLevel.High, "Long-term corporate investment financing", "USD", true, true),
        // DE → EUR
        ("de", "Privatkredit", "CT-DE-IND-001", CreditCategory.Individual, CreditRiskLevel.Medium, "Unbesicherter Privatkredit", "EUR", false, false),
        ("de", "Immobilienkredit", "CT-DE-IND-002", CreditCategory.Individual, CreditRiskLevel.Low, "Finanzierung für Wohneigentum", "EUR", true, false),
        ("de", "Autokredit", "CT-DE-IND-003", CreditCategory.Individual, CreditRiskLevel.Medium, "Fahrzeugfinanzierung für Privatkunden", "EUR", true, false),
        ("de", "KMU Betriebsmittelkredit", "CT-DE-CORP-001", CreditCategory.Corporate, CreditRiskLevel.Medium, "Betriebsmittelfinanzierung für KMU", "EUR", false, true),
        ("de", "Unternehmenskredit", "CT-DE-CORP-002", CreditCategory.Corporate, CreditRiskLevel.High, "Investitionskredit für Unternehmen", "EUR", true, true)
    ];

    public static int Count => Catalog.Length;

    public static List<CreditType> Generate(int count, int startIndex = 0)
    {
        if (count <= 0 || startIndex >= Catalog.Length)
            return [];

        int take = Math.Min(count, Catalog.Length - startIndex);
        List<CreditType> items = [];

        for (int i = 0; i < take; i++)
        {
            var definition = Catalog[startIndex + i];
            items.Add(new CreditType
            {
                Name = definition.Name,
                Code = definition.Code,
                Locale = definition.Locale,
                Category = definition.Category,
                RiskLevel = definition.RiskLevel,
                Description = definition.Description,
                CurrencyCode = definition.CurrencyCode,
                RequiresCollateral = definition.RequiresCollateral,
                RequiresGuarantor = definition.RequiresGuarantor,
                IsActive = true
            });
        }

        return items;
    }

    public static IReadOnlyDictionary<string, string> CurrencyByCode { get; } =
        Catalog.ToDictionary(c => c.Code, c => c.CurrencyCode, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyDictionary<string, string> LocaleByCode { get; } =
        Catalog.ToDictionary(c => c.Code, c => c.Locale, StringComparer.OrdinalIgnoreCase);
}
