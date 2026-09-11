# Primavera EPPM — Akka.NET two-way sync plan

**Scope:** Pilot with 1–2 projects. Start inbound (structure), then limited outbound (dates/progress), then reconciliation.

**Stack:** Akka.NET, `Microsoft.Extensions.DependencyInjection`, EF Core (or equivalent persist layer).

**EPPM deployment:** On-premises P6 EPPM — session cookie auth via `POST /restapi/login`, `Limit`/`Offset` paging. **Superseded for MES integration** by `docs/mes-primavera-eppm-sync-architecture.md`.

---

## 1. Actor topology

```text
SyncSupervisor
├── SyncSessionRegistryActor          # session + cursor registry (in-memory + DB hydrate)
├── EppmGatewayActor                  # sole EPPM HTTP boundary
├── PersistActor                      # sole DB writer for sync batches
├── ConflictQueueActor                # unresolved conflicts
├── ReconciliationActor               # drift detection for pilot projects
└── ProjectSyncRouterActor            # one child router per tenant (or global)
    ├── ProjectInboundSyncActor       # child per active inbound session
    └── ProjectOutboundSyncActor      # child per active outbound session
```

| Actor | Lifetime | Notes |
|-------|----------|-------|
| `SyncSupervisor` | Singleton | Supervision strategy: restart child sessions, not whole tree |
| `EppmGatewayActor` | Singleton | Rate limit + auth session renewal |
| `PersistActor` | Singleton | `IServiceScopeFactory` per message |
| `ProjectSyncRouterActor` | Singleton per tenant | Enforces pilot scope + session lock |
| `ProjectInboundSyncActor` | Per inbound run | Stopped when session completes/fails |
| `ProjectOutboundSyncActor` | Per outbound run | Drains outbox for one project |

---

## 2. Contracts (`PrimaveraSync.Contracts`)

### 2.1 Enums and policy

```csharp
namespace PrimaveraSync.Contracts;

public enum SyncMode { Full, Incremental }

public enum SyncDirection { Inbound, Outbound }

public enum SyncSessionStatus
{
    Pending,
    Fetching,
    Mapping,
    Persisting,
    Reconciling,
    Completed,
    Failed,
    Cancelled
}

public enum EntityKind
{
    Project,
    Wbs,
    Activity,
    Assignment,
    Relation
}

public enum ConflictRule
{
    RemoteWins,
    LocalWins,
    NewestWins,
    Manual
}

public sealed record EntitySyncPolicy(
    EntityKind Kind,
    bool InboundEnabled,
    bool OutboundEnabled,
    ConflictRule ConflictRule,
    IReadOnlySet<string> LocalOwnedFields,
    IReadOnlySet<string> RemoteOwnedFields);

public sealed record PilotScope(
    string TenantId,
    IReadOnlySet<string> ProjectExternalIds,
    IReadOnlySet<EntityKind> EnabledEntities,
    bool OutboundEnabled);
```

### 2.2 Correlation base

```csharp
public interface ISyncMessage
{
    Guid SyncId { get; }
    string TenantId { get; }
    string CorrelationId { get; }
}

public abstract record SyncMessageBase(
    Guid SyncId,
    string TenantId,
    string CorrelationId) : ISyncMessage;
```

### 2.3 Commands (API / scheduler → actors)

```csharp
public sealed record StartInboundSync(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    SyncMode Mode,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record StartOutboundSync(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    bool DrainOutboxOnly,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record ReconcileProject(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record CancelSync(
    Guid SyncId,
    string TenantId,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record ResolveConflict(
    Guid SyncId,
    string TenantId,
    Guid ConflictId,
    ConflictResolutionChoice Choice,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public enum ConflictResolutionChoice { ApplyRemote, ApplyLocal, Skip }
```

### 2.4 Canonical change model

```csharp
public sealed record EntityUpsert(
    EntityKind Kind,
    string ExternalId,
    string? ParentExternalId,
    DateTimeOffset? RemoteModifiedAt,
    IReadOnlyDictionary<string, object?> Fields);

public sealed record EntityDelete(
    EntityKind Kind,
    string ExternalId,
    DateTimeOffset? RemoteModifiedAt);

public sealed record RelationChange(
    string SourceExternalId,
    string TargetExternalId,
    string RelationType,
    int? LagDays);

public sealed record SyncChangeSet(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    SyncDirection Direction,
    IReadOnlyList<EntityUpsert> Upserts,
    IReadOnlyList<EntityDelete> Deletes,
    IReadOnlyList<RelationChange> Relations);
```

