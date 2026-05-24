using Akka.Actor;
using Akka.TestKit.Xunit;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.Actors;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

public sealed class FrontendPushActorTests : TestKit
{
    public FrontendPushActorTests(ITestOutputHelper output)
        : base(DefaultConfig, nameof(FrontendPushActorTests), output)
    {
    }

    [Fact]
    public void Publishes_actor_message_when_started()
    {
        var hubPushActor = CreateTestProbe("hub-push");

        var actor = Sys.ActorOf(
            FrontendPushActor.Props(hubPushActor.Ref, pushInterval: TimeSpan.FromMinutes(10)),
            "initial-push-actor");

        var publish = hubPushActor.ExpectMsg<PublishActorMessage>(TimeSpan.FromSeconds(3));

        var message = publish.Message;
        Assert.Equal(1, message.Sequence);
        Assert.Equal("Akka.NET actor heartbeat #1", message.Text);
        Assert.Contains("initial-push-actor", message.Source);

        actor.GracefulStop(TimeSpan.FromSeconds(3)).Wait();
    }

    [Fact]
    public void Publishes_actor_messages_on_configured_interval()
    {
        var hubPushActor = CreateTestProbe("hub-push");

        var actor = Sys.ActorOf(
            FrontendPushActor.Props(
                hubPushActor.Ref,
                pushInterval: TimeSpan.FromMilliseconds(50),
                publishImmediately: false),
            "periodic-push-actor");

        var firstMessage = hubPushActor.ExpectMsg<PublishActorMessage>(TimeSpan.FromSeconds(3)).Message;
        var secondMessage = hubPushActor.ExpectMsg<PublishActorMessage>(TimeSpan.FromSeconds(3)).Message;

        Assert.Equal(1, firstMessage.Sequence);
        Assert.Equal(2, secondMessage.Sequence);

        actor.GracefulStop(TimeSpan.FromSeconds(3)).Wait();
    }
}
