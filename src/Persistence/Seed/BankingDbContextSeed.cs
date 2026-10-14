using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;

namespace Persistence.Seed;

public static class BankingDbContextSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        BankingDbContext context = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        await SeedIndividualCustomersAsync(context);
        await SeedCorporateCustomersAsync(context);
        await SeedCreditTypesAsync(context);
        await SeedCreditSubTypesAsync(context);
        await SeedCreditApplicationsAsync(context);
        await IdentitySeed.SeedAsync(context);
    }

    private static async Task SeedIndividualCustomersAsync(BankingDbContext context)
    {
        int existingCount = await context.IndividualCustomers.CountAsync();
        int missingCount = IndividualCustomerSeed.Count - existingCount;
        if (missingCount <= 0)
            return;

        await context.IndividualCustomers.AddRangeAsync(
            IndividualCustomerSeed.Generate(missingCount, existingCount));
        await context.SaveChangesAsync();
    }

    private static async Task SeedCorporateCustomersAsync(BankingDbContext context)
    {
        int existingCount = await context.CorporateCustomers.CountAsync();
        int missingCount = CorporateCustomerSeed.Count - existingCount;
        if (missingCount <= 0)
            return;

        await context.CorporateCustomers.AddRangeAsync(
            CorporateCustomerSeed.Generate(missingCount, existingCount));
        await context.SaveChangesAsync();
    }

    private static async Task SeedCreditTypesAsync(BankingDbContext context)
    {
        int existingCount = await context.CreditTypes.CountAsync();
        int missingCount = CreditTypeSeed.Count - existingCount;
        if (missingCount <= 0)
            return;

        await context.CreditTypes.AddRangeAsync(
            CreditTypeSeed.Generate(missingCount, existingCount));
        await context.SaveChangesAsync();
    }

    private static async Task SeedCreditSubTypesAsync(BankingDbContext context)
    {
        List<CreditType> creditTypes = await context.CreditTypes
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        List<CreditSubType> toAdd = [];

        foreach (CreditType creditType in creditTypes)
        {
            int existingCount = await context.CreditSubTypes
                .CountAsync(c => c.CreditTypeId == creditType.Id);
            int missingCount = CreditSubTypeSeed.SubTypesPerCreditType - existingCount;
            if (missingCount <= 0)
                continue;

            toAdd.AddRange(CreditSubTypeSeed.GenerateForCreditType(creditType, missingCount, existingCount));
        }

        if (toAdd.Count == 0)
            return;

        await context.CreditSubTypes.AddRangeAsync(toAdd);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCreditApplicationsAsync(BankingDbContext context)
    {
        int existingCount = await context.CreditApplications.CountAsync();
        int missingCount = CreditApplicationSeed.Count - existingCount;
        if (missingCount <= 0)
            return;

        List<IndividualCustomer> individuals = await context.IndividualCustomers
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .ToListAsync();
        List<CorporateCustomer> corporates = await context.CorporateCustomers
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        List<CreditType> creditTypes = await context.CreditTypes
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.IsActive)
            .ToListAsync();
        Dictionary<Guid, CreditCategory> categoryByTypeId = creditTypes.ToDictionary(c => c.Id, c => c.Category);

        List<CreditSubType> activeSubTypes = await context.CreditSubTypes
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.IsActive)
            .ToListAsync();

        List<(CreditSubType SubType, CreditCategory Category)> subTypes = activeSubTypes
            .Where(s => categoryByTypeId.ContainsKey(s.CreditTypeId))
            .Select(s => (s, categoryByTypeId[s.CreditTypeId]))
            .ToList();

        List<CreditApplication> applications = CreditApplicationSeed.Generate(
            missingCount,
            existingCount,
            individuals,
            corporates,
            subTypes);

        if (applications.Count == 0)
            return;

        await context.CreditApplications.AddRangeAsync(applications);
        await context.SaveChangesAsync();
    }
}
