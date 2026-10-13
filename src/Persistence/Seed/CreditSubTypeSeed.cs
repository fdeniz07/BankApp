using Bogus;
using Domain.Entities;
using Domain.Enums;

namespace Persistence.Seed;

public static class CreditSubTypeSeed
{
    public const int SubTypesPerCreditType = 2;

    private static readonly string[] IndividualSuffixes = ["Standart", "Avantajlı"];
    private static readonly string[] CorporateSuffixes = ["Kısa Vade", "Uzun Vade"];

    public static List<CreditSubType> GenerateForCreditType(CreditType creditType, int count, int startIndex = 0)
    {
        if (count <= 0)
            return [];

        bool isCorporate = creditType.Category == CreditCategory.Corporate;
        string[] suffixes = isCorporate ? CorporateSuffixes : IndividualSuffixes;

        int sequence = startIndex;

        Faker faker = new("tr");
        List<CreditSubType> items = [];

        for (int i = 0; i < count; i++)
        {
            int index = startIndex + i;
            string suffix = index < suffixes.Length
                ? suffixes[index]
                : faker.Commerce.ProductAdjective();

            decimal minAmount = isCorporate
                ? faker.Finance.Amount(100_000m, 500_000m, 2)
                : faker.Finance.Amount(10_000m, 50_000m, 2);
            decimal maxAmount = isCorporate
                ? faker.Finance.Amount(Math.Max(minAmount + 100_000m, 1_000_000m), 10_000_000m, 2)
                : faker.Finance.Amount(Math.Max(minAmount + 10_000m, 100_000m), 1_000_000m, 2);

            int minTerm = isCorporate ? faker.Random.Int(6, 12) : faker.Random.Int(3, 12);
            int maxTerm = isCorporate ? faker.Random.Int(24, 120) : faker.Random.Int(24, 60);

            items.Add(new CreditSubType
            {
                CreditTypeId = creditType.Id,
                Name = $"{creditType.Name} {suffix}",
                Code = $"{creditType.Code}-ST-{(++sequence):D2}",
                Description = $"{creditType.Name} için {suffix.ToLowerInvariant()} paket",
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                InterestRate = Math.Round(faker.Random.Decimal(1.5m, 4.5m), 4),
                MinTermMonths = minTerm,
                MaxTermMonths = Math.Max(maxTerm, minTerm + 6),
                LateFeeRate = Math.Round(faker.Random.Decimal(0.1m, 1.0m), 4),
                EarlyPaymentFeeRate = Math.Round(faker.Random.Decimal(0.5m, 2.0m), 4),
                ProcessingFeeAmount = faker.Finance.Amount(100m, 2500m, 2),
                ProcessingFeeRate = Math.Round(faker.Random.Decimal(0.1m, 1.0m), 4),
                GracePeriodDays = faker.Random.Int(0, 30),
                RequiresCollateral = creditType.RequiresCollateral || faker.Random.Bool(0.3f),
                RequiresIncomeDocument = !isCorporate || faker.Random.Bool(0.7f),
                IsActive = true
            });
        }

        return items;
    }
}
