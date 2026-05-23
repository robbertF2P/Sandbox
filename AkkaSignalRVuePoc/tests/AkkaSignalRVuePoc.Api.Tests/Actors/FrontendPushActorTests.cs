using System.Collections.Concurrent;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using AkkaSignalRVuePoc.Api.Actors;
using AkkaSignalRVuePoc.Api.Hubs;
using AkkaSignalRVuePoc.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

public sealed class FrontendPushActorTests : TestKit
{
    private readonly ITestOutputHelper _output;

    public FrontendPushActorTests(ITestOutputHelper output)
        : base(DefaultConfig, nameof(FrontendPushActorTests), output)
    {
        _output = output;
    }

    [Fact]
    public async Task Publishes_actor_message_when_started()
    {
        using var loggerFactory = CreateSerilogLoggerFactory();
        var hubContext = new RecordingHubContext();
        var hubPushActor = CreateHubPushActor(hubContext, loggerFactory);

        var actor = Sys.ActorOf(
            Props.Create(() => new FrontendPushActor(
                hubPushActor,
                pushInterval: TimeSpan.FromMinutes(10))),
            "initial-push-actor");

        var call = await hubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(3));

        Assert.Equal("actorMessage", call.Method);
        var message = Assert.IsType<PushMessage>(Assert.Single(call.Arguments));
        Assert.Equal(1, message.Sequence);
        Assert.Equal("Akka.NET actor heartbeat #1", message.Text);
        Assert.Contains("initial-push-actor", message.Source);

        await actor.GracefulStop(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Publishes_actor_messages_on_configured_interval()
    {
        using var loggerFactory = CreateSerilogLoggerFactory();
        var hubContext = new RecordingHubContext();
        var hubPushActor = CreateHubPushActor(hubContext, loggerFactory);

        var actor = Sys.ActorOf(
            Props.Create(() => new FrontendPushActor(
                hubPushActor,
                pushInterval: TimeSpan.FromMilliseconds(50),
                publishImmediately: false)),
            "periodic-push-actor");

        var firstCall = await hubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(3));
        var secondCall = await hubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(3));

        Assert.Equal(1, GetMessage(firstCall).Sequence);
        Assert.Equal(2, GetMessage(secondCall).Sequence);
        Assert.All(new[] { firstCall, secondCall }, call => Assert.Equal("actorMessage", call.Method));

        await actor.GracefulStop(TimeSpan.FromSeconds(3));
    }

    private IActorRef CreateHubPushActor(
        RecordingHubContext hubContext,
        SerilogLoggerFactory loggerFactory)
    {
        return Sys.ActorOf(
            Props.Create(() => new SignalRHubPushActor(
                hubContext,
                loggerFactory.CreateLogger<SignalRHubPushActor>())),
            "signalr-hub-push");
    }

    private SerilogLoggerFactory CreateSerilogLoggerFactory()
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.TestOutput(
                _output,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        return new SerilogLoggerFactory(logger, dispose: true);
    }

    private static PushMessage GetMessage(RecordedHubCall call)
    {
        return Assert.IsType<PushMessage>(Assert.Single(call.Arguments));
    }

    private sealed class RecordingHubContext : IHubContext<LiveMessagesHub>
    {
        public RecordingHubContext()
        {
            ClientProxy = new RecordingClientProxy();
            Clients = new RecordingHubClients(ClientProxy);
            Groups = new NoopGroupManager();
        }

        public RecordingClientProxy ClientProxy { get; }

        public IHubClients Clients { get; }

        public IGroupManager Groups { get; }
    }

    private sealed class RecordingHubClients : IHubClients
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

    private sealed class NoopGroupManager : IGroupManager
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

    private sealed class RecordingClientProxy : IClientProxy
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

    private sealed record RecordedHubCall(string Method, object?[] Arguments);
}
