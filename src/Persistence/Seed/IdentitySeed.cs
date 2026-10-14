using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seed;

public static class IdentitySeed
{
    public static async Task SeedAsync(BankingDbContext context)
    {
        await SeedOperationClaimsAsync(context);
        await SeedUsersAsync(context);
    }

    private static async Task SeedOperationClaimsAsync(BankingDbContext context)
    {
        string[] claimNames = [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];

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
        OperationClaim bankStaffClaim = await context.OperationClaims
            .SingleAsync(c => c.Name == GeneralOperationClaims.BankStaff);
        OperationClaim customerClaim = await context.OperationClaims
            .SingleAsync(c => c.Name == GeneralOperationClaims.Customer);

        if (!await context.Users.AnyAsync(u => u.Email == "staff@bank.local"))
        {
            HashingHelper.CreatePasswordHash("Staff123!", out byte[] staffHash, out byte[] staffSalt);
            User staff = new()
            {
                Email = "staff@bank.local",
                PasswordHash = staffHash,
                PasswordSalt = staffSalt,
                IsActive = true
            };
            await context.Users.AddAsync(staff);
            await context.SaveChangesAsync();

            await context.UserOperationClaims.AddAsync(new UserOperationClaim
            {
                UserId = staff.Id,
                OperationClaimId = bankStaffClaim.Id
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync(u => u.Email == "customer@bank.local"))
        {
            Guid? firstIndividualId = await context.IndividualCustomers
                .Where(c => !c.IsDeleted && c.IsActive)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync();

            HashingHelper.CreatePasswordHash("Customer123!", out byte[] customerHash, out byte[] customerSalt);
            User customerUser = new()
            {
                Email = "customer@bank.local",
                PasswordHash = customerHash,
                PasswordSalt = customerSalt,
                CustomerId = firstIndividualId,
                IsActive = true
            };
            await context.Users.AddAsync(customerUser);
            await context.SaveChangesAsync();

            await context.UserOperationClaims.AddAsync(new UserOperationClaim
            {
                UserId = customerUser.Id,
                OperationClaimId = customerClaim.Id
            });
            await context.SaveChangesAsync();
        }
    }
}