### 2.5 Persist boundary

```csharp
public sealed record ApplyChangeSet(
    SyncChangeSet ChangeSet,
    int BatchNumber,
    string CorrelationId);

public sealed record ApplyResult(
    Guid SyncId,
    int BatchNumber,
    int Upserted,
    int Deleted,
    int Skipped,
    int Conflicts,
    DateTimeOffset? MaxRemoteModifiedAt);

public sealed record ChangeSetApplied(
    Guid SyncId,
    ApplyResult Result,
    string CorrelationId);
```

### 2.6 EPPM gateway

```csharp
public sealed record EppmFetchPage(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    EntityKind Kind,
    int PageNumber,
    DateTimeOffset? ModifiedSince,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record EppmPageFetched(
    Guid SyncId,
    EntityKind Kind,
    int PageNumber,
    bool HasMore,
    IReadOnlyList<EppmRecordDto> Records,
    string CorrelationId);

public sealed record EppmWrite(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    EntityKind Kind,
    string ExternalId,
    EppmPayload Payload,
    string IdempotencyKey,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record EppmWriteCompleted(
    Guid SyncId,
    EntityKind Kind,
    string ExternalId,
    bool Success,
    string? Error,
    string CorrelationId);

public sealed record EppmRecordDto(
    string ExternalId,
    string? ParentExternalId,
    DateTimeOffset? ModifiedAt,
    IReadOnlyDictionary<string, object?> Fields);

public sealed record EppmPayload(
    IReadOnlyDictionary<string, object?> Fields);
```

### 2.7 Session registry

```csharp
public sealed record RegisterSyncSession(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    SyncDirection Direction,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record SyncSessionRegistered(
    Guid SyncId,
    bool Accepted,
    string? RejectReason,
    string CorrelationId);

public sealed record AdvanceCursor(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    EntityKind Kind,
    SyncDirection Direction,
    DateTimeOffset? LastModifiedAt,
    int LastPage,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record SyncProgressUpdated(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    SyncSessionStatus Status,
    EntityKind? CurrentEntity,
    int BatchNumber,
    int RecordsProcessed,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record SyncCompleted(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    SyncDirection Direction,
    ApplyResult Summary,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);

public sealed record SyncFailed(
    Guid SyncId,
    string TenantId,
    string ProjectExternalId,
    string Error,
    bool Retryable,
    string CorrelationId)
    : SyncMessageBase(SyncId, TenantId, CorrelationId);
```

### 2.8 Outbound / domain events

```csharp
public sealed record LocalEntityChanged(
    string TenantId,
    string ProjectExternalId,
    EntityKind Kind,
    string LocalId,
    string? ExternalId,
    long ChangeVersion,
    IReadOnlyDictionary<string, object?> ChangedFields,
    DateTimeOffset ChangedAt,
    string CorrelationId);
```

---

## 3. Pilot policy (config)

```csharp
public static class PilotPolicies
{
    public static PilotScope Default(string tenantId) => new(
        TenantId: tenantId,
        ProjectExternalIds: new HashSet<string> { "PILOT-PROJECT-1" },
        EnabledEntities: new HashSet<EntityKind>
        {
            EntityKind.Wbs,
            EntityKind.Activity,
            EntityKind.Relation
        },
        OutboundEnabled: true);

    public static IReadOnlyList<EntitySyncPolicy> EntityPolicies => new[]
    {
        new EntitySyncPolicy(
            EntityKind.Wbs,
            InboundEnabled: true,
            OutboundEnabled: false,
            ConflictRule: ConflictRule.RemoteWins,
            LocalOwnedFields: new HashSet<string>(),
            RemoteOwnedFields: new HashSet<string> { "*" }),

        new EntitySyncPolicy(
            EntityKind.Activity,
            InboundEnabled: true,
            OutboundEnabled: true,
            ConflictRule: ConflictRule.Manual,
            LocalOwnedFields: new HashSet<string>
            {
                "PercentComplete",
                "PlannedStart",
                "PlannedFinish",
                "ActualStart",
                "ActualFinish"
            },
            RemoteOwnedFields: new HashSet<string>
            {
                "Name",
                "WbsExternalId",
                "ActivityCode"
            }),

        new EntitySyncPolicy(
            EntityKind.Relation,
            InboundEnabled: true,
            OutboundEnabled: false,
            ConflictRule: ConflictRule.RemoteWins,
            LocalOwnedFields: new HashSet<string>(),
            RemoteOwnedFields: new HashSet<string> { "*" })
    };
}
```

