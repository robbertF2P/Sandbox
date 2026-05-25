using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Data;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Core.Actors.Data;

public sealed class DataManagerActor : ReceiveActor
{
    private readonly IDbContextFactory<CatalogDbContext> _dbContextFactory;
    private IActorRef _organisationData = ActorRefs.Nobody;
    private IActorRef _projectData = ActorRefs.Nobody;

    public DataManagerActor(IDbContextFactory<CatalogDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    private void Ready()
    {
        Receive<GetAllOrganisationsQuery>(_organisationData.Forward);
        Receive<GetOrganisationByIdQuery>(_organisationData.Forward);
        Receive<CreateOrganisationCommand>(_organisationData.Forward);

        Receive<GetAllProjectsQuery>(_projectData.Forward);
        Receive<GetProjectByIdQuery>(_projectData.Forward);
        Receive<GetProjectsByOrganisationQuery>(_projectData.Forward);

        Receive<CreateProjectCommand>(cmd =>
        {
            _projectData.Tell(cmd);
            Become(() => WaitForResult(Sender));
        });
        Receive<UpdateProjectCommand>(cmd =>
        {
            _projectData.Tell(cmd);
            Become(() => WaitForResult(Sender));
        });
        Receive<DeleteProjectCommand>(cmd =>
        {
            _projectData.Tell(cmd);
            Become(() => WaitForResult(Sender));
        });
    }

    private void WaitForResult(IActorRef originalSender)
    {
        Receive<Failure>(msg =>
        {
            originalSender.Tell(msg);
            Become(Ready);
        });
        Receive<CreateProjectResult>(result =>
        {
            if (result.Project != null)
            {
                Context.System.EventStream.Publish(new ProjectCreated(result.Project, DateTimeOffset.UtcNow));
            }

            originalSender.Tell(result);
            Become(Ready);
        });
        Receive<UpdateProjectResult>(result =>
        {
            if (result.Project != null)
            {
                Context.System.EventStream.Publish(new ProjectUpdated(result.Project, DateTimeOffset.UtcNow));
            }

            originalSender.Tell(result);
            Become(Ready);
        });
        Receive<DeleteProjectResult>(result =>
        {
            if (result.Project != null)
            {
                Context.System.EventStream.Publish(new ProjectDeleted(result.Project, DateTimeOffset.UtcNow));
            }

            originalSender.Tell(result);
            Become(Ready);
        });
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
        Become(Ready);
    }
}
