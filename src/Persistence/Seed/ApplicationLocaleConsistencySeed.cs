using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

/// <summary>
/// Fixes legacy seed mismatches: application product locale must match customer branch locale.
/// </summary>
public static class ApplicationLocaleConsistencySeed
{
    public static async Task SeedAsync(BankingDbContext context)
    {
        Dictionary<Guid, string> localeByBranchId = await context.Branches
            .Where(b => !b.IsDeleted)
            .AsNoTracking()
            .ToDictionaryAsync(
                b => b.Id,
                b => BranchLocaleMap.ResolveLocale(b.Code) ?? "tr");

        List<CreditApplication> applications = await context.CreditApplications
            .Where(a => !a.IsDeleted)
            .Include(a => a.Customer)
            .ToListAsync();

        if (applications.Count == 0)
            return;

        List<CreditType> creditTypes = await context.CreditTypes
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.IsActive)
            .ToListAsync();
        Dictionary<Guid, CreditType> typeById = creditTypes.ToDictionary(t => t.Id);

        List<CreditSubType> subTypes = await context.CreditSubTypes
            .Where(s => !s.IsDeleted && s.IsActive)
            .ToListAsync();
        Dictionary<Guid, CreditSubType> subTypeById = subTypes.ToDictionary(s => s.Id);

        var typedSubTypes = subTypes
            .Where(s => typeById.ContainsKey(s.CreditTypeId))
            .Select(s =>
            {
                CreditType type = typeById[s.CreditTypeId];
                return (SubType: s, Category: type.Category, Locale: type.Locale, Currency: type.CurrencyCode);
            })
            .ToList();

        Random random = Random.Shared;
        bool changed = false;

        foreach (CreditApplication application in applications)
        {
            Guid? branchId = application.Customer?.BranchId ?? application.BranchId;
            if (branchId.HasValue && application.BranchId != branchId)
            {
                application.BranchId = branchId;
                changed = true;
            }

            if (!branchId.HasValue || !localeByBranchId.TryGetValue(branchId.Value, out string? expectedLocale))
                continue;

            if (!subTypeById.TryGetValue(application.CreditSubTypeId, out CreditSubType? currentSub))
                continue;
            if (!typeById.TryGetValue(currentSub.CreditTypeId, out CreditType? currentType))
                continue;

            if (string.Equals(currentType.Locale, expectedLocale, StringComparison.OrdinalIgnoreCase))
                continue;

            CreditCategory category = currentType.Category;
            var candidates = typedSubTypes
                .Where(x => x.Locale == expectedLocale && x.Category == category)
                .Select(x => x.SubType)
                .ToList();
            if (candidates.Count == 0)
                continue;

            CreditSubType replacement = candidates[random.Next(candidates.Count)];
            CreditType replacementType = typeById[replacement.CreditTypeId];
            application.CreditSubTypeId = replacement.Id;
            application.RequestedAmount = CreditSubTypeSeed.RoundAmount(
                Math.Clamp(application.RequestedAmount, replacement.MinAmount, replacement.MaxAmount),
                replacementType.CurrencyCode);
            application.RequestedTermMonths = CreditSubTypeSeed.SnapTermMonths(
                application.RequestedTermMonths,
                replacement.MinTermMonths,
                replacement.MaxTermMonths);
            if (application.Status == CreditApplicationStatus.Approved)
            {
                if (application.ApprovedAmount is decimal approved)
                    application.ApprovedAmount = CreditSubTypeSeed.RoundAmount(
                        Math.Clamp(approved, replacement.MinAmount, application.RequestedAmount),
                        replacementType.CurrencyCode);
                if (application.ApprovedTermMonths is int term)
                    application.ApprovedTermMonths = CreditSubTypeSeed.SnapTermMonths(
                        term, replacement.MinTermMonths, application.RequestedTermMonths);
                application.ApprovedInterestRate = replacement.InterestRate;
            }

            changed = true;
        }

        if (changed)
            await context.SaveChangesAsync();
    }
}
