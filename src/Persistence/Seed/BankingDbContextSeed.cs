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
}
