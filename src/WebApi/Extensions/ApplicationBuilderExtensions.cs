using Core.CrossCuttingConcerns.Exceptions.Extensions;
using Scalar.AspNetCore;
using Serilog;
using WebApi.Constants;

namespace WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseWebApiInfrastructure(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.ConfigureCustomExceptionMiddleware();
        app.UseResponseCompression();
        app.UseHttpsRedirection();
        app.UseCors(CorsPolicies.Default);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapControllers();

        return app;
    }
}
