using System.Collections.Concurrent;

using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Tests.TestDoubles;

internal sealed class RecordingClientProxy : IClientProxy
{
    private readonly ConcurrentQueue<RecordedHubCall> _calls = new();
    private readonly SemaphoreSlim _availableCalls = new(0);

    public Task SendCoreAsync(
        string method,
        object?[] args,
        CancellationToken cancellationToken = default)
    {
        _calls.Enqueue(new RecordedHubCall(method, args));
        _availableCalls.Release();

        return Task.CompletedTask;
    }

    public async Task<RecordedHubCall> WaitForCallAsync(TimeSpan timeout)
    {
        using var timeoutCancellation = new CancellationTokenSource(timeout);
        await _availableCalls.WaitAsync(timeoutCancellation.Token);

        if (_calls.TryDequeue(out var call))
        {
            return call;
        }

        throw new InvalidOperationException("A SignalR hub call was signaled but could not be read.");
    }
}
