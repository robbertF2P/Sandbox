using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Events;

public sealed record ProjectCreated(
    ProjectDto Project,
    DateTimeOffset OccurredAt) : IDataEvent;
