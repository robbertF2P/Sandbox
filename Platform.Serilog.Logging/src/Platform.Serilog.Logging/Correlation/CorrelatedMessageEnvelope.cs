namespace Platform.Serilog.Logging.Correlation;

/// <summary>
/// Carries an actor-system message together with correlation metadata across Akka actor boundaries.
/// </summary>
public sealed record CorrelatedMessageEnvelope(
    object Message,
    string CorrelationId,
    string? UseCase = null,
    string? CausationId = null);
