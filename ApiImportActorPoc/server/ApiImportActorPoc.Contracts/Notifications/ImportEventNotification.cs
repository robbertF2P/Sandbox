namespace ApiImportActorPoc.Contracts.Notifications;

public sealed record ImportEventNotification(
    string EventType,
    Guid SessionId,
    object? Payload,
    DateTimeOffset OccurredAt);
