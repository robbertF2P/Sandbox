using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.Publishing;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class SignalRHubPushActor : ReceiveActor
{
    private readonly ILiveMessageClientPublisher _publisher;

    public SignalRHubPushActor(ILiveMessageClientPublisher publisher)
    {
        _publisher = publisher;

        ReceiveAsync<PublishActorMessage>(PublishAsync);
    }

    public static Props Props(ILiveMessageClientPublisher publisher) =>
        Akka.Actor.Props.Create(() => new SignalRHubPushActor(publisher));

    private async Task PublishAsync(PublishActorMessage message)
    {
        await _publisher.PublishActorMessageAsync(message.Message);
        Context.GetLogger().Info(
            "Published actor message {0} to SignalR clients",
            message.Message.Sequence);
    }
}
