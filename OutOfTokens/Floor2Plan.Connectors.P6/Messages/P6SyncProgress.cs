using Floor2Plan.Connectors.P6.Api.Models;

namespace Floor2Plan.Connectors.P6.Messages
{
    /// <summary>
    /// Progress events published by <see cref="Actors.P6Actor"/> on the actor system's EventStream while a
    /// sync is running. <see cref="Actors.P6SyncProgressActor"/> subscribes to these and writes them to the
    /// sync log as they happen. P6Actor owns that child actor and coordinates its readiness and completion.
    /// </summary>
    public sealed record P6SyncStarted(int ProjectCount);

    public sealed record P6ProjectStarted(int ProjectObjectId, int RemainingAfterThis);

    public sealed record P6ProjectCatalogFetched(int ProjectObjectId, P6EntityKind Kind, int Count);

    public sealed record P6ProjectCatalogFetchFailed(int ProjectObjectId, P6EntityKind Kind, string Error);

    public sealed record P6SyncCompleted(P6SyncResult Result);
}
