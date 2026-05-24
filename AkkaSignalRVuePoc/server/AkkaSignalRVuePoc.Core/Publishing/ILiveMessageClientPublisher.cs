using AkkaSignalRVuePoc.Contracts.Messages;

namespace AkkaSignalRVuePoc.Core.Publishing;

public interface ILiveMessageClientPublisher
{
    Task PublishActorMessageAsync(PushMessage message);
}
