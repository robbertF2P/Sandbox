using Floor2Plan.Connectors.P6.Api.Models;

namespace Floor2Plan.Connectors.P6.Messages
{
    /// <summary>
    /// Progress events sent by <see cref="Actors.P6SyncOrchestratorActor"/> to <see cref="Actors.P6SyncProgressActor"/>.
    /// The progress actor writes each event to <see cref="Infrastructure.Process.Contracts.Scope.IProcessLogger"/>
    /// via a fresh DI scope per message (safe when the HTTP request scope has already ended).
    /// </summary>
    public sealed record P6SyncStarted(int ProjectCount);

    public sealed record P6ProjectStarted(int ProjectObjectId, int RemainingAfterThis);

    public sealed record P6ProjectCatalogFetched(int ProjectObjectId, P6EntityKind Kind, int Count);

    public sealed record P6ProjectCatalogFetchFailed(int ProjectObjectId, P6EntityKind Kind, string Error);

    public sealed record P6SyncCompleted(P6SyncResult Result);
}
