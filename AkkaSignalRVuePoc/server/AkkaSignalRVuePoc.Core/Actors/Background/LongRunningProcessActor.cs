using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.InternalMessages;

namespace AkkaSignalRVuePoc.Core.Actors.Background;

public sealed class LongRunningProcessActor : ReceiveActor, IWithTimers
{
    private const string BusyTimerKey = "background-process-busy";
    private const string CompleteTimerKey = "background-process-complete";

    private readonly IActorRef _hubPushActor;
    private readonly string _processId;
    private readonly BackgroundProcessTiming _timing;
    private readonly string? _correlationId;
    private readonly string? _useCase;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private long _busySequence;

    public LongRunningProcessActor(
        IActorRef hubPushActor,
        string processId,
        BackgroundProcessTiming timing,
        string? correlationId = null,
        string? useCase = null)
    {
        _hubPushActor = hubPushActor;
        _processId = processId;
        _timing = timing;
        _correlationId = correlationId;
        _useCase = useCase;

        Receive<BackgroundProcessTick>(HandleTick);
    }

    public static Props Props(
        IActorRef hubPushActor,
        string processId,
        BackgroundProcessTiming? timing = null,
        string? correlationId = null,
        string? useCase = null) =>
        Akka.Actor.Props.Create(() => new LongRunningProcessActor(
            hubPushActor,
            processId,
            timing ?? BackgroundProcessTiming.Default,
            correlationId,
            useCase));

    public ITimerScheduler Timers { get; set; } = null!;

    protected override void PreStart()
    {
        PublishBusySignal();
        Timers.StartPeriodicTimer(BusyTimerKey, BackgroundProcessTick.Busy, _timing.BusySignalInterval);
        Timers.StartSingleTimer(CompleteTimerKey, BackgroundProcessTick.Complete, _timing.Duration);
        _log.Info("Long-running process {0} started", _processId);
    }

    protected override void PostStop()
    {
        Timers.Cancel(BusyTimerKey);
        Timers.Cancel(CompleteTimerKey);
    }

    private void HandleTick(BackgroundProcessTick tick)
    {
        if (tick == BackgroundProcessTick.Busy)
        {
            PublishBusySignal();
            return;
        }

        CompleteProcess();
    }

    private void PublishBusySignal()
    {
        var sequence = ++_busySequence;
        var message = new PushMessage(
            Sequence: sequence,
            Text: $"Background process {_processId} is busy (#{sequence})",
            SentAt: DateTimeOffset.UtcNow,
            Source: Self.Path.ToStringWithoutAddress(),
            CorrelationId: _correlationId,
            UseCase: _useCase);

        _hubPushActor.Tell(new PublishActorMessage(message));
    }

    private void CompleteProcess()
    {
        Timers.Cancel(BusyTimerKey);
        Timers.Cancel(CompleteTimerKey);

        Context.System.EventStream.Publish(new ProcessFinished(
            _processId,
            DateTimeOffset.UtcNow,
            _correlationId,
            _useCase));

        _log.Info("Long-running process {0} finished", _processId);
        Context.Stop(Self);
    }
}
