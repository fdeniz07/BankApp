using Bogus;
using Domain.Entities;
using Domain.Enums;

namespace Persistence.Seed;

public static class CreditApplicationSeed
{
    public const int Count = 48;

    private static readonly Dictionary<string, string[]> PurposesByLocale = new()
    {
        ["tr"] = ["Konut alımı", "Araç alımı", "İhtiyaç", "İşletme sermayesi", "Yatırım", "Borç yapılandırma"],
        ["en"] = ["Home purchase", "Vehicle purchase", "Personal expenses", "Working capital", "Investment", "Debt restructuring"],
        ["de"] = ["Immobilienkauf", "Fahrzeugkauf", "Privatbedarf", "Betriebsmittel", "Investition", "Umschuldung"]
    };

    private static readonly Dictionary<string, string[]> RejectionReasonsByLocale = new()
    {
        ["tr"] = ["Yetersiz gelir", "Kredi notu uygun değil", "Eksik belge", "Mevcut borç yükü yüksek"],
        ["en"] = ["Insufficient income", "Credit score not acceptable", "Missing documents", "Existing debt too high"],
        ["de"] = ["Unzureichendes Einkommen", "Bonität nicht ausreichend", "Fehlende Unterlagen", "Bestehende Verschuldung zu hoch"]
    };

    public static List<CreditApplication> Generate(
        int count,
        int startIndex,
        IReadOnlyList<IndividualCustomer> individualCustomers,
        IReadOnlyList<CorporateCustomer> corporateCustomers,
        IReadOnlyList<(CreditSubType SubType, CreditCategory Category, string CurrencyCode, string Locale)> subTypes)
    {
        if (count <= 0)
            return [];

        List<(CreditSubType SubType, CreditCategory Category, string CurrencyCode, string Locale)> individualSubTypes =
            subTypes.Where(s => s.Category == CreditCategory.Individual).ToList();
        List<(CreditSubType SubType, CreditCategory Category, string CurrencyCode, string Locale)> corporateSubTypes =
            subTypes.Where(s => s.Category == CreditCategory.Corporate).ToList();

        string[] locales = ["tr", "en", "de"];
        int sequence = startIndex;
        Random random = Random.Shared;
        List<CreditApplication> items = [];

        for (int i = 0; i < count; i++)
        {
            string locale = locales[i % locales.Length];
            Faker faker = new(locale);

            List<IndividualCustomer> localeIndividuals = activeByLocale(individualCustomers, locale, "@individual.");
            List<CorporateCustomer> localeCorporates = activeByLocale(corporateCustomers, locale, "@corporate.");

            var localeIndividual = individualSubTypes.Where(s => s.Locale == locale).ToList();
            var localeCorporate = corporateSubTypes.Where(s => s.Locale == locale).ToList();

            bool preferIndividual = faker.Random.Bool(0.55f);
            bool useIndividual =
                preferIndividual
                    ? localeIndividuals.Count > 0 && localeIndividual.Count > 0
                    : !(localeCorporates.Count > 0 && localeCorporate.Count > 0)
                      && localeIndividuals.Count > 0
                      && localeIndividual.Count > 0;

            if (!useIndividual && (localeCorporates.Count == 0 || localeCorporate.Count == 0))
            {
                if (localeIndividuals.Count == 0 || localeIndividual.Count == 0)
                    continue;
                useIndividual = true;
            }

            Guid customerId;
            Guid? branchId;
            CreditSubType subType;
            string currencyCode;

            if (useIndividual)
            {
                IndividualCustomer customer = faker.PickRandom(localeIndividuals);
                customerId = customer.Id;
                branchId = customer.BranchId;
                var picked = faker.PickRandom(localeIndividual);
                subType = picked.SubType;
                currencyCode = picked.CurrencyCode;
            }
            else
            {
                CorporateCustomer customer = faker.PickRandom(localeCorporates);
                customerId = customer.Id;
                branchId = customer.BranchId;
                var picked = faker.PickRandom(localeCorporate);
                subType = picked.SubType;
                currencyCode = picked.CurrencyCode;
            }

            CreditApplicationStatus status = faker.PickRandom(
                CreditApplicationStatus.Pending,
                CreditApplicationStatus.Approved,
                CreditApplicationStatus.Rejected,
                CreditApplicationStatus.Cancelled);

            DateTimeOffset applicationDate = faker.Date.RecentOffset(90);
            decimal requestedAmount = CreditSubTypeSeed.PickRoundAmount(
                random, subType.MinAmount, subType.MaxAmount, currencyCode);
            int requestedTerm = PickAllowedTerm(random, subType.MinTermMonths, subType.MaxTermMonths);

            CreditApplication application = new()
            {
                ApplicationNumber = $"APP-{(++sequence):D8}",
                CustomerId = customerId,
                BranchId = branchId,
                CreditSubTypeId = subType.Id,
                RequestedAmount = requestedAmount,
                RequestedTermMonths = requestedTerm,
                MonthlyIncome = CreditSubTypeSeed.RoundAmount(
                    faker.Finance.Amount(
                        Math.Max(requestedAmount * 0.08m, 5_000m),
                        Math.Max(requestedAmount * 0.35m, 8_000m),
                        0),
                    currencyCode),
                ExistingDebtAmount = faker.Random.Bool(0.6f)
                    ? CreditSubTypeSeed.RoundAmount(
                        faker.Finance.Amount(0m, Math.Max(requestedAmount * 0.25m, 1_000m), 0),
                        currencyCode)
                    : null,
                Purpose = faker.PickRandom(PurposesByLocale[locale]),
                Status = status,
                ApplicationDate = applicationDate,
                Notes = faker.Random.Bool(0.4f) ? faker.Lorem.Sentence() : null
            };

            if (status == CreditApplicationStatus.Approved)
            {
                application.DecisionDate = applicationDate.AddDays(faker.Random.Int(1, 14));
                application.ApprovedAmount = CreditSubTypeSeed.PickRoundAmount(
                    random, subType.MinAmount, requestedAmount, currencyCode);
                application.ApprovedTermMonths = PickApprovedTerm(
                    random, subType.MinTermMonths, requestedTerm);
                application.ApprovedInterestRate = subType.InterestRate;
                application.DecisionBy = faker.Name.FullName();
            }
            else if (status == CreditApplicationStatus.Rejected)
            {
                application.DecisionDate = applicationDate.AddDays(faker.Random.Int(1, 10));
                application.RejectionReason = faker.PickRandom(RejectionReasonsByLocale[locale]);
                application.DecisionBy = faker.Name.FullName();
            }
            else if (status == CreditApplicationStatus.Cancelled)
            {
                application.DecisionDate = applicationDate.AddDays(faker.Random.Int(0, 7));
                application.Notes ??= locale switch
                {
                    "en" => "Cancelled by customer",
                    "de" => "Vom Kunden storniert",
                    _ => "Müşteri tarafından iptal edildi"
                };
            }

            items.Add(application);
        }

        return items;
    }