---

## 4. `ProjectSyncRouterActor`

Ensures pilot scope, prevents duplicate concurrent sessions per `(tenant, project, direction)`.

```csharp
using Akka.Actor;
using Akka.Routing;
using PrimaveraSync.Contracts;

namespace PrimaveraSync.Core.Actors;

public sealed class ProjectSyncRouterActor : ReceiveActor
{
    private readonly PilotScope _pilot;
    private readonly Dictionary<(string ProjectId, SyncDirection Dir), Guid> _active = new();

    public ProjectSyncRouterActor(PilotScope pilot)
    {
        _pilot = pilot;

        Receive<StartInboundSync>(StartInbound);
        Receive<StartOutboundSync>(StartOutbound);
        Receive<SyncCompleted>(OnSessionEnded);
        Receive<SyncFailed>(OnSessionEnded);
    }

    private void StartInbound(StartInboundSync cmd)
    {
        if (!TryAccept(cmd.ProjectExternalId, SyncDirection.Inbound, out var reason))
        {
            Sender.Tell(new SyncSessionRegistered(cmd.SyncId, false, reason, cmd.CorrelationId));
            return;
        }

        var child = Context.ActorOf(
            Props.Create(() => new ProjectInboundSyncActor(_pilot))
                .WithRouter(new RoundRobinPool(1)),
            $"inbound-{cmd.ProjectExternalId}-{cmd.SyncId:N}");

        child.Forward(cmd);
    }

    private void StartOutbound(StartOutboundSync cmd)
    {
        if (!_pilot.OutboundEnabled)
        {
            Sender.Tell(new SyncSessionRegistered(cmd.SyncId, false, "Outbound disabled for pilot.", cmd.CorrelationId));
            return;
        }

        if (!TryAccept(cmd.ProjectExternalId, SyncDirection.Outbound, out var reason))
        {
            Sender.Tell(new SyncSessionRegistered(cmd.SyncId, false, reason, cmd.CorrelationId));
            return;
        }

        var child = Context.ActorOf(
            Props.Create(() => new ProjectOutboundSyncActor(_pilot)),
            $"outbound-{cmd.ProjectExternalId}-{cmd.SyncId:N}");

        child.Forward(cmd);
    }

    private bool TryAccept(string projectId, SyncDirection dir, out string? reason)
    {
        if (!_pilot.ProjectExternalIds.Contains(projectId))
        {
            reason = $"Project '{projectId}' is outside pilot scope.";
            return false;
        }

        if (_active.ContainsKey((projectId, dir)))
        {
            reason = $"Active {dir} sync already running for project '{projectId}'.";
            return false;
        }

        reason = null;
        return true;
    }

    private void OnSessionEnded(ISyncMessage msg)
    {
        var projectId = msg switch
        {
            SyncCompleted c => c.ProjectExternalId,
            SyncFailed f => f.ProjectExternalId,
            _ => null
        };
        var dir = msg switch
        {
            SyncCompleted c => c.Direction,
            SyncFailed f => SyncDirection.Inbound, // both keys cleared below by SyncId lookup if needed
            _ => SyncDirection.Inbound
        };

        if (projectId is null) return;

        _active.Remove((projectId, SyncDirection.Inbound));
        _active.Remove((projectId, SyncDirection.Outbound));
    }
}
```

---

## 5. `ProjectInboundSyncActor` — state machine

Fetch order: `Wbs` → `Activity` → `Relation`. Uses `PipeTo` for EPPM calls and `PersistActor` for DB writes.

