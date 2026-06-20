using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Data;
using AkkaSignalRVuePoc.Contracts.Interfaces;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Contracts.Models;
using Platform.Serilog.Logging.Akka;

namespace AkkaSignalRVuePoc.Api.Services;

public sealed class ActorSystemCommandFacade : IActorSystemCommandFacade
{
    private static readonly TimeSpan _askTimeout = TimeSpan.FromSeconds(10);
    private readonly IActorRef _rootActor;

    public ActorSystemCommandFacade(IActorRef rootActor)
    {
        _rootActor = rootActor;
    }

    public void SendLiveMessage(string text) =>
        _rootActor.TellCorrelated(new PublishLiveMessageCommand(text), "LiveMessage.Publish");

    public void StartBackgroundProcess() =>
        _rootActor.TellCorrelated(new StartBackgroundProcessCommand(), "Background.Start");

    public async Task<IReadOnlyList<OrganisationDto>> GetOrganisationsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.AskCorrelated<GetAllOrganisationsResult>(
            new GetAllOrganisationsQuery(),
            "Catalog.GetOrganisations",
            _askTimeout,
            cancellationToken);
        return result.Organisations;
    }

    public async Task<OrganisationDto?> GetOrganisationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.AskCorrelated<GetOrganisationByIdResult>(
            new GetOrganisationByIdQuery(id),
            "Catalog.GetOrganisation",
            _askTimeout,
            cancellationToken);
        return result.Organisation;
    }

    public async Task<OrganisationDto> CreateOrganisationAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.AskCorrelated<CreateOrganisationResult>(
            new CreateOrganisationCommand(name),
            "Catalog.CreateOrganisation",
            _askTimeout,
            cancellationToken);
        return result.Organisation;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.AskCorrelated<GetAllProjectsResult>(
            new GetAllProjectsQuery(),
            "Catalog.GetProjects",
            _askTimeout,
            cancellationToken);
        return result.Projects;
    }

    public async Task<ProjectDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.AskCorrelated<GetProjectByIdResult>(
            new GetProjectByIdQuery(id),
            "Catalog.GetProject",
            _askTimeout,
            cancellationToken);
        return result.Project;
    }

    public async Task<GetProjectsByOrganisationResponse> GetProjectsForOrganisationAsync(
        Guid organisationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.AskCorrelated<GetProjectsByOrganisationResult>(
            new GetProjectsByOrganisationQuery(organisationId),
            "Catalog.GetProjectsByOrganisation",
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
        var result = await _rootActor.AskCorrelated<CreateProjectResult>(
            new CreateProjectCommand(organisationId, name, description),
            "Catalog.CreateProject",
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
        var result = await _rootActor.AskCorrelated<UpdateProjectResult>(
            new UpdateProjectCommand(id, name, description),
            "Catalog.UpdateProject",
            _askTimeout,
            cancellationToken);
        return new UpdateProjectResponse(result.Exists, result.Project);
    }

    public async Task<DeleteProjectResponse> DeleteProjectAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.AskCorrelated<DeleteProjectResult>(
            new DeleteProjectCommand(id),
            "Catalog.DeleteProject",
            _askTimeout,
            cancellationToken);
        return new DeleteProjectResponse(result.Exists, result.Project);
    }
}
