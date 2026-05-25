using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Models;

public sealed record DeleteProjectResponse(bool Exists, ProjectDto? Project);