using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

public static class IdentitySeed
{
    private static readonly (string Email, string Password, string[] Roles, string? Locale)[] DemoUsers =
    [
        ("staff@bank.local", "Staff123!", [GeneralOperationClaims.BankStaff], null),
        ("staff.tr@bank.local", "Staff123!", [GeneralOperationClaims.BankStaff], null),
        ("staff.en@bank.local", "Staff123!", [GeneralOperationClaims.BankStaff], null),
        ("staff.de@bank.local", "Staff123!", [GeneralOperationClaims.BankStaff], null),
        ("manager@bank.local", "Manager123!", [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager], null),
        ("customer@bank.local", "Customer123!", [GeneralOperationClaims.Customer], "tr"),
        ("customer.tr@bank.local", "Customer123!", [GeneralOperationClaims.Customer], "tr"),
        ("customer.en@bank.local", "Customer123!", [GeneralOperationClaims.Customer], "en"),
        ("customer.de@bank.local", "Customer123!", [GeneralOperationClaims.Customer], "de")
    ];

    public static async Task SeedAsync(BankingDbContext context)
    {
        await SeedOperationClaimsAsync(context);
        await SeedUsersAsync(context);
        await EnsureDemoCustomersHaveApplicationsAsync(context);
    }

    private static async Task SeedOperationClaimsAsync(BankingDbContext context)
    {
        string[] claimNames =
        [
            GeneralOperationClaims.Customer,
            GeneralOperationClaims.BankStaff,
            GeneralOperationClaims.BankManager
        ];

        foreach (string claimName in claimNames)
        {
            bool exists = await context.OperationClaims.AnyAsync(c => c.Name == claimName);
            if (exists)
                continue;

            await context.OperationClaims.AddAsync(new OperationClaim { Name = claimName });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(BankingDbContext context)
    {
        Dictionary<string, OperationClaim> claimsByName = await context.OperationClaims
            .ToDictionaryAsync(c => c.Name);

        foreach (var demo in DemoUsers)
        {
            if (await context.Users.AnyAsync(u => u.Email == demo.Email))
                continue;

            HashingHelper.CreatePasswordHash(demo.Password, out byte[] hash, out byte[] salt);
            Guid? customerId = demo.Roles.Contains(GeneralOperationClaims.Customer)
                ? await ResolveCustomerIdForLocaleAsync(context, demo.Locale ?? "tr")
                : null;

            ApplicationUser user = new()
            {
                Email = demo.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CustomerId = customerId,
                IsActive = true
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            foreach (string role in demo.Roles)
            {
                if (!claimsByName.TryGetValue(role, out OperationClaim? claim))
                    throw new InvalidOperationException($"Operation claim '{role}' is not seeded.");

                await context.UserOperationClaims.AddAsync(new UserOperationClaim
                {
                    UserId = user.Id,
                    OperationClaimId = claim.Id
                });
            }

            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureDemoCustomersHaveApplicationsAsync(BankingDbContext context)
    {
        string[] customerEmails =
        [
            "customer@bank.local",
            "customer.tr@bank.local",
            "customer.en@bank.local",
            "customer.de@bank.local"
        ];

        foreach (string email in customerEmails)
        {
            ApplicationUser? customerUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
            if (customerUser is null)
                continue;

            string locale = email.Contains(".en@", StringComparison.OrdinalIgnoreCase) ? "en"
                : email.Contains(".de@", StringComparison.OrdinalIgnoreCase) ? "de"
                : "tr";

            bool hasApplications = customerUser.CustomerId.HasValue &&
                await context.CreditApplications.AnyAsync(a =>
                    a.CustomerId == customerUser.CustomerId.Value && !a.IsDeleted);

            if (hasApplications)
                continue;

            Guid? customerId = await ResolveCustomerIdForLocaleAsync(context, locale);
            if (!customerId.HasValue)
                continue;

            customerUser.CustomerId = customerId;
            await context.SaveChangesAsync();
        }
    }

    private static async Task<Guid?> ResolveCustomerIdForLocaleAsync(BankingDbContext context, string locale)
    {
        string marker = $"@individual.{locale}.local";
        Guid? byLocale = await context.IndividualCustomers
            .Where(c => !c.IsDeleted && c.IsActive && c.Email.Contains(marker))
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync();

        if (byLocale.HasValue)
            return byLocale;

        return await context.IndividualCustomers
            .Where(c => !c.IsDeleted && c.IsActive)
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync();
    }
}
