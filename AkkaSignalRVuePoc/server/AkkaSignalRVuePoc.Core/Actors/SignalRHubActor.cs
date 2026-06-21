using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Contracts.Events;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Contracts.Notifications;
using AkkaSignalRVuePoc.Core.Publishing;
using Platform.Serilog.Logging.Correlation;

namespace AkkaSignalRVuePoc.Core.Actors;

public sealed class SignalRHubActor : ReceiveActor
{
    private readonly ISignalrHubWrapper _publisher;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private long _processFinishedSequence;

    public SignalRHubActor(ISignalrHubWrapper publisher)
    {
        _publisher = publisher;
        Context.System.EventStream.Subscribe<ProcessFinished>(Self);
        Context.System.EventStream.Subscribe<IDataEvent>(Self);
        _log.Info("SignalRHubActor created");
        ReceiveAsync<PublishActorMessage>(PublishAsync);
        ReceiveAsync<CorrelatedMessageEnvelope>(PublishCorrelatedAsync);
        ReceiveAsync<ProcessFinished>(PublishProcessFinishedAsync);
        ReceiveAsync<IDataEvent>(PublishDataEventAsync);
    }

    protected override void PostStop()
    {
        Context.System.EventStream.Unsubscribe<ProcessFinished>(Self);
        Context.System.EventStream.Unsubscribe<IDataEvent>(Self);
        _log.Info("SignalRHubActor stopped and unsubscribed from event stream");
    }

    public static Props Props(ISignalrHubWrapper publisher) =>
        Akka.Actor.Props.Create(() => new SignalRHubActor(publisher));

    private async Task PublishAsync(PublishActorMessage message)
    {
        await _publisher.PublishActorMessageAsync(message.Message);
        _log.Info(
            "Published actor message {0} correlation {1} to SignalR clients",
            message.Message.Sequence,
            message.Message.CorrelationId);
    }

    private async Task PublishCorrelatedAsync(CorrelatedMessageEnvelope envelope)
    {
        if (envelope.Message is not PublishActorMessage message)
        {
            Unhandled(envelope);
            return;
        }

        var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
        using CorrelationScope scope = flow.BeginScope();
        await PublishAsync(message);
    }

    private async Task PublishProcessFinishedAsync(ProcessFinished finished)
    {
        var message = new PushMessage(
            Sequence: ++_processFinishedSequence,
            Text: $"Background process {finished.ProcessId} finished at {finished.FinishedAt:O}",
            SentAt: DateTimeOffset.UtcNow,
            Source: Self.Path.ToStringWithoutAddress(),
            CorrelationId: finished.CorrelationId,
            UseCase: finished.UseCase);

        await _publisher.PublishActorMessageAsync(message);
        _log.Info("Published process finished event for {0}", finished.ProcessId);
    }

    private async Task PublishDataEventAsync(IDataEvent dataEvent)
    {
        var notification = dataEvent switch
        {
            ProjectCreated created => new DataEventNotification(
                nameof(ProjectCreated),
                created.Project,
                created.OccurredAt,
                created.CorrelationId,
                created.UseCase),
            ProjectUpdated updated => new DataEventNotification(
                nameof(ProjectUpdated),
                updated.Project,
                updated.OccurredAt,
                updated.CorrelationId,
                updated.UseCase),
            ProjectDeleted deleted => new DataEventNotification(
                nameof(ProjectDeleted),
                deleted.Project,
                deleted.OccurredAt,
                deleted.CorrelationId,
                deleted.UseCase),
            _ => null
        };

        if (notification is null)
        {
            return;
        }

        await _publisher.PublishDataEventAsync(notification);
        _log.Info(
            "Published data event {0} for project {1} correlation {2}",
            notification.EventType,
            notification.Project.Id,
            notification.CorrelationId);
    }
}
