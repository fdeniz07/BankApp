using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Core.Logging;

/// <summary>
/// Adds TraceId / SpanId from the current <see cref="Activity"/> so Seq logs can be correlated with OTel traces.
/// </summary>
public sealed class ActivityEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Activity? activity = Activity.Current;
        if (activity is null)
            return;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
    }
}
