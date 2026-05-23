using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Tests.TestDoubles;

internal sealed class NoopGroupManager : IGroupManager
{
    public Task AddToGroupAsync(
        string connectionId,
        string groupName,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RemoveFromGroupAsync(
        string connectionId,
        string groupName,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
