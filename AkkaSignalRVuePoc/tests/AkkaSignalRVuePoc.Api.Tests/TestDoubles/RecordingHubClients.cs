using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Tests.TestDoubles;

internal sealed class RecordingHubClients : IHubClients
{
    private readonly IClientProxy _clientProxy;

    public RecordingHubClients(IClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public IClientProxy All => _clientProxy;

    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => _clientProxy;

    public IClientProxy Client(string connectionId) => _clientProxy;

    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => _clientProxy;

    public IClientProxy Group(string groupName) => _clientProxy;

    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => _clientProxy;

    public IClientProxy Groups(IReadOnlyList<string> groupNames) => _clientProxy;

    public IClientProxy User(string userId) => _clientProxy;

    public IClientProxy Users(IReadOnlyList<string> userIds) => _clientProxy;
}
