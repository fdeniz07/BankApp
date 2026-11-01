using Application;
using Core.Logging;
using Persistence;
using Persistence.Contexts;
using Serilog;
using WebApi.Configuration;
using WebApi.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting BankApp WebApi");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseCoreSerilog();

    // Lab: optional JWT key from Vault KV (secret/bankapp); falls back to env/appsettings.
    await builder.Configuration.ApplyVaultSecretsAsync();

    builder.Services.AddWebApiInfrastructure(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddPersistenceServices(builder.Configuration);

    var app = builder.Build();

    bool runMigrations = app.Configuration.GetValue(
        "Database:RunMigrations",
        app.Environment.IsDevelopment());
    bool runSeed = app.Configuration.GetValue(
        "Database:RunSeed",
        app.Environment.IsDevelopment());

    await DatabaseInitializer.InitializeAsync(app.Services, runMigrations, runSeed);

    // Lab: warm Elasticsearch credit-applications index after SQL seed
    try
    {
        using IServiceScope scope = app.Services.CreateScope();
        Application.Services.Search.ICreditApplicationSearchService search =
            scope.ServiceProvider.GetRequiredService<Application.Services.Search.ICreditApplicationSearchService>();
        if (search.IsAvailable)
            await search.ReindexAllAsync();
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Elasticsearch reindex skipped (SQL list still works)");
    }

    app.UseWebApiInfrastructure();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "BankApp WebApi terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
