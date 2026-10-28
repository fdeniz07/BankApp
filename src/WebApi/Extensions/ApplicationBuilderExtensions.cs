using Core.CrossCuttingConcerns.Exceptions.Extensions;
using Scalar.AspNetCore;
using Serilog;
using WebApi.Constants;
using WebApi.Modules.Notification.Realtime;

namespace WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseWebApiInfrastructure(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.ConfigureCustomExceptionMiddleware();
        app.UseResponseCompression();
        // Docker / local HTTP (:8080) has no TLS — skip redirect so Scalar and clients stay on http.
        if (!app.Environment.IsEnvironment("Testing")
            && !string.Equals(
                Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECTION"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            app.UseHttpsRedirection();
        }

        string webRoot = app.Environment.WebRootPath
            ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
        Directory.CreateDirectory(Path.Combine(webRoot, "uploads", "profiles"));
        Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data", "credit-documents"));
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
            // Absolute OpenAPI URL: relative "openapi/v1.json" is resolved under /scalar/ → 404.
            app.MapScalarApiReference((options, httpContext) =>
            {
                string baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
                options.WithOpenApiRoutePattern($"{baseUrl}/openapi/{{documentName}}.json");
            });
        }

        // Liveness only — no DB/Redis probes (avoids false-unhealthy during migrate/seed).
        app.MapHealthChecks("/health").AllowAnonymous().DisableRateLimiting();

        app.MapControllers();
        app.MapHub<CreditApplicationsHub>(CreditApplicationsHub.Path).DisableRateLimiting();

        return app;
    }
}