```csharp
using Akka.Actor;
using Akka.Event;
using PrimaveraSync.Contracts;

namespace PrimaveraSync.Core.Actors;

public sealed class ProjectInboundSyncActor : ReceiveActor
{
    private static readonly EntityKind[] FetchOrder =
    {
        EntityKind.Wbs,
        EntityKind.Activity,
        EntityKind.Relation
    };

    private readonly PilotScope _pilot;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    private IActorRef _eppm = ActorRefs.Nobody;
    private IActorRef _persist = ActorRefs.Nobody;
    private IActorRef _registry = ActorRefs.Nobody;
    private IActorRef _replyTo = ActorRefs.Nobody;

    // Session state
    private StartInboundSync _command = null!;
    private EntityKind _currentKind;
    private int _kindIndex;
    private int _page;
    private int _batch;
    private int _recordsProcessed;
    private DateTimeOffset? _maxModified;
    private DateTimeOffset? _cursor;

    public ProjectInboundSyncActor(PilotScope pilot)
    {
        _pilot = pilot;
        Receive<StartInboundSync>(Start);
        Become(Idle);
    }

    private void Idle()
    {
        Receive<StartInboundSync>(Start);
    }

    private void Start(StartInboundSync cmd)
    {
        _command = cmd;
        _replyTo = Sender;
        _eppm = Context.ActorSelection("/user/sync/eppm").ResolveOne(TimeSpan.FromSeconds(3)).Result;
        _persist = Context.ActorSelection("/user/sync/persist").ResolveOne(TimeSpan.FromSeconds(3)).Result;
        _registry = Context.ActorSelection("/user/sync/registry").ResolveOne(TimeSpan.FromSeconds(3)).Result;

        _kindIndex = 0;
        _page = 1;
        _batch = 0;
        _recordsProcessed = 0;
        _maxModified = null;

        _registry.Tell(new RegisterSyncSession(
            cmd.SyncId, cmd.TenantId, cmd.ProjectExternalId,
            SyncDirection.Inbound, cmd.CorrelationId));

        _log.Info("Inbound sync started {SyncId} project={Project} mode={Mode}",
            cmd.SyncId, cmd.ProjectExternalId, cmd.Mode);

        PublishProgress(SyncSessionStatus.Fetching);
        BeginCurrentKind();
        Become(Fetching);
    }

    private void Fetching()
    {
        Receive<EppmPageFetched>(OnPageFetched);
        Receive<ChangeSetApplied>(OnChangeSetApplied);
        Receive<Status.Failure>(f => Fail($"EPPM fetch failed: {f.Cause.Message}", retryable: true));
    }

    private void Persisting()
    {
        Receive<ChangeSetApplied>(OnChangeSetApplied);
        Receive<Status.Failure>(f => Fail($"Persist failed: {f.Cause.Message}", retryable: false));
    }

    private void BeginCurrentKind()
    {
        _currentKind = FetchOrder[_kindIndex];
        if (!_pilot.EnabledEntities.Contains(_currentKind))
        {
            NextKindOrComplete();
            return;
        }

        _page = 1;
        _cursor = _command.Mode == SyncMode.Incremental
            ? LoadCursor(_currentKind) // hydrate from registry/DB before start in production
            : null;

        RequestPage();
    }

    private void RequestPage()
    {
        var fetch = new EppmFetchPage(
            _command.SyncId,
            _command.TenantId,
            _command.ProjectExternalId,
            _currentKind,
            _page,
            _cursor,
            _command.CorrelationId);

        _eppm.Tell(fetch, Self);
    }

    private void OnPageFetched(EppmPageFetched page)
    {
        if (page.Records.Count == 0)
        {
            AdvanceCursorForKind();
            NextKindOrComplete();
            return;
        }

        var changeSet = MapToChangeSet(page.Records);
        _batch++;

        PublishProgress(SyncSessionStatus.Persisting);
        Become(Persisting);

        _persist.Tell(new ApplyChangeSet(changeSet, _batch, _command.CorrelationId), Self);
    }

    private void OnChangeSetApplied(ChangeSetApplied applied)
    {
        _recordsProcessed += applied.Result.Upserted + applied.Result.Deleted;

        if (applied.Result.MaxRemoteModifiedAt is { } max)
            _maxModified = _maxModified is null ? max : (_maxModified > max ? _maxModified : max);

        // Assumption: gateway sets HasMore on EppmPageFetched; track via session field in full impl
        _page++;
        PublishProgress(SyncSessionStatus.Fetching);
        Become(Fetching);
        RequestPage();
    }

    private SyncChangeSet MapToChangeSet(IReadOnlyList<EppmRecordDto> records)
    {
        var upserts = records.Select(r => new EntityUpsert(
            _currentKind,
            r.ExternalId,
            r.ParentExternalId,
            r.ModifiedAt,
            r.Fields)).ToList();

        return new SyncChangeSet(
            _command.SyncId,
            _command.TenantId,
            _command.ProjectExternalId,
            SyncDirection.Inbound,
            upserts,
            Array.Empty<EntityDelete>(),
            Array.Empty<RelationChange>());
    }

    private void AdvanceCursorForKind()
    {
        _registry.Tell(new AdvanceCursor(
            _command.SyncId,
            _command.TenantId,
            _command.ProjectExternalId,
            _currentKind,
            SyncDirection.Inbound,
            _maxModified,
            _page,
            _command.CorrelationId));
    }

    private void NextKindOrComplete()
    {
        _kindIndex++;
        if (_kindIndex >= FetchOrder.Length)
        {
            Complete();
            return;
        }

        _maxModified = null;
        BeginCurrentKind();
    }

    private void Complete()
    {
        var summary = new ApplyResult(_command.SyncId, _batch, _recordsProcessed, 0, 0, 0, _maxModified);
        var evt = new SyncCompleted(
            _command.SyncId,
            _command.TenantId,
            _command.ProjectExternalId,
            SyncDirection.Inbound,
            summary,
            _command.CorrelationId);

        _registry.Tell(evt);
        _replyTo.Tell(evt);
        Context.Stop(Self);
    }

    private void Fail(string error, bool retryable)
    {
        var evt = new SyncFailed(
            _command.SyncId,
            _command.TenantId,
            _command.ProjectExternalId,
            error,
            retryable,
            _command.CorrelationId);

        _registry.Tell(evt);
        _replyTo.Tell(evt);
        Context.Stop(Self);
    }

    private void PublishProgress(SyncSessionStatus status)
    {
        _registry.Tell(new SyncProgressUpdated(
            _command.SyncId,
            _command.TenantId,
            _command.ProjectExternalId,
            status,
            _currentKind,
            _batch,
            _recordsProcessed,
            _command.CorrelationId));
    }

    private DateTimeOffset? LoadCursor(EntityKind kind) => null; // load from ISyncCursorStore in production
}
```

