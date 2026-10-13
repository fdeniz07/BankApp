using Domain.Entities;
using Domain.Enums;

namespace Persistence.Seed;

public static class CreditTypeSeed
{
    public const int Count = 6;

    private static readonly (string Name, string Code, CreditCategory Category, CreditRiskLevel RiskLevel, string Description, bool RequiresCollateral, bool RequiresGuarantor)[] Catalog =
    [
        ("İhtiyaç Kredisi", "CT-IND-001", CreditCategory.Individual, CreditRiskLevel.Medium, "Bireysel ihtiyaç kredisi ürünü", false, false),
        ("Konut Kredisi", "CT-IND-002", CreditCategory.Individual, CreditRiskLevel.Low, "Bireysel konut finansmanı", true, false),
        ("Taşıt Kredisi", "CT-IND-003", CreditCategory.Individual, CreditRiskLevel.Medium, "Bireysel taşıt finansmanı", true, false),
        ("KOBİ İşletme Kredisi", "CT-CORP-001", CreditCategory.Corporate, CreditRiskLevel.Medium, "KOBİ işletme sermayesi kredisi", false, true),
        ("Yatırım Kredisi", "CT-CORP-002", CreditCategory.Corporate, CreditRiskLevel.High, "Kurumsal yatırım finansmanı", true, true),
        ("Ticari Kredi", "CT-CORP-003", CreditCategory.Corporate, CreditRiskLevel.Medium, "Genel amaçlı ticari kredi", false, true)
    ];

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
                Category = definition.Category,
                RiskLevel = definition.RiskLevel,
                Description = definition.Description,
                CurrencyCode = "TRY",
                RequiresCollateral = definition.RequiresCollateral,
                RequiresGuarantor = definition.RequiresGuarantor,
                IsActive = true
            });
        }

        return items;
    }
}
