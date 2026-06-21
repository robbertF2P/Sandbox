using F2pPlatform.Host.Contracts.Notifications;
using F2pPlatform.Host.Core.Publishing;
using F2pPlatform.Host.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace F2pPlatform.Host.Services;

public sealed class SignalRPlatformHubPublisher(IHubContext<PlatformEventsHub> hubContext) : IPlatformHubPublisher
{
    public Task PublishPlatformEventAsync(PlatformEventNotification notification)
    {
        return hubContext.Clients.All.SendAsync("platformEvent", notification);
    }
}
