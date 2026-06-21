namespace Platform.Serilog.Logging.Correlation;

public readonly record struct CorrelationContext(
    string CorrelationId,
    string? UseCase = null,
    string? CausationId = null)
{
    private static readonly AsyncLocal<CorrelationContext?> CurrentContext = new();

    public static CorrelationContext? Current
    {
        get => CurrentContext.Value;
        private set => CurrentContext.Value = value;
    }

    internal static IDisposable Push(CorrelationContext context) => new Scope(context);

    public CorrelationFlow ToFlow() => new(CorrelationId, UseCase, CausationId);

    private sealed class Scope : IDisposable
    {
        private readonly CorrelationContext? _previous;

        public Scope(CorrelationContext context)
        {
            _previous = Current;
            Current = context;
        }

        public void Dispose() => Current = _previous;
    }
}
