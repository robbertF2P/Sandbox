using Akka.Actor;
using Floor2Plan.Connectors.P6.Api;

namespace Floor2Plan.Connectors.P6.Actors;

internal abstract record SessionWork;

internal sealed record FetchProjectCatalogWork : SessionWork;

internal sealed record BuildRawDataWork : SessionWork;

internal sealed record RunSyncWork(
    IReadOnlyList<string> ProjectIds,
    IActorRef Store,
    P6SyncOptions SyncOptions) : SessionWork;

internal sealed record RunWithSession(IActorRef ReplyTo, SessionWork Work);

internal sealed record SessionReceived(string SessionId);

internal sealed record SessionFailed(string Reason);
