namespace AkkaSignalRVuePoc.Core.InternalMessages;

internal sealed class PushTick
{
    public static readonly PushTick Instance = new();

    private PushTick()
    {
    }
}
