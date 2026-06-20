namespace ApiImportActorPoc.Contracts.Events;

public sealed record ImportProgressUpdated(
    Guid SessionId,
    int Step,
    int TotalSteps,
    string Message,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null) : IDataEvent;
