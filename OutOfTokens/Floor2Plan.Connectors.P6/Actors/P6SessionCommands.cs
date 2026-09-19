using Akka.Actor;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Sync;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>Fetch the project catalog for connector configuration.</summary>
    internal sealed record FetchProjectCatalog(IActorRef ReplyTo);

    /// <summary>Build the downloadable raw-data snapshot.</summary>
    internal sealed record BuildRawData(IActorRef ReplyTo);

    /// <summary>Run catalog synchronization for the selected project ObjectIds and optional per-project plans.</summary>
    internal sealed record RunSync(
        IActorRef ReplyTo,
        IReadOnlyList<string> ProjectIds,
        IActorRef Store,
        P6SyncOptions SyncOptions,
        IReadOnlyList<P6ProjectSyncPlan> SyncPlans = null,
        IActorRef Progress = null);
}
