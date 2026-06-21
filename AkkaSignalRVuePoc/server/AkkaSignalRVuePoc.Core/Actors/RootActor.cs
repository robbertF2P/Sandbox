using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Core.Actors.Background;
using AkkaSignalRVuePoc.Core.Actors.Data;
using AkkaSignalRVuePoc.Data;
using Microsoft.EntityFrameworkCore;
using Platform.Serilog.Logging.Correlation;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class RootActor : ReceiveActor
{
    private readonly IActorRef _hubPushActor;
    private readonly IDbContextFactory<CatalogDbContext> _dbContextFactory;
    private readonly BackgroundProcessTiming _backgroundProcessTiming;
    private IActorRef _backgroundManager = ActorRefs.Nobody;
    private IActorRef _dataManager = ActorRefs.Nobody;
    private long _sequence;

    public RootActor(
        IActorRef hubPushActor,
        IDbContextFactory<CatalogDbContext> dbContextFactory,
        BackgroundProcessTiming? backgroundProcessTiming = null)
    {
        _hubPushActor = hubPushActor;
        _dbContextFactory = dbContextFactory;
        _backgroundProcessTiming = backgroundProcessTiming ?? BackgroundProcessTiming.Default;

        Context.System.EventStream.Publish(new ActorSystemStarted(
            Context.System.Name,
            DateTimeOffset.UtcNow));

        ReceiveAsync<CorrelatedMessageEnvelope>(DispatchAsync);
    }

    public static Props Props(
        IActorRef hubPushActor,
        IDbContextFactory<CatalogDbContext> dbContextFactory,
        BackgroundProcessTiming? backgroundProcessTiming = null) =>
        Akka.Actor.Props.Create(() => new RootActor(hubPushActor, dbContextFactory, backgroundProcessTiming));

    protected override void PreStart()
    {
        _backgroundManager = Context.ActorOf(
            BackgroundManagerActor.Props(_hubPushActor, _backgroundProcessTiming),
            "background-manager");
        _dataManager = Context.ActorOf(
            DataManagerActor.Props(_dbContextFactory),
            "data-manager");
    }

    private async Task DispatchAsync(CorrelatedMessageEnvelope envelope)
    {
        var sender = Sender;
        var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
        using CorrelationScope scope = flow.BeginScope();

        switch (envelope.Message)
        {
            case PublishLiveMessageCommand command:
                HandlePublish(command, flow, sender);
                break;
            case StartBackgroundProcessCommand command:
                _backgroundManager.Forward(flow.Wrap(command));
                break;
            case GetAllOrganisationsQuery query:
                _dataManager.Forward(flow.Wrap(query));
                break;
            case GetOrganisationByIdQuery query:
                _dataManager.Forward(flow.Wrap(query));
                break;
            case CreateOrganisationCommand command:
                _dataManager.Forward(flow.Wrap(command));
                break;
            case GetAllProjectsQuery query:
                _dataManager.Forward(flow.Wrap(query));
                break;
            case GetProjectByIdQuery query:
                _dataManager.Forward(flow.Wrap(query));
                break;
            case GetProjectsByOrganisationQuery query:
                _dataManager.Forward(flow.Wrap(query));
                break;
            case CreateProjectCommand command:
                _dataManager.Forward(flow.Wrap(command));
                break;
            case UpdateProjectCommand command:
                _dataManager.Forward(flow.Wrap(command));
                break;
            case DeleteProjectCommand command:
                _dataManager.Forward(flow.Wrap(command));
                break;
            default:
                Unhandled(envelope);
                break;
        }

        await Task.CompletedTask;
    }

    private void HandlePublish(PublishLiveMessageCommand command, CorrelationFlow flow, IActorRef sender)
    {
        if (string.IsNullOrWhiteSpace(command.Text))
        {
            if (!sender.IsNobody())
            {
                sender.Tell(new Status.Failure(new ArgumentException("Text is required.", nameof(command.Text))));
            }

            return;
        }

        var message = new PushMessage(
            Sequence: ++_sequence,
            Text: command.Text.Trim(),
            SentAt: DateTimeOffset.UtcNow,
            Source: Self.Path.ToStringWithoutAddress(),
            CorrelationId: flow.CorrelationId,
            UseCase: flow.UseCase);

        _hubPushActor.Tell(flow.Wrap(new PublishActorMessage(message)));
    }
}
