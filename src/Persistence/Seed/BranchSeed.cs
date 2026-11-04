using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

public static class BranchSeed
{
    public static readonly (string Code, string Name, string City, decimal MaxApprovalAmount, string Currency)[] DemoBranches =
    [
        ("IST-01", "İstanbul Kadıköy", "İstanbul", 200_000m, "TRY"),
        ("ANK-01", "Ankara Kızılay", "Ankara", 250_000m, "TRY"),
        ("BER-01", "Berlin Mitte", "Berlin", 180_000m, "EUR"),
        ("NYC-01", "New York Manhattan", "New York", 150_000m, "USD"),
        ("CHI-01", "Chicago Loop", "Chicago", 120_000m, "USD")
    ];

    public static async Task SeedAsync(BankingDbContext context)
    {
        foreach (var demo in DemoBranches)
        {
            Branch? existing = await context.Branches.FirstOrDefaultAsync(b => b.Code == demo.Code && !b.IsDeleted);
            if (existing is null)
            {
                await context.Branches.AddAsync(new Branch
                {
                    Code = demo.Code,
                    Name = demo.Name,
                    City = demo.City,
                    IsActive = true,
                    MaxApprovalAmount = demo.MaxApprovalAmount,
                    MaxApprovalCurrencyCode = demo.Currency
                });
                continue;
            }

            bool dirty = false;
            if (!existing.MaxApprovalAmount.HasValue)
            {
                existing.MaxApprovalAmount = demo.MaxApprovalAmount;
                dirty = true;
            }

            if (existing.MaxApprovalAmount.HasValue &&
                string.IsNullOrWhiteSpace(existing.MaxApprovalCurrencyCode))
            {
                existing.MaxApprovalCurrencyCode = demo.Currency;
                dirty = true;
            }

            if (dirty)
                context.Branches.Update(existing);
        }

        await context.SaveChangesAsync();
    }

    public static async Task<Guid?> ResolveIdByCodeAsync(BankingDbContext context, string code)
    {
        return await context.Branches
            .Where(b => b.Code == code && !b.IsDeleted)
            .Select(b => (Guid?)b.Id)
            .FirstOrDefaultAsync();
    }
}
