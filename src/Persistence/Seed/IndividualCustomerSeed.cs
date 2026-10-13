using Bogus;
using Domain.Entities;

namespace Persistence.Seed;

public static class IndividualCustomerSeed
{
    public const int Count = 20;

    public static List<IndividualCustomer> Generate(int count, int startIndex = 0)
    {
        if (count <= 0)
            return [];

        int sequence = startIndex;

        Faker<IndividualCustomer> faker = new Faker<IndividualCustomer>("tr")
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.CustomerNumber, _ => $"IND-{(++sequence):D6}")
            .RuleFor(c => c.Email, (f, c) => $"{c.FirstName}.{c.LastName}.{startIndex + f.IndexFaker}@individual.local".ToLowerInvariant())
            .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber("05#########"))
            .RuleFor(c => c.Address, f => f.Address.FullAddress())
            .RuleFor(c => c.IsActive, f => f.Random.Bool(0.85f))
            .RuleFor(c => c.NationalId, f => (10000000000L + startIndex + f.IndexFaker).ToString())
            .RuleFor(c => c.DateOfBirth, f => DateOnly.FromDateTime(f.Date.Past(40, DateTime.UtcNow.AddYears(-18))))
            .RuleFor(c => c.Gender, f => f.PickRandom("Male", "Female"))
            .RuleFor(c => c.MotherName, f => f.Name.FirstName(Bogus.DataSets.Name.Gender.Female))
            .RuleFor(c => c.FatherName, f => f.Name.FirstName(Bogus.DataSets.Name.Gender.Male));

        return faker.Generate(count);
    }
}
