using Bogus;
using Domain.Entities;

namespace Persistence.Seed;

public static class IndividualCustomerSeed
{
    public const int CountPerLocale = 10;
    public static readonly string[] Locales = ["tr", "en", "de"];
    public static int Count => CountPerLocale * Locales.Length;

    public static List<IndividualCustomer> GenerateForLocale(string locale, int count, int startSequence)
    {
        if (count <= 0)
            return [];

        List<IndividualCustomer> items = [];
        for (int i = 0; i < count; i++)
            items.Add(CreateOne(locale, startSequence + i));
        return items;
    }

    private static IndividualCustomer CreateOne(string locale, int sequence)
    {
        Faker faker = new(locale);
        string firstName = faker.Name.FirstName();
        string lastName = faker.Name.LastName();

        return new IndividualCustomer
        {
            FirstName = firstName,
            LastName = lastName,
            CustomerNumber = $"IND-{locale.ToUpperInvariant()}-{(sequence + 1):D4}",
            Email = $"{firstName}.{lastName}.{sequence}@individual.{locale}.local".ToLowerInvariant(),
            PhoneNumber = locale switch
            {
                "de" => faker.Phone.PhoneNumber("+49 1## #######"),
                "en" => faker.Phone.PhoneNumber("+1 ###-###-####"),
                _ => faker.Phone.PhoneNumber("05#########")
            },
            Address = faker.Address.FullAddress(),
            IsActive = faker.Random.Bool(0.9f),
            NationalId = (10000000000L + sequence).ToString(),
            DateOfBirth = DateOnly.FromDateTime(faker.Date.Past(40, DateTime.UtcNow.AddYears(-18))),
            Gender = faker.PickRandom("Male", "Female"),
            MotherName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female),
            FatherName = faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male)
        };
    }
}
