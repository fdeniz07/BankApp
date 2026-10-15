using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.Seed;

namespace Persistence.Contexts;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        bool runMigrations,
        bool runSeed,
        CancellationToken cancellationToken = default)
    {
        if (!runMigrations && !runSeed)
            return;

        using IServiceScope scope = serviceProvider.CreateScope();
        ILogger logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseInitializer");

        BankingDbContext context = scope.ServiceProvider.GetRequiredService<BankingDbContext>();

        if (runMigrations)
        {
            logger.LogInformation("Applying EF Core migrations...");
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("EF Core migrations applied.");
        }

        if (runSeed)
        {
            logger.LogInformation("Seeding database...");
            await BankingDbContextSeed.SeedAsync(serviceProvider);
            logger.LogInformation("Database seed completed.");
        }
    }
}
