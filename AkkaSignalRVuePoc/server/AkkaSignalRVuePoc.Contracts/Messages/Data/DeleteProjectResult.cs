using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record DeleteProjectResult(bool Exists, ProjectDto? Project);
