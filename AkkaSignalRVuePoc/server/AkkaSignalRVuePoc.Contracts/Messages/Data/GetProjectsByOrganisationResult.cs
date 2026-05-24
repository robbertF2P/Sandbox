using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Contracts.Messages.Data;

public sealed record GetProjectsByOrganisationResult(
    Guid OrganisationId,
    bool OrganisationExists,
    IReadOnlyList<ProjectDto> Projects);