### Production hardening for inbound actor

| Gap in sample | Add |
|---------------|-----|
| `HasMore` paging | Keep `_hasMore` from `EppmPageFetched`; only increment page when `HasMore` |
| `Ask` for actor refs | Inject `IActorRef` for eppm/persist/registry via ctor |
| Retry | `IWithTimers` + `ScheduleTellOnce` on transient `SyncFailed` |
| Relations pass | Map `RelationChange` when `_currentKind == Relation` |
| Deletes | Compare snapshots in `ReconciliationActor` or EPPM tombstone support |

---

## 6. `EppmGatewayActor` (sketch)

```csharp
public sealed class EppmGatewayActor : ReceiveActor
{
    private readonly IEppmClient _client;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public EppmGatewayActor(IEppmClient client)
    {
        _client = client;

        ReceiveAsync<EppmFetchPage>(async msg =>
        {
            var page = await _client.FetchPageAsync(
                msg.TenantId,
                msg.ProjectExternalId,
                msg.Kind,
                msg.PageNumber,
                msg.ModifiedSince,
                Context.Parent);

            Sender.Tell(new EppmPageFetched(
                msg.SyncId,
                msg.Kind,
                msg.PageNumber,
                page.HasMore,
                page.Records,
                msg.CorrelationId));
        });

        ReceiveAsync<EppmWrite>(async msg =>
        {
            try
            {
                await _client.WriteAsync(msg.TenantId, msg.Kind, msg.ExternalId, msg.Payload);
                Sender.Tell(new EppmWriteCompleted(
                    msg.SyncId, msg.Kind, msg.ExternalId, true, null, msg.CorrelationId));
            }
            catch (Exception ex)
            {
                _log.Warning(ex, "EPPM write failed {Kind} {ExternalId}", msg.Kind, msg.ExternalId);
                Sender.Tell(new EppmWriteCompleted(
                    msg.SyncId, msg.Kind, msg.ExternalId, false, ex.Message, msg.CorrelationId));
            }
        });
    }
}
```

---

## 7. `PersistActor` (sketch)

