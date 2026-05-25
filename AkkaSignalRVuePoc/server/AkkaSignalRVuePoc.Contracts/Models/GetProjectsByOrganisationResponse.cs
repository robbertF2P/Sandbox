using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Models;

public sealed record GetProjectsByOrganisationResponse(
    bool OrganisationExists,
    IReadOnlyList<ProjectDto> Projects);