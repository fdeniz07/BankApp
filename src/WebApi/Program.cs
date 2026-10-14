using Application;
using Core.Logging;
using Persistence;
using Persistence.Seed;
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

    if (app.Environment.IsDevelopment())
    {
        await BankingDbContextSeed.SeedAsync(app.Services);
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
