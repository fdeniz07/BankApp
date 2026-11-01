using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Http.BatchFormatters;

namespace Core.Logging;

public static class SerilogHostBuilderExtensions
{
    /// <summary>
    /// Configures Serilog from <c>IConfiguration</c> (Serilog + Seq + optional ELK).
    /// Seq stays the primary .NET log UX; Logstash/Elasticsearch add Kibana search.
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

            // Classic ELK path: Serilog → Logstash HTTP → Elasticsearch (bankapp-logs-*)
            bool logstashEnabled = context.Configuration.GetValue("Logstash:Enabled", false);
            string? logstashHttp = context.Configuration["Logstash:HttpUrl"];
            if (logstashEnabled && !string.IsNullOrWhiteSpace(logstashHttp))
            {
                loggerConfiguration.WriteTo.Http(
                    requestUri: logstashHttp,
                    queueLimitBytes: null,
                    textFormatter: new CompactJsonFormatter(),
                    batchFormatter: new ArrayBatchFormatter());
            }

            // Direct ES sink when Logstash is off but Elasticsearch is on
            bool esEnabled = context.Configuration.GetValue("Elasticsearch:Enabled", false);
            string? esNode = context.Configuration["Elasticsearch:Nodes:0"]
                ?? context.Configuration.GetSection("Elasticsearch:Nodes").Get<string[]>()?.FirstOrDefault();
            if (esEnabled && !string.IsNullOrWhiteSpace(esNode) && !logstashEnabled)
            {
                loggerConfiguration.WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(esNode))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = "bankapp-logs-{0:yyyy.MM.dd}",
                    TypeName = null
                });
            }
        });
    }
}
