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

    // Lab: warm Elasticsearch indexes after SQL seed
    try
    {
        using IServiceScope scope = app.Services.CreateScope();
        Application.Services.Search.ICreditApplicationSearchService creditSearch =
            scope.ServiceProvider.GetRequiredService<Application.Services.Search.ICreditApplicationSearchService>();
        if (creditSearch.IsAvailable)
            await creditSearch.ReindexAllAsync();

        Application.Services.Search.IStaffUserSearchService staffSearch =
            scope.ServiceProvider.GetRequiredService<Application.Services.Search.IStaffUserSearchService>();
        if (staffSearch.IsAvailable)
            await staffSearch.ReindexAllAsync();

        Application.Services.Search.ICustomerSearchService customerSearch =
            scope.ServiceProvider.GetRequiredService<Application.Services.Search.ICustomerSearchService>();
        if (customerSearch.IsAvailable)
            await customerSearch.ReindexAllAsync();

        Application.Services.Search.ICreditTypeSearchService creditTypeSearch =
            scope.ServiceProvider.GetRequiredService<Application.Services.Search.ICreditTypeSearchService>();
        if (creditTypeSearch.IsAvailable)
            await creditTypeSearch.ReindexAllAsync();
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
