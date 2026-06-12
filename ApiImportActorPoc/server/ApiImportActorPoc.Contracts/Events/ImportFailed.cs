namespace ApiImportActorPoc.Contracts.Events;

public sealed record ImportFailed(
    Guid SessionId,
    string ErrorMessage,
    DateTimeOffset OccurredAt) : IDataEvent;
