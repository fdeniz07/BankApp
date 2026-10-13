using Core.CrossCuttingConcerns.Exceptions.Extensions;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();




var app = builder.Build();

app.ConfigureCustomExceptionMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

