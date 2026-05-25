using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.InternalMessages;

namespace AkkaSignalRVuePoc.Core.Actors;

/// <summary>
/// This actor simulates a heartbeat ticker that periodically sends messages to the frontend via the hub push actor. It can publish immediately on startup and then continue at regular intervals, which is useful for demonstrating real-time updates to connected clients.
/// </summary>
public sealed class FrontendPushActor : ReceiveActor, IWithTimers
{
    private const string TimerKey = "frontend-message-push";
    private static readonly TimeSpan DefaultPushInterval = TimeSpan.FromSeconds(5);

    private readonly IActorRef _hubPushActor;
    private readonly TimeSpan _pushInterval;
    private readonly bool _publishImmediately;
    private long _sequence;

    public FrontendPushActor(
        IActorRef hubPushActor,
        TimeSpan pushInterval,
        bool publishImmediately)
    {
        _hubPushActor = hubPushActor;
        _pushInterval = pushInterval;
        _publishImmediately = publishImmediately;
        Context.System.EventStream.Subscribe(Self, typeof(ActorSystemStarted));

        Receive<PushTick>(_ => PublishHeartbeat());
        Receive<ActorSystemStarted>(msg => PublishMessage("Actor system started"));
    }

    public static Props Props(
        IActorRef hubPushActor,
        TimeSpan? pushInterval = null,
        bool publishImmediately = true)
    {
        var interval = pushInterval ?? DefaultPushInterval;
        return Akka.Actor.Props.Create(() => new FrontendPushActor(hubPushActor, interval, publishImmediately));
    }

    public ITimerScheduler Timers { get; set; } = null!;

    protected override void PreStart()
    {
        if (_publishImmediately)
        {
            Self.Tell(PushTick.Instance);
        }

        Timers.StartPeriodicTimer(TimerKey, PushTick.Instance, _pushInterval);
    }

    protected override void PostStop()
    {
        Timers.Cancel(TimerKey);
    }

    private void PublishMessage(string messageText)
    {
        var sequence = ++_sequence;
        var message = new PushMessage(
            Sequence: sequence,
            Text: messageText,
            SentAt: DateTimeOffset.UtcNow,
            Source: Self.Path.ToStringWithoutAddress());

        _hubPushActor.Tell(new PublishActorMessage(message));
    }

    private void PublishHeartbeat()
    {
        var sequence = _sequence + 1;
        PublishMessage($"Akka.NET actor heartbeat #{sequence}");
    }
}
