using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Contexts;

namespace WebApi.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"BankAppTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:RunMigrations"] = "false",
                ["Database:RunSeed"] = "false",
                ["ConnectionStrings:BankingConnectionString"] = "Server=(localdb)\\unused;Database=unused;",
                ["TokenOptions:Audience"] = "BankAppAudience",
                ["TokenOptions:Issuer"] = "BankAppIssuer",
                ["TokenOptions:AccessTokenExpiration"] = "60",
                ["TokenOptions:SecurityKey"] =
                    "BankApp-Super-Secret-Security-Key-For-Jwt-Must-Be-At-Least-64-Bytes-Long-1234567890",
                ["Seq:ServerUrl"] = "",
                ["Redis:ConnectionString"] = "localhost:6379",
                ["RabbitMq:Enabled"] = "false",
                ["Vault:Enabled"] = "false",
                ["App:PublicClientBaseUrl"] = "http://localhost:3000",
                ["CacheSettings:SlidingExpirationMinutes"] = "30",
                ["RateLimiting:Fixed:PermitLimit"] = "1000",
                ["RateLimiting:Fixed:WindowSeconds"] = "60",
                ["RateLimiting:Auth:PermitLimit"] = "1000",
                ["RateLimiting:Auth:WindowSeconds"] = "60",
                ["RateLimiting:Write:PermitLimit"] = "1000",
                ["RateLimiting:Write:WindowSeconds"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            // EF Core 8+ keeps provider configs via IDbContextOptionsConfiguration<T>.
            services.RemoveAll<IDbContextOptionsConfiguration<BankingDbContext>>();
            services.RemoveAll<DbContextOptions<BankingDbContext>>();
            services.RemoveAll<BankingDbContext>();
            services.RemoveAll<IDistributedCache>();

            services.AddDistributedMemoryCache();
            services.AddDbContext<BankingDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    public async Task SeedStaffUserAsync(
        string email = "staff@bank.local",
        string password = "Staff123!")
    {
        using IServiceScope scope = Services.CreateScope();
        BankingDbContext db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (!await db.OperationClaims.AnyAsync(c => c.Name == GeneralOperationClaims.BankStaff))
        {
            db.OperationClaims.Add(new OperationClaim { Name = GeneralOperationClaims.BankStaff });
            await db.SaveChangesAsync();
        }

        if (await db.Users.AnyAsync(u => u.Email == email))
            return;

        OperationClaim staffClaim = await db.OperationClaims
            .SingleAsync(c => c.Name == GeneralOperationClaims.BankStaff);

        HashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);
        ApplicationUser user = new()
        {
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true,
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "Staff"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.UserOperationClaims.Add(new UserOperationClaim
        {
            UserId = user.Id,
            OperationClaimId = staffClaim.Id
        });
        await db.SaveChangesAsync();
    }

    /// <summary>Seeds one active individual customer + credit type/sub-type for create/approve flows.</summary>
    public async Task<(Guid CustomerId, Guid CreditSubTypeId)> SeedCreditCatalogAsync()
    {
        using IServiceScope scope = Services.CreateScope();
        BankingDbContext db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        await db.Database.EnsureCreatedAsync();

        IndividualCustomer? existingCustomer = await db.IndividualCustomers
            .FirstOrDefaultAsync(c => c.CustomerNumber == "CUST-TEST-001");
        CreditSubType? existingSubType = await db.CreditSubTypes
            .FirstOrDefaultAsync(c => c.Code == "CST-TEST-001");

        if (existingCustomer is not null && existingSubType is not null)
            return (existingCustomer.Id, existingSubType.Id);

        IndividualCustomer customer = new()
        {
            CustomerNumber = "CUST-TEST-001",
            Email = "applicant@bank.local",
            PhoneNumber = "+905551112233",
            Address = "Test Street 1",
            IsActive = true,
            FirstName = "Ada",
            LastName = "Applicant",
            NationalId = "10000000146",
            Gender = Gender.Female.Name
        };
        db.IndividualCustomers.Add(customer);

        CreditType creditType = new()
        {
            Name = "Test Personal Loan",
            Code = "CT-TEST-IND-001",
            Locale = "en",
            Category = CreditCategory.Individual,
            CurrencyCode = "TRY",
            RiskLevel = CreditRiskLevel.Medium,
            IsActive = true
        };
        db.CreditTypes.Add(creditType);
        await db.SaveChangesAsync();

        CreditSubType subType = new()
        {
            CreditTypeId = creditType.Id,
            Name = "Test Package",
            Code = "CST-TEST-001",
            MinAmount = 10_000m,
            MaxAmount = 100_000m,
            InterestRate = 2.5m,
            MinTermMonths = 12,
            MaxTermMonths = 60,
            IsActive = true
        };
        db.CreditSubTypes.Add(subType);
        await db.SaveChangesAsync();

        return (customer.Id, subType.Id);
    }

    public async Task SeedCustomerUserAsync(
        Guid customerId,
        string email = "customer@bank.local",
        string password = "Customer123!")
    {
        using IServiceScope scope = Services.CreateScope();
        BankingDbContext db = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (!await db.OperationClaims.AnyAsync(c => c.Name == GeneralOperationClaims.Customer))
        {
            db.OperationClaims.Add(new OperationClaim { Name = GeneralOperationClaims.Customer });
            await db.SaveChangesAsync();
        }

        if (await db.Users.AnyAsync(u => u.Email == email))
            return;

        OperationClaim customerClaim = await db.OperationClaims
            .SingleAsync(c => c.Name == GeneralOperationClaims.Customer);

        HashingHelper.CreatePasswordHash(password, out byte[] hash, out byte[] salt);
        ApplicationUser user = new()
        {
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CustomerId = customerId,
            IsActive = true,
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "Customer"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.UserOperationClaims.Add(new UserOperationClaim
        {
            UserId = user.Id,
            OperationClaimId = customerClaim.Id
        });
        await db.SaveChangesAsync();
    }
}
