using Akka.Actor;
using Akka.Event;
using ApiImportActorPoc.Contracts.Events;
using ApiImportActorPoc.Contracts.Notifications;
using ApiImportActorPoc.Core.Publishing;

namespace ApiImportActorPoc.Core.Actors;

public sealed class SignalRHubActor : ReceiveActor
{
    private readonly ISignalrHubWrapper _publisher;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public SignalRHubActor(ISignalrHubWrapper publisher)
    {
        _publisher = publisher;
        Context.System.EventStream.Subscribe<IDataEvent>(Self);
        _log.Info("SignalRHubActor created");
        ReceiveAsync<IDataEvent>(PublishDataEventAsync);
    }

    protected override void PostStop()
    {
        Context.System.EventStream.Unsubscribe<IDataEvent>(Self);
        _log.Info("SignalRHubActor stopped");
    }

    public static Props Props(ISignalrHubWrapper publisher) =>
        Akka.Actor.Props.Create(() => new SignalRHubActor(publisher));

    private async Task PublishDataEventAsync(IDataEvent dataEvent)
    {
        var notification = dataEvent switch
        {
            ImportStarted started => new ImportEventNotification(
                nameof(ImportStarted),
                started.SessionId,
                new { started.ProjectName },
                started.OccurredAt),
            ImportProgressUpdated progress => new ImportEventNotification(
                nameof(ImportProgressUpdated),
                progress.SessionId,
                new { progress.Step, progress.TotalSteps, progress.Message },
                progress.OccurredAt),
            ImportCompleted completed => new ImportEventNotification(
                nameof(ImportCompleted),
                completed.SessionId,
                new { completed.Model.Id, completed.Model.Name, ComponentCount = completed.Model.Components.Count },
                completed.OccurredAt),
            ImportFailed failed => new ImportEventNotification(
                nameof(ImportFailed),
                failed.SessionId,
                new { failed.ErrorMessage },
                failed.OccurredAt),
            ImportPersisted persisted => new ImportEventNotification(
                nameof(ImportPersisted),
                persisted.SessionId,
                new { persisted.ProjectId },
                persisted.OccurredAt),
            _ => null
        };

        if (notification is null)
        {
            return;
        }

        await _publisher.PublishImportEventAsync(notification);
        _log.Info("Published import event {0} for session {1}", notification.EventType, notification.SessionId);
    }
}
