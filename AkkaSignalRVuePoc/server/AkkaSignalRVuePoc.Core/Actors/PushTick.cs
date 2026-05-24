namespace AkkaSignalRVuePoc.Core.Actors;

internal sealed class PushTick
{
    public static readonly PushTick Instance = new();

    private PushTick()
    {
    }
}
