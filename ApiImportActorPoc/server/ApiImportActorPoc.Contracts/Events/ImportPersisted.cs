namespace ApiImportActorPoc.Contracts.Events;

public sealed record ImportPersisted(
    Guid SessionId,
    int ProjectId,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null) : IDataEvent;
