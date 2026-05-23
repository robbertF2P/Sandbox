using Akka.Actor;
using Akka.TestKit.Xunit;
using AkkaSignalRVuePoc.Api.Actors;
using AkkaSignalRVuePoc.Api.Models;
using AkkaSignalRVuePoc.Api.Tests.TestDoubles;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.XUnit3;

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
            FrontendPushActor.Props(hubPushActor, pushInterval: TimeSpan.FromMinutes(10)),
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
            FrontendPushActor.Props(
                hubPushActor,
                pushInterval: TimeSpan.FromMilliseconds(50),
                publishImmediately: false),
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
            SignalRHubPushActor.Props(
                hubContext,
                loggerFactory.CreateLogger<SignalRHubPushActor>()),
            "signalr-hub-push");
    }

    private SerilogLoggerFactory CreateSerilogLoggerFactory()
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.XUnit3TestOutput()
            .CreateLogger();

        return new SerilogLoggerFactory(logger, dispose: true);
    }

    private static PushMessage GetMessage(RecordedHubCall call)
    {
        return Assert.IsType<PushMessage>(Assert.Single(call.Arguments));
    }
}
