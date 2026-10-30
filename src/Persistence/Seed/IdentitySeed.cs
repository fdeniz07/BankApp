using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

public static class IdentitySeed
{
    /// <summary>Unified lab password for all demo accounts (easy copy/paste on login card).</summary>
    public const string DemoPassword = BranchLocaleMap.DemoPassword;

    private static readonly (string Email, string[] Roles, string? Locale, string? FirstName, string? LastName, StaffPosition? Position)[] DemoUsers =
    [
        ("staff.tr@bank.local", [GeneralOperationClaims.BankStaff], null, "Emre", "Kaya", StaffPosition.BranchOfficer),
        ("staff.en@bank.local", [GeneralOperationClaims.BankStaff], null, "Anna", "Miller", StaffPosition.RelationshipManager),
        ("staff.de@bank.local", [GeneralOperationClaims.BankStaff], null, "Jonas", "Weber", StaffPosition.RiskOfficer),
        ("staff@bank.local", [GeneralOperationClaims.BankStaff], null, "Selin", "Yılmaz", StaffPosition.CreditAnalyst),
        ("staff.nyc@bank.local", [GeneralOperationClaims.BankStaff], null, "Maya", "Chen", StaffPosition.CreditAnalyst),
        ("staff.chi@bank.local", [GeneralOperationClaims.BankStaff], null, "Jordan", "Lee", StaffPosition.BranchOfficer),
        ("manager.tr@bank.local", [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager], null, "Deniz", "Aydın", StaffPosition.BranchManager),
        ("manager.en@bank.local", [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager], null, "Alex", "Brooks", StaffPosition.BranchManager),
        ("manager.de@bank.local", [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager], null, "Lena", "Hoffmann", StaffPosition.BranchManager),
        ("manager@bank.local", [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager], null, "Deniz", "Aydın", StaffPosition.BranchManager),
        ("manager.nyc@bank.local", [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager], null, "Alex", "Brooks", StaffPosition.BranchManager),
        ("manager.chi@bank.local", [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager], null, "Sam", "Patel", StaffPosition.BranchManager),
        ("manager.ber@bank.local", [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager], null, "Lena", "Hoffmann", StaffPosition.BranchManager),
        ("superadmin@bank.local", [GeneralOperationClaims.SuperAdmin, GeneralOperationClaims.BankStaff], null, "Root", "Admin", StaffPosition.BranchManager),
        ("customer@bank.local", [GeneralOperationClaims.Customer], "tr", null, null, null),
        ("customer.tr@bank.local", [GeneralOperationClaims.Customer], "tr", null, null, null),
        ("customer.en@bank.local", [GeneralOperationClaims.Customer], "en", null, null, null),
        ("customer.de@bank.local", [GeneralOperationClaims.Customer], "de", null, null, null)
    ];

    private static readonly Dictionary<string, string> DemoStaffBranchCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["staff.tr@bank.local"] = "IST-01",
        ["staff@bank.local"] = "IST-01",
        ["staff.en@bank.local"] = "NYC-01",
        ["staff.nyc@bank.local"] = "NYC-01",
        ["staff.chi@bank.local"] = "CHI-01",
        ["staff.de@bank.local"] = "BER-01",
        ["manager.tr@bank.local"] = "ANK-01",
        ["manager@bank.local"] = "ANK-01",
        ["manager.en@bank.local"] = "NYC-01",
        ["manager.nyc@bank.local"] = "NYC-01",
        ["manager.chi@bank.local"] = "CHI-01",
        ["manager.de@bank.local"] = "BER-01",
        ["manager.ber@bank.local"] = "BER-01"
    };

    public static async Task SeedAsync(BankingDbContext context)
    {
        await SeedOperationClaimsAsync(context);
        await SeedUsersAsync(context);
        await EnsureDemoPasswordsAsync(context);
        await EnsureDemoStaffProfilesAsync(context);
        await EnsureDemoStaffBranchesAsync(context);
        await EnsureDemoCustomersHaveApplicationsAsync(context);
    }

    private static async Task SeedOperationClaimsAsync(BankingDbContext context)
    {
        string[] claimNames =
        [
            GeneralOperationClaims.Customer,
            GeneralOperationClaims.BankStaff,
            GeneralOperationClaims.BankManager,
            GeneralOperationClaims.SuperAdmin
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

            HashingHelper.CreatePasswordHash(DemoPassword, out byte[] hash, out byte[] salt);
            Guid? customerId = demo.Roles.Contains(GeneralOperationClaims.Customer)
                ? await ResolveCustomerIdForLocaleAsync(context, demo.Locale ?? "tr")
                : null;

            ApplicationUser user = new()
            {
                Email = demo.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CustomerId = customerId,
                FirstName = demo.FirstName,
                LastName = demo.LastName,
                Position = demo.Position,
                IsActive = true,
                EmailConfirmed = true
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

    /// <summary>Resets all known demo accounts to the shared lab password (no DB wipe).</summary>
    private static async Task EnsureDemoPasswordsAsync(BankingDbContext context)
    {
        HashingHelper.CreatePasswordHash(DemoPassword, out byte[] hash, out byte[] salt);
        HashSet<string> emails = DemoUsers.Select(d => d.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);

        List<ApplicationUser> users = await context.Users
            .Where(u => emails.Contains(u.Email))
            .ToListAsync();

        foreach (ApplicationUser user in users)
        {
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.EmailConfirmed = true;
            user.IsActive = true;
            context.Users.Update(user);
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDemoStaffProfilesAsync(BankingDbContext context)
    {
        foreach (var demo in DemoUsers.Where(d => d.FirstName is not null))
        {
            ApplicationUser? user = await context.Users.FirstOrDefaultAsync(u => u.Email == demo.Email);
            if (user is null)
                continue;

            bool changed = false;
            if (string.IsNullOrWhiteSpace(user.FirstName) && demo.FirstName is not null)
            {
                user.FirstName = demo.FirstName;
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(user.LastName) && demo.LastName is not null)
            {
                user.LastName = demo.LastName;
                changed = true;
            }
            if (user.Position is null && demo.Position is not null)
            {
                user.Position = demo.Position;
                changed = true;
            }

            if (changed)
                context.Users.Update(user);
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureDemoStaffBranchesAsync(BankingDbContext context)
    {
        foreach ((string email, string branchCode) in DemoStaffBranchCodes)
        {
            ApplicationUser? user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
                continue;

            Guid? branchId = await BranchSeed.ResolveIdByCodeAsync(context, branchCode);
            if (!branchId.HasValue)
                continue;

            if (user.BranchId == branchId)
                continue;

            user.BranchId = branchId;
            context.Users.Update(user);
        }

        await context.SaveChangesAsync();
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
