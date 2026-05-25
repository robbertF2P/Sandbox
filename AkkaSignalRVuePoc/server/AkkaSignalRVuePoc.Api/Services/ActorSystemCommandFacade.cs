using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Data;
using AkkaSignalRVuePoc.Contracts.Interfaces;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Contracts.Models;

namespace AkkaSignalRVuePoc.Api.Services;

public sealed class ActorSystemCommandFacade : IActorSystemCommandFacade
{
    private static readonly TimeSpan _askTimeout = TimeSpan.FromSeconds(10);
    private readonly IActorRef _rootActor;

    public ActorSystemCommandFacade(IActorRef rootActor)
    {
        _rootActor = rootActor;
    }

    public void SendLiveMessage(string text)
    {
        _rootActor.Tell(new PublishLiveMessageCommand(text));
    }

    public void StartBackgroundProcess()
    {
        _rootActor.Tell(new StartBackgroundProcessCommand());
    }

    public async Task<IReadOnlyList<OrganisationDto>> GetOrganisationsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<GetAllOrganisationsResult>(
            new GetAllOrganisationsQuery(),
            _askTimeout,
            cancellationToken);
        return result.Organisations;
    }

    public async Task<OrganisationDto?> GetOrganisationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<GetOrganisationByIdResult>(
            new GetOrganisationByIdQuery(id),
            _askTimeout,
            cancellationToken);
        return result.Organisation;
    }

    public async Task<OrganisationDto> CreateOrganisationAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<CreateOrganisationResult>(
            new CreateOrganisationCommand(name),
            _askTimeout,
            cancellationToken);
        return result.Organisation;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<GetAllProjectsResult>(
            new GetAllProjectsQuery(),
            _askTimeout,
            cancellationToken);
        return result.Projects;
    }

    public async Task<ProjectDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<GetProjectByIdResult>(
            new GetProjectByIdQuery(id),
            _askTimeout,
            cancellationToken);
        return result.Project;
    }

    public async Task<GetProjectsByOrganisationResponse> GetProjectsForOrganisationAsync(
        Guid organisationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<GetProjectsByOrganisationResult>(
            new GetProjectsByOrganisationQuery(organisationId),
            _askTimeout,
            cancellationToken);
        return new GetProjectsByOrganisationResponse(result.OrganisationExists, result.Projects);
    }

    public async Task<CreateProjectResponse> CreateProjectAsync(
        Guid organisationId,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<CreateProjectResult>(
            new CreateProjectCommand(organisationId, name, description),
            _askTimeout,
            cancellationToken);
        return new CreateProjectResponse(result.OrganisationExists, result.Project);
    }

    public async Task<UpdateProjectResponse> UpdateProjectAsync(
        Guid id,
        string? name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<UpdateProjectResult>(
            new UpdateProjectCommand(id, name, description),
            _askTimeout,
            cancellationToken);
        return new UpdateProjectResponse(result.Exists, result.Project);
    }

    public async Task<DeleteProjectResponse> DeleteProjectAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<DeleteProjectResult>(
            new DeleteProjectCommand(id),
            _askTimeout,
            cancellationToken);
        return new DeleteProjectResponse(result.Exists, result.Project);
    }
}
