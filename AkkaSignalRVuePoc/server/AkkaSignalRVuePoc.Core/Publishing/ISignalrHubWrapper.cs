using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Contracts.Notifications;

namespace AkkaSignalRVuePoc.Core.Publishing;

public interface ISignalrHubWrapper
{
    Task PublishActorMessageAsync(PushMessage message);

    Task PublishDataEventAsync(DataEventNotification notification);
}
