using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Messages;

namespace AkkaSignalRVuePoc.Core.Actors.Background;

public sealed class BackgroundManagerActor : ReceiveActor
{
    private readonly IActorRef _hubPushActor;
    private readonly BackgroundProcessTiming _timing;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private int _processCounter;
    private IActorRef? _activeProcess;

    public BackgroundManagerActor(IActorRef hubPushActor, BackgroundProcessTiming timing)
    {
        _hubPushActor = hubPushActor;
        _timing = timing;
        Receive<StartBackgroundProcessCommand>(HandleStart);
        Receive<Terminated>(_ => _activeProcess = null);
    }

    public static Props Props(IActorRef hubPushActor, BackgroundProcessTiming? timing = null) =>
        Akka.Actor.Props.Create(() => new BackgroundManagerActor(
            hubPushActor,
            timing ?? BackgroundProcessTiming.Default));

    private void HandleStart(StartBackgroundProcessCommand command)
    {
        if (_activeProcess is { } existing && !existing.IsNobody())
        {
            _log.Warning("Background process already running at {0}", existing.Path);
            return;
        }

        var processId = $"background-process-{++_processCounter}";
        _activeProcess = Context.ActorOf(
            LongRunningProcessActor.Props(_hubPushActor, processId, _timing),
            processId);
        Context.Watch(_activeProcess);

        _log.Info("Started long-running background process {0}", processId);
    }
}
