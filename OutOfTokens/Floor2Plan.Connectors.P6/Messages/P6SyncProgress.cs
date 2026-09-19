using Floor2Plan.Connectors.P6.Api.Models;

namespace Floor2Plan.Connectors.P6.Messages
{
    /// <summary>
    /// P6 sync progress events published on the actor system <c>EventStream</c> by
    /// <see cref="Actors.P6SyncOrchestratorActor"/>. Subscribers (e.g. <see cref="Actors.P6SyncProgressActor"/>)
    /// handle logging, SignalR push, etc. without the orchestrator knowing who is listening.
    /// </summary>
    public interface IP6SyncProgressEvent
    {
    }

    public sealed record P6SyncStarted(int ProjectCount) : IP6SyncProgressEvent;

    public sealed record P6ProjectStarted(int ProjectObjectId, int RemainingAfterThis) : IP6SyncProgressEvent;

    public sealed record P6ProjectCatalogFetched(int ProjectObjectId, P6EntityKind Kind, int Count) : IP6SyncProgressEvent;

    public sealed record P6ProjectCatalogFetchFailed(int ProjectObjectId, P6EntityKind Kind, string Error) : IP6SyncProgressEvent;

    public sealed record P6SyncCompleted(P6SyncResult Result) : IP6SyncProgressEvent;
}
