using Akka.Actor;
using AkkaSignalRVuePoc.Api.Models;

namespace AkkaSignalRVuePoc.Api.Actors;

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

        Receive<PushTick>(_ => PublishMessage());
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

    private void PublishMessage()
    {
        var sequence = ++_sequence;
        var message = new PushMessage(
            Sequence: sequence,
            Text: $"Akka.NET actor heartbeat #{sequence}",
            SentAt: DateTimeOffset.UtcNow,
            Source: Self.Path.ToStringWithoutAddress());

        _hubPushActor.Tell(new PublishActorMessage(message));
    }
}
