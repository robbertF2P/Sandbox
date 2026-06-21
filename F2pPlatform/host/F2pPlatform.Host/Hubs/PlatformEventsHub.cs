using Microsoft.AspNetCore.SignalR;

namespace F2pPlatform.Host.Hubs;

public sealed class PlatformEventsHub : Hub
{
    public override Task OnConnectedAsync()
    {
        return Clients.Caller.SendAsync(
            "platformEvent",
            new
            {
                EventType = "Connected",
                Payload = new { Message = "Connected to platform events hub." },
                OccurredAt = DateTimeOffset.UtcNow,
            });
    }
}
