namespace AkkaSignalRVuePoc.Core.Actors;

public sealed record BackgroundProcessTiming(
    TimeSpan Duration,
    TimeSpan BusySignalInterval)
{
    public static BackgroundProcessTiming Default { get; } =
        new(Duration: TimeSpan.FromMinutes(1), BusySignalInterval: TimeSpan.FromSeconds(15));
}
