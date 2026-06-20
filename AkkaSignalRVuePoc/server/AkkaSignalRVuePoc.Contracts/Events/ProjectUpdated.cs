using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Events;

public sealed record ProjectUpdated(
    ProjectDto Project,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null,
    string? UseCase = null) : IDataEvent;
