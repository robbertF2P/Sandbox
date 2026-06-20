using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Data;
using Microsoft.EntityFrameworkCore;
using Platform.Serilog.Logging.Correlation;

namespace AkkaSignalRVuePoc.Core.Actors.Data;

public sealed class DataManagerActor : ReceiveActor
{
    private readonly IDbContextFactory<CatalogDbContext> _dbContextFactory;
    private IActorRef _organisationData = ActorRefs.Nobody;
    private IActorRef _projectData = ActorRefs.Nobody;

    public DataManagerActor(IDbContextFactory<CatalogDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        ReceiveAsync<CorrelatedMessageEnvelope>(DispatchAsync);
    }

    private async Task DispatchAsync(CorrelatedMessageEnvelope envelope)
    {
        var sender = Sender;
        var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
        using CorrelationScope scope = flow.BeginScope();

        switch (envelope.Message)
        {
            case GetAllOrganisationsQuery query:
                _organisationData.Forward(flow.Wrap(query));
                break;
            case GetOrganisationByIdQuery query:
                _organisationData.Forward(flow.Wrap(query));
                break;
            case CreateOrganisationCommand command:
                _organisationData.Forward(flow.Wrap(command));
                break;
            case GetAllProjectsQuery query:
                _projectData.Forward(flow.Wrap(query));
                break;
            case GetProjectByIdQuery query:
                _projectData.Forward(flow.Wrap(query));
                break;
            case GetProjectsByOrganisationQuery query:
                _projectData.Forward(flow.Wrap(query));
                break;
            case CreateProjectCommand command:
                _projectData.Tell(flow.Wrap(command));
                Become(() => WaitForResult(flow, sender));
                break;
            case UpdateProjectCommand command:
                _projectData.Tell(flow.Wrap(command));
                Become(() => WaitForResult(flow, sender));
                break;
            case DeleteProjectCommand command:
                _projectData.Tell(flow.Wrap(command));
                Become(() => WaitForResult(flow, sender));
                break;
            default:
                Unhandled(envelope);
                break;
        }

        await Task.CompletedTask;
    }

    private void WaitForResult(CorrelationFlow flow, IActorRef originalSender)
    {
        Receive<Failure>(msg =>
        {
            using CorrelationScope scope = flow.BeginScope();
            originalSender.Tell(msg);
            Become(Ready);
        });
        Receive<CreateProjectResult>(result =>
        {
            using CorrelationScope scope = flow.BeginScope();
            if (result.Project != null)
            {
                Context.System.EventStream.Publish(new ProjectCreated(
                    result.Project,
                    DateTimeOffset.UtcNow,
                    flow.CorrelationId,
                    flow.UseCase));
            }

            originalSender.Tell(result);
            Become(Ready);
        });
        Receive<UpdateProjectResult>(result =>
        {
            using CorrelationScope scope = flow.BeginScope();
            if (result.Project != null)
            {
                Context.System.EventStream.Publish(new ProjectUpdated(
                    result.Project,
                    DateTimeOffset.UtcNow,
                    flow.CorrelationId,
                    flow.UseCase));
            }

            originalSender.Tell(result);
            Become(Ready);
        });
        Receive<DeleteProjectResult>(result =>
        {
            using CorrelationScope scope = flow.BeginScope();
            if (result.Project != null)
            {
                Context.System.EventStream.Publish(new ProjectDeleted(
                    result.Project,
                    DateTimeOffset.UtcNow,
                    flow.CorrelationId,
                    flow.UseCase));
            }

            originalSender.Tell(result);
            Become(Ready);
        });
    }

    private void Ready()
    {
        ReceiveAsync<CorrelatedMessageEnvelope>(DispatchAsync);
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
