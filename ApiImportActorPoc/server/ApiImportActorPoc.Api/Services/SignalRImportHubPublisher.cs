using ApiImportActorPoc.Api.Hubs;
using ApiImportActorPoc.Contracts.Notifications;
using ApiImportActorPoc.Core.Publishing;
using Microsoft.AspNetCore.SignalR;

namespace ApiImportActorPoc.Api.Services;

public sealed class SignalRImportHubPublisher : ISignalrHubWrapper
{
    private readonly IHubContext<ImportHub> _hubContext;

    public SignalRImportHubPublisher(IHubContext<ImportHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishImportEventAsync(ImportEventNotification notification) =>
        _hubContext.Clients.All.SendAsync("importEvent", notification);
}
