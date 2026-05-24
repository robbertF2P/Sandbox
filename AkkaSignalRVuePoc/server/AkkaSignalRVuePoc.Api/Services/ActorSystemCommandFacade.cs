using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Data;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Contracts.Messages.Data;

namespace AkkaSignalRVuePoc.Api.Services;

public sealed class ActorSystemCommandFacade : IActorSystemCommandFacade
{
    private static readonly TimeSpan AskTimeout = TimeSpan.FromSeconds(10);
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
            AskTimeout,
            cancellationToken);
        return result.Organisations;
    }

    public async Task<OrganisationDto?> GetOrganisationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<GetOrganisationByIdResult>(
            new GetOrganisationByIdQuery(id),
            AskTimeout,
            cancellationToken);
        return result.Organisation;
    }

    public async Task<OrganisationDto> CreateOrganisationAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<CreateOrganisationResult>(
            new CreateOrganisationCommand(name),
            AskTimeout,
            cancellationToken);
        return result.Organisation;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<GetAllProjectsResult>(
            new GetAllProjectsQuery(),
            AskTimeout,
            cancellationToken);
        return result.Projects;
    }

    public async Task<ProjectDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<GetProjectByIdResult>(
            new GetProjectByIdQuery(id),
            AskTimeout,
            cancellationToken);
        return result.Project;
    }

    public async Task<GetProjectsByOrganisationResponse> GetProjectsForOrganisationAsync(
        Guid organisationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _rootActor.Ask<GetProjectsByOrganisationResult>(
            new GetProjectsByOrganisationQuery(organisationId),
            AskTimeout,
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
            AskTimeout,
            cancellationToken);
        return new CreateProjectResponse(result.OrganisationExists, result.Project);
    }
}
