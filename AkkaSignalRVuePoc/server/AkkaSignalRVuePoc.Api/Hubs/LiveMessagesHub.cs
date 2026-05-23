using AkkaSignalRVuePoc.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Hubs;

public sealed class LiveMessagesHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("serverMessage", new PushMessage(
            Sequence: 0,
            Text: "Connected to the SignalR hub. Waiting for Akka.NET actor messages...",
            SentAt: DateTimeOffset.UtcNow,
            Source: "SignalR Hub"));

        await base.OnConnectedAsync();
    }
}
