using Bogus;
using Domain.Entities;

namespace Persistence.Seed;

public static class CorporateCustomerSeed
{
    public const int Count = 20;

    public static List<CorporateCustomer> Generate(int count, int startIndex = 0)
    {
        if (count <= 0)
            return [];

        int sequence = startIndex;

        Faker<CorporateCustomer> faker = new Faker<CorporateCustomer>("tr")
            .RuleFor(c => c.CompanyName, f => f.Company.CompanyName())
            .RuleFor(c => c.CustomerNumber, _ => $"CORP-{(++sequence):D6}")
            .RuleFor(c => c.Email, f => $"corp{startIndex + f.IndexFaker}@corporate.local")
            .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber("05#########"))
            .RuleFor(c => c.Address, f => f.Address.FullAddress())
            .RuleFor(c => c.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(c => c.TaxNumber, f => (2000000000L + startIndex + f.IndexFaker).ToString())
            .RuleFor(c => c.TaxOffice, f => f.Address.City() + " Vergi Dairesi")
            .RuleFor(c => c.CompanyRegistrationNumber, f => $"CRN-{(startIndex + f.IndexFaker + 1):D8}")
            .RuleFor(c => c.CompanyEmail, f => $"info{startIndex + f.IndexFaker}@company.local")
            .RuleFor(c => c.AuthorizedPersonName, f => f.Name.FullName())
            .RuleFor(c => c.CompanyAddress, f => f.Address.FullAddress())
            .RuleFor(c => c.CompanyType, f => f.PickRandom("A.Ş.", "Ltd. Şti.", "Kooperatif"))
            .RuleFor(c => c.CompanySector, f => f.Commerce.Department())
            .RuleFor(c => c.CompanyIndustry, f => f.Commerce.Categories(1)[0])
            .RuleFor(c => c.CompanySubIndustry, f => f.Commerce.ProductAdjective())
            .RuleFor(c => c.CompanyFoundationDate, f => f.Date.Past(30));

        return faker.Generate(count);
    }
}
