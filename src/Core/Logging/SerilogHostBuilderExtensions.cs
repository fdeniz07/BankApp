using Microsoft.Extensions.Hosting;
using Serilog;

namespace Core.Logging;

public static class SerilogHostBuilderExtensions
{
    /// <summary>
    /// Configures Serilog from <c>IConfiguration</c> (Serilog + Seq sections).
    /// Reusable across host applications that reference Core.
    /// </summary>
    public static IHostBuilder UseCoreSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.With<ActivityEnricher>();

            string? seqServerUrl = context.Configuration["Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqServerUrl))
            {
                loggerConfiguration.WriteTo.Seq(seqServerUrl);
            }
        });
    }
}
