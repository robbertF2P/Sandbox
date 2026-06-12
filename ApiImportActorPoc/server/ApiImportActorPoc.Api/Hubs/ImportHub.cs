using Microsoft.AspNetCore.SignalR;

namespace ApiImportActorPoc.Api.Hubs;

public sealed class ImportHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("serverMessage", new
        {
            Text = "Connected to import hub. Start an import to receive progress events.",
            SentAt = DateTimeOffset.UtcNow
        });

        await base.OnConnectedAsync();
    }
}
