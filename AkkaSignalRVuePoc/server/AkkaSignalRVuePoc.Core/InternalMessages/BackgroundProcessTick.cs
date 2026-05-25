namespace AkkaSignalRVuePoc.Core.InternalMessages;

internal sealed class BackgroundProcessTick
{
    public static readonly BackgroundProcessTick Busy = new();
    public static readonly BackgroundProcessTick Complete = new();

    private BackgroundProcessTick()
    {
    }
}
