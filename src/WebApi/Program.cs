using Application;
using Persistence;
using Persistence.Seed;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

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
