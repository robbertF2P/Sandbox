using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class LiveMessageRootActor : ReceiveActor
{
    private readonly IActorRef _hubPushActor;
    private long _sequence;

    public LiveMessageRootActor(IActorRef hubPushActor)
    {
        _hubPushActor = hubPushActor;

        Context.System.EventStream.Publish(new ActorSystemStarted(
            Context.System.Name,
            DateTimeOffset.UtcNow));
        Receive<PublishLiveMessageCommand>(HandlePublish);
    }

    public static Props Props(IActorRef hubPushActor) =>
        Akka.Actor.Props.Create(() => new LiveMessageRootActor(hubPushActor));

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

        if (!Sender.IsNobody())
        {
            Sender.Tell(message);
        }
    }
}
