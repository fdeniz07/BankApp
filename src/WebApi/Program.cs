using Application;
using Persistence;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApiInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);

var app = builder.Build();

app.UseWebApiInfrastructure();




app.Run();
