using System.Net.Http.Json;
using System.Threading.Channels;
using AkkaSignalRVuePoc.Api.Models;
using AkkaSignalRVuePoc.Contracts.Messages;
using Microsoft.AspNetCore.SignalR.Client;

namespace AkkaSignalRVuePoc.Api.Tests.Integration;

public sealed class PostLiveMessageIntegrationTests : IAsyncLifetime
{
    private readonly ApiWebApplicationFactory _factory = new();
    private HubConnection? _connection;
    private readonly Channel<PushMessage> _actorMessages = Channel.CreateUnbounded<PushMessage>();

    public async ValueTask InitializeAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(
                new Uri(_factory.Server.BaseAddress!, "hubs/live-messages"),
                options => options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler())
            .Build();

        _connection.On<PushMessage>("actorMessage", message => _actorMessages.Writer.TryWrite(message));

        await _connection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }

        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Post_message_reaches_signalr_clients_via_actor_system()
    {
        var uniqueText = $"integration-{Guid.NewGuid()}";
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/messages",
            new PostLiveMessageRequest(uniqueText),
            TestContext.Current.CancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        PushMessage? receivedMessage = null;

        await foreach (var message in _actorMessages.Reader.ReadAllAsync(timeout.Token))
        {
            if (message.Text == uniqueText)
            {
                receivedMessage = message;
                break;
            }
        }

        Assert.NotNull(receivedMessage);
        Assert.Contains("live-message-root", receivedMessage.Source, StringComparison.Ordinal);
    }
}
