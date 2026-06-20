using Serilog.Core;
using Serilog.Events;

namespace Platform.Serilog.Logging.Correlation;

public sealed class CorrelationLogEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        CorrelationContext? context = CorrelationContext.Current;
        if (context is CorrelationContext value)
        {
            AddIfAbsent(logEvent, propertyFactory, "CorrelationId", value.CorrelationId);
            if (!string.IsNullOrWhiteSpace(value.UseCase))
            {
                AddIfAbsent(logEvent, propertyFactory, "UseCase", value.UseCase);
            }

            if (!string.IsNullOrWhiteSpace(value.CausationId))
            {
                AddIfAbsent(logEvent, propertyFactory, "CausationId", value.CausationId);
            }
        }

        System.Diagnostics.Activity? activity = System.Diagnostics.Activity.Current;
        if (activity is not null)
        {
            AddIfAbsent(logEvent, propertyFactory, "TraceId", activity.TraceId.ToString());
            AddIfAbsent(logEvent, propertyFactory, "SpanId", activity.SpanId.ToString());
        }
    }

    private static void AddIfAbsent(
        LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory,
        string name,
        object? value)
    {
        if (value is null || logEvent.Properties.ContainsKey(name))
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(name, value));
    }
}
