namespace ApiImportActorPoc.Contracts.Events;

public sealed record ImportStarted(
    Guid SessionId,
    string ProjectName,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null) : IDataEvent;
