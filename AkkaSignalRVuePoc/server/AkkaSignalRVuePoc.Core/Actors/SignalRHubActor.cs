using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.Publishing;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class SignalRHubActor : ReceiveActor
{
    private readonly ISignalrHubWrapper _publisher;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private long _processFinishedSequence;

    public SignalRHubActor(ISignalrHubWrapper publisher)
    {
        _publisher = publisher;
        Context.System.EventStream.Subscribe(Self, typeof(ProcessFinished));
        _log.Info("SignalRHubActor created");
        ReceiveAsync<PublishActorMessage>(PublishAsync);
        ReceiveAsync<ProcessFinished>(PublishProcessFinishedAsync);
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

    private async Task PublishProcessFinishedAsync(ProcessFinished finished)
    {
        var message = new PushMessage(
            Sequence: ++_processFinishedSequence,
            Text: $"Background process {finished.ProcessId} finished at {finished.FinishedAt:O}",
            SentAt: DateTimeOffset.UtcNow,
            Source: Self.Path.ToStringWithoutAddress());

        await _publisher.PublishActorMessageAsync(message);
        _log.Info("Published process finished event for {0}", finished.ProcessId);
    }
}
