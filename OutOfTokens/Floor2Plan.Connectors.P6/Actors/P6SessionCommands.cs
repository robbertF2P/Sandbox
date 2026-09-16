using Akka.Actor;
using Floor2Plan.Connectors.P6.Api;

namespace Floor2Plan.Connectors.P6.Actors;

/// <summary>Fetch the project catalog for connector configuration.</summary>
internal sealed record FetchProjectCatalog(IActorRef ReplyTo);

/// <summary>Build the downloadable raw-data snapshot.</summary>
internal sealed record BuildRawData(IActorRef ReplyTo);

/// <summary>Run a full catalog synchronization for the selected project ObjectIds.</summary>
internal sealed record RunSync(
    IActorRef ReplyTo,
    IReadOnlyList<string> ProjectIds,
    IActorRef Store,
    P6SyncOptions SyncOptions);
