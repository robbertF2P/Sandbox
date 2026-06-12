namespace ApiImportActorPoc.Contracts.Events;

public sealed record ImportPersisted(
    Guid SessionId,
    int ProjectId,
    DateTimeOffset OccurredAt) : IDataEvent;
