namespace Platform.Serilog.Logging.Correlation;

public static class CorrelationHeaders
{
    public const string CorrelationId = "X-Correlation-Id";
    public const string UseCase = "X-Use-Case";
    public const string CausationId = "X-Causation-Id";
}
