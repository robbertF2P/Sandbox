namespace ApiImportActorPoc.Contracts.Events;

public sealed record ImportPersisted(
    Guid SessionId,
    Guid ProjectId,
    DateTimeOffset OccurredAt) : IDataEvent;
