using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WebApi.Extensions;

public static class OpenTelemetryExtensions
{
    public const string SectionName = "OpenTelemetry";

    public static IServiceCollection AddBankAppOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        OpenTelemetryOptions options = configuration
            .GetSection(SectionName)
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        if (!options.Enabled)
            return services;

        string serviceName = string.IsNullOrWhiteSpace(options.ServiceName)
            ? "bankapp-webapi"
            : options.ServiceName;

        string otlpEndpoint = string.IsNullOrWhiteSpace(options.OtlpEndpoint)
            ? "http://localhost:4317"
            : options.OtlpEndpoint;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development"
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(instrumentation =>
                    {
                        instrumentation.Filter = context =>
                            !context.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri(otlpEndpoint);
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(exporter =>
                    {
                        exporter.Endpoint = new Uri(otlpEndpoint);
                    });
            });

        return services;
    }
}

public sealed class OpenTelemetryOptions
{
    public bool Enabled { get; set; }
    public string ServiceName { get; set; } = "bankapp-webapi";
    /// <summary>OTLP gRPC endpoint, e.g. http://otel-collector:4317</summary>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
}
