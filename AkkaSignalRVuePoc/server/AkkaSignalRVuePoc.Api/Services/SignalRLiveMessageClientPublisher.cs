using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.Publishing;
using AkkaSignalRVuePoc.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Services;

public sealed class SignalRLiveMessageClientPublisher : ISignalrHubWrapper
{
    private readonly IHubContext<LiveMessagesHub> _hubContext;

    public SignalRLiveMessageClientPublisher(IHubContext<LiveMessagesHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishActorMessageAsync(PushMessage message)
    {
        return _hubContext.Clients.All.SendAsync("actorMessage", message);
    }
}
