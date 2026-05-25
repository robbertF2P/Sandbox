using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Models;

public sealed record CreateProjectResponse(bool OrganisationExists, ProjectDto? Project);