using Serilog.Context;

namespace Platform.Serilog.Logging.Correlation;

public sealed class CorrelationScope : IDisposable
{
    private readonly IDisposable? _contextScope;
    private readonly IDisposable? _correlationProperty;
    private readonly IDisposable? _useCaseProperty;
    private readonly IDisposable? _causationProperty;
    private readonly IDisposable? _traceIdProperty;
    private readonly IDisposable? _spanIdProperty;

    private CorrelationScope(
        string correlationId,
        string? useCase,
        string? causationId)
    {
        _contextScope = CorrelationContext.Push(new CorrelationContext(correlationId, useCase, causationId));
        _correlationProperty = LogContext.PushProperty("CorrelationId", correlationId);
        if (!string.IsNullOrWhiteSpace(useCase))
        {
            _useCaseProperty = LogContext.PushProperty("UseCase", useCase);
        }

        if (!string.IsNullOrWhiteSpace(causationId))
        {
            _causationProperty = LogContext.PushProperty("CausationId", causationId);
        }

        System.Diagnostics.Activity? activity = System.Diagnostics.Activity.Current;
        if (activity is not null)
        {
            _traceIdProperty = LogContext.PushProperty("TraceId", activity.TraceId.ToString());
            _spanIdProperty = LogContext.PushProperty("SpanId", activity.SpanId.ToString());
        }
    }

    public static CorrelationScope Begin(
        string? correlationId = null,
        string? useCase = null,
        string? causationId = null) =>
        new(CorrelationId.GetOrCreate(correlationId), useCase, causationId);

    public static CorrelationScope Begin(CorrelationContext context) =>
        Begin(context.CorrelationId, context.UseCase, context.CausationId);

    public void Dispose()
    {
        _spanIdProperty?.Dispose();
        _traceIdProperty?.Dispose();
        _causationProperty?.Dispose();
        _useCaseProperty?.Dispose();
        _correlationProperty?.Dispose();
        _contextScope?.Dispose();
    }
}
