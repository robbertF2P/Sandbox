using AkkaSignalRVuePoc.Contracts.Data;

namespace AkkaSignalRVuePoc.Api.Services;

public interface IActorSystemCommandFacade
{
    void SendLiveMessage(string text);

    void StartBackgroundProcess();

    Task<IReadOnlyList<OrganisationDto>> GetOrganisationsAsync(CancellationToken cancellationToken = default);

    Task<OrganisationDto?> GetOrganisationAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OrganisationDto> CreateOrganisationAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken cancellationToken = default);

    Task<ProjectDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default);

    Task<GetProjectsByOrganisationResponse> GetProjectsForOrganisationAsync(
        Guid organisationId,
        CancellationToken cancellationToken = default);

    Task<CreateProjectResponse> CreateProjectAsync(
        Guid organisationId,
        string name,
        string? description,
        CancellationToken cancellationToken = default);

    Task<UpdateProjectResponse> UpdateProjectAsync(
        Guid id,
        string? name,
        string? description,
        CancellationToken cancellationToken = default);
}

public sealed record GetProjectsByOrganisationResponse(
    bool OrganisationExists,
    IReadOnlyList<ProjectDto> Projects);

public sealed record CreateProjectResponse(bool OrganisationExists, ProjectDto? Project);

public sealed record UpdateProjectResponse(bool Exists, ProjectDto? Project);
