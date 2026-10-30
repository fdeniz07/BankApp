using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

public static class BranchSeed
{
    public static readonly (string Code, string Name, string City)[] DemoBranches =
    [
        ("IST-01", "İstanbul Kadıköy", "İstanbul"),
        ("ANK-01", "Ankara Kızılay", "Ankara"),
        ("BER-01", "Berlin Mitte", "Berlin"),
        ("NYC-01", "New York Manhattan", "New York"),
        ("CHI-01", "Chicago Loop", "Chicago")
    ];

    public static async Task SeedAsync(BankingDbContext context)
    {
        foreach (var demo in DemoBranches)
        {
            bool exists = await context.Branches.AnyAsync(b => b.Code == demo.Code && !b.IsDeleted);
            if (exists)
                continue;

            await context.Branches.AddAsync(new Branch
            {
                Code = demo.Code,
                Name = demo.Name,
                City = demo.City,
                IsActive = true
            });
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
