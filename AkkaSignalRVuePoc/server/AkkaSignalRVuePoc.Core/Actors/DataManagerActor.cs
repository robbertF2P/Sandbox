using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Data;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Data;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class DataManagerActor : ReceiveActor
{
    private readonly IDbContextFactory<CatalogDbContext> _dbContextFactory;
    private IActorRef _organisationData = ActorRefs.Nobody;
    private IActorRef _projectData = ActorRefs.Nobody;

    public DataManagerActor(IDbContextFactory<CatalogDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;

        Receive<GetAllOrganisationsQuery>(message => _organisationData.Forward(message));
        Receive<GetOrganisationByIdQuery>(message => _organisationData.Forward(message));
        Receive<CreateOrganisationCommand>(message => _organisationData.Forward(message));

        Receive<GetAllProjectsQuery>(message => _projectData.Forward(message));
        Receive<GetProjectByIdQuery>(message => _projectData.Forward(message));
        Receive<GetProjectsByOrganisationQuery>(message => _projectData.Forward(message));

        ReceiveAsync<CreateProjectCommand>(HandleCreateProjectAsync);
        ReceiveAsync<UpdateProjectCommand>(HandleUpdateProjectAsync);
    }

    public static Props Props(IDbContextFactory<CatalogDbContext> dbContextFactory) =>
        Akka.Actor.Props.Create(() => new DataManagerActor(dbContextFactory));

    protected override void PreStart()
    {
        _organisationData = Context.ActorOf(
            OrganisationDataActor.Props(_dbContextFactory),
            "organisation-data");
        _projectData = Context.ActorOf(
            ProjectDataActor.Props(_dbContextFactory),
            "project-data");
    }

    private async Task HandleCreateProjectAsync(CreateProjectCommand command)
    {
        var sender = Sender;
        var result = await _projectData.Ask<CreateProjectResult>(command);
        if (result.OrganisationExists && result.Project is { } project)
        {
            PublishProjectCreated(project);
        }

        sender.Tell(result);
    }

    private async Task HandleUpdateProjectAsync(UpdateProjectCommand command)
    {
        var sender = Sender;
        var result = await _projectData.Ask<UpdateProjectResult>(command);
        if (result.Exists && result.Project is { } project)
        {
            PublishProjectUpdated(project);
        }

        sender.Tell(result);
    }

    private void PublishProjectCreated(ProjectDto project)
    {
        Context.System.EventStream.Publish(new ProjectCreated(project, DateTimeOffset.UtcNow));
    }

    private void PublishProjectUpdated(ProjectDto project)
    {
        Context.System.EventStream.Publish(new ProjectUpdated(project, DateTimeOffset.UtcNow));
    }
}
