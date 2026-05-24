using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class LiveMessageRootActor : ReceiveActor
{
    private readonly IActorRef _hubPushActor;
    private readonly BackgroundProcessTiming _backgroundProcessTiming;
    private IActorRef _backgroundManager = ActorRefs.Nobody;
    private long _sequence;

    public LiveMessageRootActor(IActorRef hubPushActor, BackgroundProcessTiming? backgroundProcessTiming = null)
    {
        _hubPushActor = hubPushActor;
        _backgroundProcessTiming = backgroundProcessTiming ?? BackgroundProcessTiming.Default;

        Context.System.EventStream.Publish(new ActorSystemStarted(
            Context.System.Name,
            DateTimeOffset.UtcNow));
        Receive<PublishLiveMessageCommand>(HandlePublish);
        Receive<StartBackgroundProcessCommand>(ForwardToBackgroundManager);
    }

    public static Props Props(IActorRef hubPushActor, BackgroundProcessTiming? backgroundProcessTiming = null) =>
        Akka.Actor.Props.Create(() => new LiveMessageRootActor(hubPushActor, backgroundProcessTiming));

    protected override void PreStart()
    {
        _backgroundManager = Context.ActorOf(
            BackgroundManagerActor.Props(_hubPushActor, _backgroundProcessTiming),
            "background-manager");
    }

    private void ForwardToBackgroundManager(StartBackgroundProcessCommand command)
    {
        _backgroundManager.Forward(command);
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
