using AkkaSignalRVuePoc.Contracts.Messages;

namespace AkkaSignalRVuePoc.Core.Publishing;

public interface ISignalrHubWrapper
{
    Task PublishActorMessageAsync(PushMessage message);
}
