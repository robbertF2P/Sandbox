using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Models;

public sealed record UpdateProjectResponse(bool Exists, ProjectDto? Project);