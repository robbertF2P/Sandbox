using Akka.Actor;
using AkkaSignalRVuePoc.Contracts.Messages;

namespace AkkaSignalRVuePoc.Api.Services;

public sealed class ActorSystemCommandFacade : IActorSystemCommandFacade
{
    private readonly IActorRef _rootActor;

    public ActorSystemCommandFacade(IActorRef rootActor)
    {
        _rootActor = rootActor;
    }

    public void SendLiveMessage(string text)
    {
        _rootActor.Tell(new PublishLiveMessageCommand(text));
    }

    public void StartBackgroundProcess()
    {
        _rootActor.Tell(new StartBackgroundProcessCommand());
    }
}
