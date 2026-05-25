using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Events;

public sealed record ProjectDeleted(
    ProjectDto Project,
    DateTimeOffset OccurredAt) : IDataEvent;
