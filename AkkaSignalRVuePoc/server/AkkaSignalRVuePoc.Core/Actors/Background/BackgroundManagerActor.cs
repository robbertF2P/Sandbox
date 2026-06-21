using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Messages;
using Platform.Serilog.Logging.Correlation;

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
        ReceiveAsync<CorrelatedMessageEnvelope>(DispatchAsync);
        Receive<Terminated>(_ => _activeProcess = null);
    }

    public static Props Props(IActorRef hubPushActor, BackgroundProcessTiming? timing = null) =>
        Akka.Actor.Props.Create(() => new BackgroundManagerActor(
            hubPushActor,
            timing ?? BackgroundProcessTiming.Default));

    private Task DispatchAsync(CorrelatedMessageEnvelope envelope)
    {
        var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
        using CorrelationScope scope = flow.BeginScope();

        if (envelope.Message is StartBackgroundProcessCommand command)
        {
            HandleStart(command, flow);
            return Task.CompletedTask;
        }

        Unhandled(envelope);
        return Task.CompletedTask;
    }

    private void HandleStart(StartBackgroundProcessCommand command, CorrelationFlow flow)
    {
        if (_activeProcess is { } existing && !existing.IsNobody())
        {
            _log.Warning("Background process already running at {0}", existing.Path);
            return;
        }

        var processId = $"background-process-{++_processCounter}";
        _activeProcess = Context.ActorOf(
            LongRunningProcessActor.Props(
                _hubPushActor,
                processId,
                _timing,
                flow.CorrelationId,
                flow.UseCase),
            processId);
        Context.Watch(_activeProcess);

        _log.Info(
            "Started long-running background process {0} correlation {1}",
            processId,
            flow.CorrelationId);
    }
}
