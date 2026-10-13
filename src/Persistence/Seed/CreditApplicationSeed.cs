using Bogus;
using Domain.Entities;
using Domain.Enums;

namespace Persistence.Seed;

public static class CreditApplicationSeed
{
    public const int Count = 30;

    public static List<CreditApplication> Generate(
        int count,
        int startIndex,
        IReadOnlyList<IndividualCustomer> individualCustomers,
        IReadOnlyList<CorporateCustomer> corporateCustomers,
        IReadOnlyList<(CreditSubType SubType, CreditCategory Category)> subTypes)
    {
        if (count <= 0)
            return [];

        List<(CreditSubType SubType, CreditCategory Category)> individualSubTypes =
            subTypes.Where(s => s.Category == CreditCategory.Individual).ToList();
        List<(CreditSubType SubType, CreditCategory Category)> corporateSubTypes =
            subTypes.Where(s => s.Category == CreditCategory.Corporate).ToList();

        List<IndividualCustomer> activeIndividuals = individualCustomers.Where(c => c.IsActive).ToList();
        List<CorporateCustomer> activeCorporates = corporateCustomers.Where(c => c.IsActive).ToList();

        if ((activeIndividuals.Count == 0 || individualSubTypes.Count == 0) &&
            (activeCorporates.Count == 0 || corporateSubTypes.Count == 0))
            return [];

        Faker faker = new("tr");
        int sequence = startIndex;
        List<CreditApplication> items = [];

        for (int i = 0; i < count; i++)
        {
            bool preferIndividual = faker.Random.Bool(0.55f);
            bool useIndividual =
                preferIndividual
                    ? activeIndividuals.Count > 0 && individualSubTypes.Count > 0
                    : !(activeCorporates.Count > 0 && corporateSubTypes.Count > 0)
                      && activeIndividuals.Count > 0
                      && individualSubTypes.Count > 0;

            if (!useIndividual && (activeCorporates.Count == 0 || corporateSubTypes.Count == 0))
            {
                if (activeIndividuals.Count == 0 || individualSubTypes.Count == 0)
                    break;
                useIndividual = true;
            }

            Guid customerId;
            CreditSubType subType;

            if (useIndividual)
            {
                customerId = faker.PickRandom(activeIndividuals).Id;
                subType = faker.PickRandom(individualSubTypes).SubType;
            }
            else
            {
                customerId = faker.PickRandom(activeCorporates).Id;
                subType = faker.PickRandom(corporateSubTypes).SubType;
            }

            CreditApplicationStatus status = faker.PickRandom(
                CreditApplicationStatus.Pending,
                CreditApplicationStatus.Approved,
                CreditApplicationStatus.Rejected,
                CreditApplicationStatus.Cancelled);

            DateTimeOffset applicationDate = faker.Date.RecentOffset(90);
            decimal requestedAmount = faker.Finance.Amount(subType.MinAmount, subType.MaxAmount, 2);
            int requestedTerm = faker.Random.Int(subType.MinTermMonths, subType.MaxTermMonths);

            CreditApplication application = new()
            {
                ApplicationNumber = $"APP-{(++sequence):D8}",
                CustomerId = customerId,
                CreditSubTypeId = subType.Id,
                RequestedAmount = requestedAmount,
                RequestedTermMonths = requestedTerm,
                MonthlyIncome = faker.Finance.Amount(15_000m, 250_000m, 2),
                ExistingDebtAmount = faker.Random.Bool(0.6f)
                    ? faker.Finance.Amount(0m, 150_000m, 2)
                    : null,
                Purpose = faker.PickRandom(
                    "Konut alımı",
                    "Araç alımı",
                    "İhtiyaç",
                    "İşletme sermayesi",
                    "Yatırım",
                    "Borç yapılandırma"),
                Status = status,
                ApplicationDate = applicationDate,
                Notes = faker.Random.Bool(0.4f) ? faker.Lorem.Sentence() : null
            };

            if (status == CreditApplicationStatus.Approved)
            {
                application.DecisionDate = applicationDate.AddDays(faker.Random.Int(1, 14));
                application.ApprovedAmount = faker.Finance.Amount(subType.MinAmount, requestedAmount, 2);
                application.ApprovedTermMonths = faker.Random.Int(subType.MinTermMonths, requestedTerm);
                application.ApprovedInterestRate = subType.InterestRate;
                application.DecisionBy = faker.Name.FullName();
            }
            else if (status == CreditApplicationStatus.Rejected)
            {
                application.DecisionDate = applicationDate.AddDays(faker.Random.Int(1, 10));
                application.RejectionReason = faker.PickRandom(
                    "Yetersiz gelir",
                    "Kredi notu uygun değil",
                    "Eksik belge",
                    "Mevcut borç yükü yüksek");
                application.DecisionBy = faker.Name.FullName();
            }
            else if (status == CreditApplicationStatus.Cancelled)
            {
                application.DecisionDate = applicationDate.AddDays(faker.Random.Int(0, 7));
                application.Notes ??= "Müşteri tarafından iptal edildi";
            }

            items.Add(application);
        }

        return items;
    }
}
