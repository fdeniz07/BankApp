namespace Application.Services.Search;

public sealed class LogstashOptions
{
    public const string SectionName = "Logstash";

    public bool Enabled { get; set; }

    /// <summary>HTTP input URL for Serilog.Sinks.Http (Compose: http://logstash:8080).</summary>
    public string? HttpUrl { get; set; }
}
