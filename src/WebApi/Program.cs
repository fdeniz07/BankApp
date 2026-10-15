using Application;
using Core.Logging;
using Persistence;
using Persistence.Contexts;
using Serilog;
using WebApi.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting BankApp WebApi");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseCoreSerilog();

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
