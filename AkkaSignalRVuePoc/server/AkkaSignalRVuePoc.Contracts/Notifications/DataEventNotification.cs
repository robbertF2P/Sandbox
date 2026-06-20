using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Notifications;

public sealed record DataEventNotification(
    string EventType,
    ProjectDto Project,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null);