```csharp
public sealed class PersistActor : ReceiveActor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public PersistActor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        ReceiveAsync<ApplyChangeSet>(async msg =>
        {
            using var scope = _scopeFactory.CreateScope();
            var applier = scope.ServiceProvider.GetRequiredService<IChangeSetApplier>();

            var result = await applier.ApplyAsync(msg.ChangeSet);
            Sender.Tell(new ChangeSetApplied(msg.ChangeSet.SyncId, result, msg.CorrelationId));
        });
    }
}
```

`IChangeSetApplier` responsibilities:

- Resolve external IDs
- Apply `EntitySyncPolicy` (field ownership, conflict detection)
- Batch EF `SaveChanges`
- Return `MaxRemoteModifiedAt` for cursor advancement

---

## 8. `ProjectOutboundSyncActor` (sketch)

```csharp
public sealed class ProjectOutboundSyncActor : ReceiveActor, IWithTimers
{
    private readonly PilotScope _pilot;
    private StartOutboundSync _command = null!;
    private Queue<OutboundItem> _queue = new();
    public ITimerScheduler Timers { get; set; } = null!;

    public ProjectOutboundSyncActor(PilotScope pilot) => _pilot = pilot;

    protected override void PreStart() => Receive<StartOutboundSync>(Start);

    private void Start(StartOutboundSync cmd)
    {
        _command = cmd;
        // Load pending outbox rows for project where DeliveredAt is null
        Become(Draining);
        Self.Tell(new DrainNext());
    }

    private void Draining()
    {
        Receive<DrainNext>(_ => WriteNext());
        Receive<EppmWriteCompleted>(OnWriteCompleted);
    }

    private void WriteNext() { /* map outbox row -> EppmWrite, forward to gateway */ }
    private void OnWriteCompleted(EppmWriteCompleted result) { /* mark delivered or conflict */ }
    private sealed record DrainNext();
}
```

Outbound trigger:

```text
Domain service commits
  -> publish LocalEntityChanged to EventStream
  -> OutboxProjectorActor writes OutboundChangeOutbox row
  -> scheduler or manual StartOutboundSync drains queue
```

---

## 9. Host registration

```csharp
services.AddSingleton<IEppmClient, EppmClient>();
services.AddScoped<IChangeSetApplier, EfChangeSetApplier>();
services.AddSingleton(PilotPolicies.Default(tenantId));

services.AddAkka("primavera-sync", (builder, sp) =>
{
    builder.WithActors((system, registry) =>
    {
        var pilot = sp.GetRequiredService<PilotScope>();

        var supervisor = system.ActorOf(
            Props.Create(() => new SyncSupervisor(pilot)), "sync");

        // HTTP facade resolves supervisor and AskCorrelated at boundary only
    });
});
```

---

## 10. Delivery checklist

### Phase 1 — Inbound pilot (project 1)

- [ ] Contracts assembly
- [ ] `EppmGatewayActor` + `IEppmClient`
- [ ] `PersistActor` + `IChangeSetApplier` + external ID table
- [ ] `SyncSessionRegistryActor` + cursor store
- [ ] `ProjectInboundSyncActor` full/incremental
- [ ] API: `POST /sync/inbound` with `StartInboundSync`
- [ ] Metrics: records/sec, batch duration, failures

### Phase 2 — Limited outbound

- [ ] `OutboundChangeOutbox` + `LocalEntityChanged` projector
- [ ] `ProjectOutboundSyncActor`
- [ ] Field-level policy enforcement for Activity dates/progress
- [ ] `ConflictQueueActor` + resolve API

### Phase 3 — Second project + reconcile

- [ ] Add project 2 to `PilotScope`
- [ ] `ReconciliationActor` nightly job
- [ ] Dashboard: last sync, conflicts, replay

---

## 11. HTTP boundary (Ask only here)

```csharp
[ApiController]
[Route("api/sync")]
public sealed class SyncController : ControllerBase
{
    private readonly IActorRef _router;

    [HttpPost("inbound")]
    public async Task<ActionResult<SyncCompleted>> StartInbound(
        [FromBody] StartInboundRequest request,
        CancellationToken ct)
    {
        var syncId = Guid.NewGuid();
        var correlationId = HttpContext.TraceIdentifier;

        var cmd = new StartInboundSync(
            syncId,
            request.TenantId,
            request.ProjectExternalId,
            request.Mode,
            correlationId);

        var result = await _router.Ask<SyncCompleted>(cmd, ct);
        return Ok(result);
    }
}
```

---

*Draft — September 2026. For external projects; adapt namespaces and persistence to your host.*
