using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

/// <summary>Copies Customer.BranchId onto CreditApplication.BranchId when missing.</summary>
public static class ApplicationBranchAssignmentSeed
{
    public static async Task SeedAsync(BankingDbContext context)
    {
        List<CreditApplication> applications = await context.CreditApplications
            .Where(a => !a.IsDeleted && a.BranchId == null)
            .ToListAsync();

        if (applications.Count == 0)
            return;

        HashSet<Guid> customerIds = applications.Select(a => a.CustomerId).ToHashSet();
        Dictionary<Guid, Guid?> branchByCustomer = await context.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.BranchId);

        bool changed = false;
        foreach (CreditApplication application in applications)
        {
            if (!branchByCustomer.TryGetValue(application.CustomerId, out Guid? branchId) || !branchId.HasValue)
                continue;

            application.BranchId = branchId;
            changed = true;
        }

        if (changed)
            await context.SaveChangesAsync();
    }
}
