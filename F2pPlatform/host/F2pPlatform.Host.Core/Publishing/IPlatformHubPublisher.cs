namespace F2pPlatform.Host.Core.Publishing;

public interface IPlatformHubPublisher
{
    Task PublishPlatformEventAsync(Contracts.Notifications.PlatformEventNotification notification);
}
