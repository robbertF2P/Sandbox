using Akka.Actor;
using Akka.Event;
using F2pPlatform.Host.Contracts.Events;
using F2pPlatform.Host.Contracts.Notifications;
using F2pPlatform.Host.Core.Publishing;

namespace F2pPlatform.Host.Core.Actors;

public sealed class PlatformSignalRHubActor : ReceiveActor
{
    private readonly IPlatformHubPublisher _publisher;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public PlatformSignalRHubActor(IPlatformHubPublisher publisher)
    {
        _publisher = publisher;
        Context.System.EventStream.Subscribe<IDataEvent>(Self);
        ReceiveAsync<IDataEvent>(PublishDataEventAsync);
    }

    protected override void PostStop()
    {
        Context.System.EventStream.Unsubscribe<IDataEvent>(Self);
        base.PostStop();
    }

    public static Props Props(IPlatformHubPublisher publisher) =>
        Akka.Actor.Props.Create(() => new PlatformSignalRHubActor(publisher));

    private async Task PublishDataEventAsync(IDataEvent dataEvent)
    {
        var notification = dataEvent switch
        {
            PlatformStarted started => new PlatformEventNotification(
                nameof(PlatformStarted),
                new { started.OccurredAt },
                started.OccurredAt,
                started.CorrelationId,
                started.UseCase),
            _ => null,
        };

        if (notification is null)
        {
            return;
        }

        await _publisher.PublishPlatformEventAsync(notification);
        _log.Info(
            "Published platform event {0} correlation {1}",
            notification.EventType,
            notification.CorrelationId);
    }
}
