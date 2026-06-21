using Akka.Actor;
using Akka.Event;
using F2pPlatform.Host.Contracts.Events;
using F2pPlatform.Host.Contracts.Messages;

namespace F2pPlatform.Host.Core.Actors;

public sealed class PlatformRootActor : ReceiveActor
{
    private readonly IActorRef _signalRHubActor;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public PlatformRootActor(IActorRef signalRHubActor)
    {
        _signalRHubActor = signalRHubActor;
        Receive<PublishPlatformStarted>(HandlePlatformStarted);
    }

    public static Props Props(IActorRef signalRHubActor) =>
        Akka.Actor.Props.Create(() => new PlatformRootActor(signalRHubActor));

    private void HandlePlatformStarted(PublishPlatformStarted message)
    {
        var platformStarted = new PlatformStarted(message.OccurredAt);
        Context.System.EventStream.Publish(platformStarted);
        _log.Info("Platform started at {0}", message.OccurredAt);
    }
}
