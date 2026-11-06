using Domain.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

/// <summary>
/// Backfills Customer.BranchId for rows still null (legacy locale-only seeds).
/// New customers are created with BranchId in BankingDbContextSeed.
/// </summary>
public static class CustomerBranchAssignmentSeed
{
    public static async Task SeedAsync(BankingDbContext context)
    {
        Dictionary<string, Guid> branchIdByCode = await context.Branches
            .Where(b => !b.IsDeleted)
            .ToDictionaryAsync(b => b.Code, b => b.Id);

        if (branchIdByCode.Count == 0)
            return;

        bool changed = false;

        List<IndividualCustomer> individuals = await context.IndividualCustomers
            .Where(c => !c.IsDeleted && c.BranchId == null)
            .ToListAsync();

        foreach (IndividualCustomer customer in individuals)
        {
            string locale = ResolveLocaleFromEmail(customer.Email, "@individual.");
            string? code = BranchLocaleMap.ResolveBranchCodeForLocale(locale);
            if (code is null || !branchIdByCode.TryGetValue(code, out Guid branchId))
                continue;

            customer.BranchId = branchId;
            changed = true;
        }

        List<CorporateCustomer> corporates = await context.CorporateCustomers
            .Where(c => !c.IsDeleted && c.BranchId == null)
            .ToListAsync();

        int corporateIndex = 0;
        string[] allCodes = BranchSeed.DemoBranches.Select(b => b.Code).ToArray();
        foreach (CorporateCustomer customer in corporates)
        {
            string locale = ResolveLocaleFromEmail(customer.Email, "@corporate.");
            string? code = BranchLocaleMap.ResolveBranchCodeForLocale(locale);

            // Spread same-locale corporates across sibling branches (IST↔ANK, NYC↔CHI).
            if (locale == "tr")
                code = corporateIndex % 2 == 0 ? "IST-01" : "ANK-01";
            else if (locale == "en")
                code = corporateIndex % 2 == 0 ? "NYC-01" : "CHI-01";
            else if (string.IsNullOrWhiteSpace(code))
                code = allCodes[corporateIndex % allCodes.Length];

            corporateIndex++;

            if (code is null || !branchIdByCode.TryGetValue(code, out Guid branchId))
                continue;

            customer.BranchId = branchId;
            changed = true;
        }

        if (changed)
            await context.SaveChangesAsync();
    }

    private static string ResolveLocaleFromEmail(string email, string marker)
    {
        int at = email.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (at < 0)
            return "tr";

        string rest = email[(at + marker.Length)..];
        int dot = rest.IndexOf('.');
        if (dot <= 0)
            return "tr";

        return rest[..dot].ToLowerInvariant();
    }
}
