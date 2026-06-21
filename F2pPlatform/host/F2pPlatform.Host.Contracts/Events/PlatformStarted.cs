namespace F2pPlatform.Host.Contracts.Events;

public sealed record PlatformStarted(
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null) : IDataEvent;
