using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record UpdateProjectResult(bool Exists, ProjectDto? Project);
