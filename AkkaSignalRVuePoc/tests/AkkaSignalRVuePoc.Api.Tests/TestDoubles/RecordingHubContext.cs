using AkkaSignalRVuePoc.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Tests.TestDoubles;

public sealed class RecordingHubContext : IHubContext<LiveMessagesHub>
{
    public RecordingHubContext()
    {
        ClientProxy = new RecordingClientProxy();
        Clients = new RecordingHubClients(ClientProxy);
        Groups = new NoopGroupManager();
    }

    public RecordingClientProxy ClientProxy
    {
        get;
    }

    public IHubClients Clients
    {
        get;
    }

    public IGroupManager Groups
    {
        get;
    }
}
