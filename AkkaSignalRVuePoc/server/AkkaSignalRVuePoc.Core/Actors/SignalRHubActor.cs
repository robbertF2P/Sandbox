using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.Publishing;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class SignalRHubActor : ReceiveActor
{
    private readonly ISignalrHubWrapper _publisher;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public SignalRHubActor(ISignalrHubWrapper publisher)
    {
        _publisher = publisher;
        _log.Info("SignalRHubActor created");
        ReceiveAsync<PublishActorMessage>(PublishAsync);
    }

    public static Props Props(ISignalrHubWrapper publisher) =>
        Akka.Actor.Props.Create(() => new SignalRHubActor(publisher));

    private async Task PublishAsync(PublishActorMessage message)
    {
        await _publisher.PublishActorMessageAsync(message.Message);
        _log.Info(
            "Published actor message {0} to SignalR clients",
            message.Message.Sequence);
    }
}
