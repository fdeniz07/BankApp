using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

/// <summary>
/// Backfills Customer.BranchId from locale markers (and a few corporates) without wiping data.
/// </summary>
public static class CustomerBranchAssignmentSeed
{
    private static readonly Dictionary<string, string> LocaleToBranchCode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tr"] = "IST-01",
        ["en"] = "NYC-01",
        ["de"] = "BER-01"
    };

    public static async Task SeedAsync(BankingDbContext context)
    {
        Dictionary<string, Guid> branchIdByCode = await context.Branches
            .Where(b => !b.IsDeleted)
            .ToDictionaryAsync(b => b.Code, b => b.Id);

        if (branchIdByCode.Count == 0)
            return;

        List<IndividualCustomer> individuals = await context.IndividualCustomers
            .Where(c => !c.IsDeleted && c.BranchId == null)
            .ToListAsync();

        bool changed = false;
        foreach (IndividualCustomer customer in individuals)
        {
            string locale = ResolveLocaleFromEmail(customer.Email, "@individual.");
            if (!LocaleToBranchCode.TryGetValue(locale, out string? code))
                continue;
            if (!branchIdByCode.TryGetValue(code, out Guid branchId))
                continue;

            customer.BranchId = branchId;
            changed = true;
        }

        List<CorporateCustomer> corporates = await context.CorporateCustomers
            .Where(c => !c.IsDeleted && c.BranchId == null)
            .ToListAsync();

        int corporateIndex = 0;
        string[] corporateBranchCodes = ["IST-01", "ANK-01", "BER-01", "NYC-01", "CHI-01"];
        foreach (CorporateCustomer customer in corporates)
        {
            string locale = ResolveLocaleFromEmail(customer.Email, "@corporate.");
            string code = LocaleToBranchCode.TryGetValue(locale, out string? localeCode)
                ? localeCode
                : corporateBranchCodes[corporateIndex % corporateBranchCodes.Length];
            corporateIndex++;

            // Spread EN corporates across NYC + CHI
            if (locale == "en" && corporateIndex % 2 == 0 && branchIdByCode.ContainsKey("CHI-01"))
                code = "CHI-01";

            if (!branchIdByCode.TryGetValue(code, out Guid branchId))
                continue;

            customer.BranchId = branchId;
            changed = true;
        }

        if (changed)
            await context.SaveChangesAsync();
    }

    private static string ResolveLocaleFromEmail(string email, string marker)
    {
        // e.g. name@individual.en.local
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
