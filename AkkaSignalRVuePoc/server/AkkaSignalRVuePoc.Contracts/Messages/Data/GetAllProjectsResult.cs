using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record GetAllProjectsResult(IReadOnlyList<ProjectDto> Projects);