    private static List<T> activeByLocale<T>(IReadOnlyList<T> customers, string locale, string marker)
        where T : Customer =>
        customers
            .Where(c => c.IsActive && c.Email.Contains($"{marker}{locale}.", StringComparison.OrdinalIgnoreCase))
            .ToList();

    public static int PickAllowedTerm(Random random, int minTerm, int maxTerm)
    {
        int[] candidates = CreditSubTypeSeed.GetAllowedTermsInRange(minTerm, maxTerm);
        if (candidates.Length == 0)
            return CreditSubTypeSeed.SnapTermMonths(minTerm, minTerm, maxTerm);

        return candidates[random.Next(candidates.Length)];
    }

    public static int PickApprovedTerm(Random random, int minTerm, int requestedTerm)
    {
        int[] candidates = CreditSubTypeSeed.GetAllowedTermsInRange(minTerm, requestedTerm);
        if (candidates.Length == 0)
            return CreditSubTypeSeed.SnapTermMonths(requestedTerm, minTerm, requestedTerm);

        int requestedIndex = Array.IndexOf(candidates, requestedTerm);
        if (requestedIndex < 0)
            requestedIndex = candidates.Length - 1;

        if (random.NextDouble() < 0.9 || requestedIndex == 0)
            return candidates[requestedIndex];

        return candidates[requestedIndex - 1];
    }
}
