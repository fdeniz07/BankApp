using Core.CrossCuttingConcerns.Exceptions.Extensions;
using Scalar.AspNetCore;
using Serilog;
using WebApi.Constants;
using WebApi.Realtime;

namespace WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseWebApiInfrastructure(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.ConfigureCustomExceptionMiddleware();
        app.UseResponseCompression();
        if (!app.Environment.IsEnvironment("Testing"))
            app.UseHttpsRedirection();

        string webRoot = app.Environment.WebRootPath
            ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "profiles"));
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webRoot),
            RequestPath = ""
        });

        app.UseCors(CorsPolicies.Default);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        // Liveness only — no DB/Redis probes (avoids false-unhealthy during migrate/seed).
        app.MapHealthChecks("/health").AllowAnonymous().DisableRateLimiting();

        app.MapControllers();
        app.MapHub<CreditApplicationsHub>(CreditApplicationsHub.Path).DisableRateLimiting();

        return app;
    }
}
