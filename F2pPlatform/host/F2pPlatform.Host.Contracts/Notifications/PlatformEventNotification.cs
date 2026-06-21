namespace F2pPlatform.Host.Contracts.Notifications;

public sealed record PlatformEventNotification(
    string EventType,
    object Payload,
    DateTimeOffset OccurredAt,
    string? CorrelationId,
    string? UseCase);
