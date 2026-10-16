using Bogus;
using Domain.Entities;

namespace Persistence.Seed;

public static class CorporateCustomerSeed
{
    public const int CountPerLocale = 10;
    public static readonly string[] Locales = ["tr", "en", "de"];
    public static int Count => CountPerLocale * Locales.Length;

    public static List<CorporateCustomer> GenerateForLocale(string locale, int count, int startSequence)
    {
        if (count <= 0)
            return [];

        List<CorporateCustomer> items = [];
        for (int i = 0; i < count; i++)
            items.Add(CreateOne(locale, startSequence + i));
        return items;
    }

    private static CorporateCustomer CreateOne(string locale, int sequence)
    {
        Faker faker = new(locale);

        return new CorporateCustomer
        {
            CompanyName = faker.Company.CompanyName(),
            CustomerNumber = $"CORP-{locale.ToUpperInvariant()}-{(sequence + 1):D4}",
            Email = $"corp{sequence}@corporate.{locale}.local",
            PhoneNumber = locale switch
            {
                "de" => faker.Phone.PhoneNumber("+49 3## #######"),
                "en" => faker.Phone.PhoneNumber("+1 ###-###-####"),
                _ => faker.Phone.PhoneNumber("05#########")
            },
            Address = faker.Address.FullAddress(),
            IsActive = faker.Random.Bool(0.9f),
            TaxNumber = (2000000000L + sequence).ToString(),
            TaxOffice = locale switch
            {
                "de" => faker.Address.City() + " Finanzamt",
                "en" => faker.Address.City() + " Tax Office",
                _ => faker.Address.City() + " Vergi Dairesi"
            },
            CompanyRegistrationNumber = $"CRN-{(sequence + 1):D8}",
            CompanyEmail = $"info{sequence}@company.{locale}.local",
            AuthorizedPersonName = faker.Name.FullName(),
            CompanyAddress = faker.Address.FullAddress(),
            CompanyType = locale switch
            {
                "de" => faker.PickRandom("GmbH", "AG", "UG"),
                "en" => faker.PickRandom("LLC", "Inc.", "Ltd."),
                _ => faker.PickRandom("A.Ş.", "Ltd. Şti.", "Kooperatif")
            },
            CompanySector = faker.Commerce.Department(),
            CompanyIndustry = faker.Commerce.Categories(1)[0],
            CompanySubIndustry = faker.Commerce.ProductAdjective(),
            CompanyFoundationDate = faker.Date.Past(30)
        };
    }
}
