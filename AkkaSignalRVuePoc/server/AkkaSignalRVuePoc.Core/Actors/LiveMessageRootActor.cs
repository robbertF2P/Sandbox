using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Contracts.Messages.Data;
using AkkaSignalRVuePoc.Data;
using Microsoft.EntityFrameworkCore;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class LiveMessageRootActor : ReceiveActor
{
    private readonly IActorRef _hubPushActor;
    private readonly IDbContextFactory<CatalogDbContext> _dbContextFactory;
    private readonly BackgroundProcessTiming _backgroundProcessTiming;
    private IActorRef _backgroundManager = ActorRefs.Nobody;
    private IActorRef _dataManager = ActorRefs.Nobody;
    private long _sequence;

    public LiveMessageRootActor(
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

        Receive<PublishLiveMessageCommand>(HandlePublish);
        Receive<StartBackgroundProcessCommand>(ForwardToBackgroundManager);

        Receive<GetAllOrganisationsQuery>(ForwardToDataManager);
        Receive<GetOrganisationByIdQuery>(ForwardToDataManager);
        Receive<CreateOrganisationCommand>(ForwardToDataManager);
        Receive<GetAllProjectsQuery>(ForwardToDataManager);
        Receive<GetProjectByIdQuery>(ForwardToDataManager);
        Receive<GetProjectsByOrganisationQuery>(ForwardToDataManager);
        Receive<CreateProjectCommand>(ForwardToDataManager);
        Receive<UpdateProjectCommand>(ForwardToDataManager);
    }

    public static Props Props(
        IActorRef hubPushActor,
        IDbContextFactory<CatalogDbContext> dbContextFactory,
        BackgroundProcessTiming? backgroundProcessTiming = null) =>
        Akka.Actor.Props.Create(() => new LiveMessageRootActor(hubPushActor, dbContextFactory, backgroundProcessTiming));

    protected override void PreStart()
    {
        _backgroundManager = Context.ActorOf(
            BackgroundManagerActor.Props(_hubPushActor, _backgroundProcessTiming),
            "background-manager");
        _dataManager = Context.ActorOf(
            DataManagerActor.Props(_dbContextFactory),
            "data-manager");
    }

    private void ForwardToBackgroundManager(StartBackgroundProcessCommand command)
    {
        _backgroundManager.Forward(command);
    }

    private void ForwardToDataManager(object message)
    {
        _dataManager.Forward(message);
    }

    private void HandlePublish(PublishLiveMessageCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Text))
        {
            if (!Sender.IsNobody())
            {
                Sender.Tell(new Status.Failure(new ArgumentException("Text is required.", nameof(command.Text))));
            }

            return;
        }

        var message = new PushMessage(
            Sequence: ++_sequence,
            Text: command.Text.Trim(),
            SentAt: DateTimeOffset.UtcNow,
            Source: Self.Path.ToStringWithoutAddress());

        _hubPushActor.Tell(new PublishActorMessage(message));
    }
}
