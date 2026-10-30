using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Contexts;

namespace Persistence.Seed;

public static class BankingDbContextSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        BankingDbContext context = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        IHostEnvironment hostEnvironment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        await BranchSeed.SeedAsync(context);
        await SeedIndividualCustomersAsync(context);
        await SeedCorporateCustomersAsync(context);
        await CustomerBranchAssignmentSeed.SeedAsync(context);
        await SeedCreditTypesAsync(context);
        await SyncCreditTypeCurrenciesAsync(context);
        await SeedCreditSubTypesAsync(context);
        await SyncCreditSubTypesAsync(context);
        await SeedCreditApplicationsAsync(context);
        await SyncCreditApplicationsAsync(context);
        await ApplicationBranchAssignmentSeed.SeedAsync(context);
        await ApplicationLocaleConsistencySeed.SeedAsync(context);
        await IdentitySeed.SeedAsync(context);
        await CreditApplicationDocumentSeed.SeedAsync(context, hostEnvironment.ContentRootPath);
    }

    private static async Task SeedIndividualCustomersAsync(BankingDbContext context)
    {
        List<IndividualCustomer> toAdd = [];
        int sequence = await context.IndividualCustomers.CountAsync();

        foreach (string locale in IndividualCustomerSeed.Locales)
        {
            int existingForLocale = await context.IndividualCustomers
                .CountAsync(c => c.Email.Contains($"@individual.{locale}.local"));
            if (locale == "tr" && existingForLocale == 0)
            {
                existingForLocale = await context.IndividualCustomers
                    .CountAsync(c => !c.Email.Contains("@individual.en.local") && !c.Email.Contains("@individual.de.local"));
                existingForLocale = Math.Min(existingForLocale, IndividualCustomerSeed.CountPerLocale);
            }

            int missing = IndividualCustomerSeed.CountPerLocale - existingForLocale;
            if (missing <= 0)
                continue;

            toAdd.AddRange(IndividualCustomerSeed.GenerateForLocale(locale, missing, sequence));
            sequence += missing;
        }

        if (toAdd.Count == 0)
            return;

        await context.IndividualCustomers.AddRangeAsync(toAdd);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCorporateCustomersAsync(BankingDbContext context)
    {
        List<CorporateCustomer> toAdd = [];
        int sequence = await context.CorporateCustomers.CountAsync();

        foreach (string locale in CorporateCustomerSeed.Locales)
        {
            int existingForLocale = await context.CorporateCustomers
                .CountAsync(c => c.Email.Contains($"@corporate.{locale}.local"));
            if (locale == "tr" && existingForLocale == 0)
            {
                existingForLocale = await context.CorporateCustomers
                    .CountAsync(c => !c.Email.Contains("@corporate.en.local") && !c.Email.Contains("@corporate.de.local"));
                existingForLocale = Math.Min(existingForLocale, CorporateCustomerSeed.CountPerLocale);
            }

            int missing = CorporateCustomerSeed.CountPerLocale - existingForLocale;
            if (missing <= 0)
                continue;

            toAdd.AddRange(CorporateCustomerSeed.GenerateForLocale(locale, missing, sequence));
            sequence += missing;
        }

        if (toAdd.Count == 0)
            return;

        await context.CorporateCustomers.AddRangeAsync(toAdd);
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

    private static async Task SyncCreditTypeCurrenciesAsync(BankingDbContext context)
    {
        List<CreditType> creditTypes = await context.CreditTypes
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        bool changed = false;
        foreach (CreditType creditType in creditTypes)
        {
            if (CreditTypeSeed.CurrencyByCode.TryGetValue(creditType.Code, out string? currency) &&
                !string.Equals(creditType.CurrencyCode, currency, StringComparison.OrdinalIgnoreCase))
            {
                creditType.CurrencyCode = currency;
                changed = true;
            }

            if (CreditTypeSeed.LocaleByCode.TryGetValue(creditType.Code, out string? locale) &&
                !string.Equals(creditType.Locale, locale, StringComparison.OrdinalIgnoreCase))
            {
                creditType.Locale = locale;
                changed = true;
            }
            else if (string.IsNullOrWhiteSpace(creditType.Locale))
            {
                creditType.Locale = CreditSubTypeSeed.ResolveLocaleFromCode(creditType.Code);
                changed = true;
            }
        }

        if (changed)
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

    private static async Task SyncCreditSubTypesAsync(BankingDbContext context)
    {
        List<CreditType> creditTypes = await context.CreditTypes
            .Where(c => !c.IsDeleted)
            .ToListAsync();
        Dictionary<Guid, CreditType> typeById = creditTypes.ToDictionary(t => t.Id);

        List<CreditSubType> subTypes = await context.CreditSubTypes
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Code)
            .ToListAsync();

        bool changed = false;
        foreach (IGrouping<Guid, CreditSubType> group in subTypes.GroupBy(s => s.CreditTypeId))
        {
            if (!typeById.TryGetValue(group.Key, out CreditType? creditType))
                continue;

            int index = 0;
            foreach (CreditSubType subType in group.OrderBy(s => s.Code))
            {
                if (CreditSubTypeSeed.ApplyCatalogValues(subType, creditType, index))
                    changed = true;
                index++;
            }
        }

        if (changed)
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
        Dictionary<Guid, CreditType> typeById = creditTypes.ToDictionary(c => c.Id);

        List<CreditSubType> activeSubTypes = await context.CreditSubTypes
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.IsActive)
            .ToListAsync();

        List<(CreditSubType SubType, CreditCategory Category, string CurrencyCode, string Locale)> subTypes = activeSubTypes
            .Where(s => typeById.ContainsKey(s.CreditTypeId))
            .Select(s =>
            {
                CreditType type = typeById[s.CreditTypeId];
                string locale = string.IsNullOrWhiteSpace(type.Locale)
                    ? CreditSubTypeSeed.ResolveLocaleFromCode(type.Code)
                    : type.Locale;
                return (s, type.Category, type.CurrencyCode, locale);
            })
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

    private static async Task SyncCreditApplicationsAsync(BankingDbContext context)
    {
        List<CreditType> creditTypes = await context.CreditTypes
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .ToListAsync();
        Dictionary<Guid, string> currencyByTypeId = creditTypes.ToDictionary(
            t => t.Id,
            t => string.IsNullOrWhiteSpace(t.CurrencyCode) ? "USD" : t.CurrencyCode);

        List<CreditSubType> subTypes = await context.CreditSubTypes
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .ToListAsync();
        Dictionary<Guid, CreditSubType> subTypeById = subTypes.ToDictionary(s => s.Id);
        Dictionary<Guid, string> currencyBySubTypeId = subTypes
            .Where(s => currencyByTypeId.ContainsKey(s.CreditTypeId))
            .ToDictionary(s => s.Id, s => currencyByTypeId[s.CreditTypeId]);

        List<CreditApplication> applications = await context.CreditApplications
            .Where(c => !c.IsDeleted)
            .ToListAsync();

        bool changed = false;
        foreach (CreditApplication application in applications)
        {
            if (!subTypeById.TryGetValue(application.CreditSubTypeId, out CreditSubType? subType))
                continue;

            string currency = currencyBySubTypeId.TryGetValue(application.CreditSubTypeId, out string? code)
                ? code
                : "USD";

            decimal requested = CreditSubTypeSeed.RoundAmount(
                Math.Clamp(application.RequestedAmount, subType.MinAmount, subType.MaxAmount),
                currency);
            int term = CreditSubTypeSeed.SnapTermMonths(
                application.RequestedTermMonths,
                subType.MinTermMonths,
                subType.MaxTermMonths);

            if (application.RequestedAmount != requested)
            {
                application.RequestedAmount = requested;
                changed = true;
            }

            if (application.RequestedTermMonths != term)
            {
                application.RequestedTermMonths = term;
                changed = true;
            }

            if (application.MonthlyIncome is decimal income)
            {
                decimal roundedIncome = CreditSubTypeSeed.RoundAmount(income, currency);
                if (application.MonthlyIncome != roundedIncome)
                {
                    application.MonthlyIncome = roundedIncome;
                    changed = true;
                }
            }

            if (application.ExistingDebtAmount is decimal debt)
            {
                decimal roundedDebt = CreditSubTypeSeed.RoundAmount(debt, currency);
                if (application.ExistingDebtAmount != roundedDebt)
                {
                    application.ExistingDebtAmount = roundedDebt;
                    changed = true;
                }
            }

            if (application.Status == CreditApplicationStatus.Approved)
            {
                if (application.ApprovedAmount is decimal approvedAmount)
                {
                    decimal roundedApproved = CreditSubTypeSeed.RoundAmount(
                        Math.Clamp(approvedAmount, subType.MinAmount, requested),
                        currency);
                    if (application.ApprovedAmount != roundedApproved)
                    {
                        application.ApprovedAmount = roundedApproved;
                        changed = true;
                    }
                }

                if (application.ApprovedTermMonths is int approvedTerm)
                {
                    int snapped = CreditSubTypeSeed.SnapTermMonths(
                        approvedTerm,
                        subType.MinTermMonths,
                        term);
                    if (application.ApprovedTermMonths != snapped)
                    {
                        application.ApprovedTermMonths = snapped;
                        changed = true;
                    }
                }
            }
        }

        if (changed)
            await context.SaveChangesAsync();
    }
}
