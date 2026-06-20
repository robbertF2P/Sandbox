namespace Platform.Serilog.Logging.Correlation;

public readonly record struct CorrelationFlow(
    string CorrelationId,
    string? UseCase = null,
    string? CausationId = null)
{
    public static CorrelationFlow FromCurrentOrNew(string? useCase = null)
    {
        CorrelationContext? current = CorrelationContext.Current;
        if (current is CorrelationContext context)
        {
            return new CorrelationFlow(context.CorrelationId, useCase ?? context.UseCase, context.CausationId);
        }

        return new CorrelationFlow(global::Platform.Serilog.Logging.Correlation.CorrelationId.New(), useCase);
    }

    public CorrelationScope BeginScope() => CorrelationScope.Begin(CorrelationId, UseCase, CausationId);

    public CorrelatedMessageEnvelope Wrap(object message) =>
        new(message, CorrelationId, UseCase, CausationId);

    public CorrelatedMessageEnvelope WrapChild(object message, string? childUseCase = null) =>
        new(message, CorrelationId, childUseCase ?? UseCase, global::Platform.Serilog.Logging.Correlation.CorrelationId.New());
}
