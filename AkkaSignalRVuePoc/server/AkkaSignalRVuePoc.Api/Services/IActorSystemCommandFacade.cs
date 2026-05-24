namespace AkkaSignalRVuePoc.Api.Services;

public interface IActorSystemCommandFacade
{
    void SendLiveMessage(string text);

    void StartBackgroundProcess();
}
