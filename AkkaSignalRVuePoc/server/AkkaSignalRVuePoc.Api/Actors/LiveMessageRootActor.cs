using Akka.Actor;
using AkkaSignalRVuePoc.Api.Models;

namespace AkkaSignalRVuePoc.Api.Actors;

public sealed class LiveMessageRootActor : ReceiveActor
{
    private readonly IActorRef _hubPushActor;
    private long _sequence;

    public LiveMessageRootActor(IActorRef hubPushActor)
    {
        _hubPushActor = hubPushActor;

        ReceiveAsync<PublishLiveMessageCommand>(HandlePublishAsync);
    }

    public static Props Props(IActorRef hubPushActor) =>
        Akka.Actor.Props.Create(() => new LiveMessageRootActor(hubPushActor));

    private Task HandlePublishAsync(PublishLiveMessageCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Text))
        {
            if (!Sender.IsNobody())
            {
                Sender.Tell(new Status.Failure(new ArgumentException("Text is required.", nameof(command.Text))));
            }

            return Task.CompletedTask;
        }

        var message = new PushMessage(
            Sequence: Interlocked.Increment(ref _sequence),
            Text: command.Text.Trim(),
            SentAt: DateTimeOffset.UtcNow,
            Source: Self.Path.ToStringWithoutAddress());

        _hubPushActor.Tell(new PublishActorMessage(message));

        if (!Sender.IsNobody())
        {
            Sender.Tell(message);
        }

        return Task.CompletedTask;
    }
}
