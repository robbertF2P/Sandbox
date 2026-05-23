using Akka.Actor;
using AkkaSignalRVuePoc.Api.Actors;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

public sealed class FrontendPushActorTests : ActorTestBase<FrontendPushActorTests>
{
    public FrontendPushActorTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public async Task Publishes_actor_message_when_started()
    {
        var hubPushActor = CreateHubPushActor();

        var actor = Sys.ActorOf(
            FrontendPushActor.Props(hubPushActor, pushInterval: TimeSpan.FromMinutes(10)),
            "initial-push-actor");

        var call = await HubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(3));

        Assert.Equal("actorMessage", call.Method);
        var message = GetPushMessage(call);
        Assert.Equal(1, message.Sequence);
        Assert.Equal("Akka.NET actor heartbeat #1", message.Text);
        Assert.Contains("initial-push-actor", message.Source);

        await actor.GracefulStop(TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task Publishes_actor_messages_on_configured_interval()
    {
        var hubPushActor = CreateHubPushActor();

        var actor = Sys.ActorOf(
            FrontendPushActor.Props(
                hubPushActor,
                pushInterval: TimeSpan.FromMilliseconds(50),
                publishImmediately: false),
            "periodic-push-actor");

        var firstCall = await HubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(3));
        var secondCall = await HubContext.ClientProxy.WaitForCallAsync(TimeSpan.FromSeconds(3));

        Assert.Equal(1, GetPushMessage(firstCall).Sequence);
        Assert.Equal(2, GetPushMessage(secondCall).Sequence);
        Assert.All(new[] { firstCall, secondCall }, call => Assert.Equal("actorMessage", call.Method));

        await actor.GracefulStop(TimeSpan.FromSeconds(3));
    }
}
