# Primavera EPPM sync — SQL Server tables (per-tenant database)

Each tenant has its own database. **No `TenantId` column** — scope is implicit.

**EPPM deployment:** On-premises P6 EPPM. See `docs/mes-primavera-eppm-sync-architecture.md` §2.1 and §5.1 for HTTP auth and API details.

---

## Tables

```text
SyncSession
SyncBatch              (optional, for large runs)
SyncCursor
ExternalIdentityMap
OutboundChangeOutbox
SyncConflict
SyncSessionEvent         (optional audit)
```

---

## SQL DDL

```sql
CREATE TABLE dbo.SyncSession (
    SyncSessionId        uniqueidentifier NOT NULL PRIMARY KEY,
    ProjectExternalId    nvarchar(128)    NOT NULL,
    Direction            tinyint          NOT NULL,  -- 1=Inbound, 2=Outbound, 3=Reconcile
    Mode                 tinyint          NOT NULL,  -- 1=Full, 2=Incremental
    Status               tinyint          NOT NULL,  -- 1=Pending, 2=Running, 3=Completed, 4=Failed, 5=Cancelled
    CorrelationId        nvarchar(128)    NOT NULL,
    StartedAtUtc         datetimeoffset   NOT NULL,
    CompletedAtUtc       datetimeoffset   NULL,
    InitiatedBy          nvarchar(128)    NULL,
    RecordsFetched       int              NOT NULL DEFAULT 0,
    RecordsApplied       int              NOT NULL DEFAULT 0,
    RecordsSkipped       int              NOT NULL DEFAULT 0,
    RecordsFailed        int              NOT NULL DEFAULT 0,
    ConflictsDetected    int              NOT NULL DEFAULT 0,
    LastError            nvarchar(max)    NULL,
    Retryable            bit              NOT NULL DEFAULT 0,
    RowVersion           rowversion       NOT NULL
);

CREATE INDEX IX_SyncSession_ProjectStarted
    ON dbo.SyncSession (ProjectExternalId, StartedAtUtc DESC);

CREATE UNIQUE INDEX UX_SyncSession_RunningPerProjectDirection
    ON dbo.SyncSession (ProjectExternalId, Direction)
    WHERE Status = 2;  -- one Running session per project+direction


CREATE TABLE dbo.SyncBatch (
    SyncBatchId          bigint           NOT NULL IDENTITY PRIMARY KEY,
    SyncSessionId        uniqueidentifier NOT NULL,
    BatchNumber          int              NOT NULL,
    EntityKind           tinyint          NOT NULL,
    Phase                tinyint          NOT NULL,  -- 1=Fetch, 2=Map, 3=Persist
    Status               tinyint          NOT NULL,
    PageNumber           int              NULL,
    RecordCount          int              NOT NULL DEFAULT 0,
    StartedAtUtc         datetimeoffset   NOT NULL,
    CompletedAtUtc       datetimeoffset   NULL,
    Error                nvarchar(max)    NULL,

    CONSTRAINT FK_SyncBatch_Session
        FOREIGN KEY (SyncSessionId) REFERENCES dbo.SyncSession(SyncSessionId)
);

CREATE UNIQUE INDEX UX_SyncBatch_SessionBatch
    ON dbo.SyncBatch (SyncSessionId, BatchNumber, EntityKind, Phase);


CREATE TABLE dbo.SyncCursor (
    SyncCursorId         bigint           NOT NULL IDENTITY PRIMARY KEY,
    ProjectExternalId    nvarchar(128)    NOT NULL,
    EntityKind           tinyint          NOT NULL,
    Direction            tinyint          NOT NULL,
    LastSuccessfulAtUtc  datetimeoffset   NULL,
    LastRemoteModifiedAt datetimeoffset   NULL,
    LastPageNumber       int              NULL,
    CheckpointToken      nvarchar(256)    NULL,
    UpdatedAtUtc         datetimeoffset   NOT NULL,
    UpdatedBySyncSession uniqueidentifier NULL,

    CONSTRAINT FK_SyncCursor_Session
        FOREIGN KEY (UpdatedBySyncSession) REFERENCES dbo.SyncSession(SyncSessionId)
);

CREATE UNIQUE INDEX UX_SyncCursor_Scope
    ON dbo.SyncCursor (ProjectExternalId, EntityKind, Direction);


CREATE TABLE dbo.ExternalIdentityMap (
    ExternalIdentityMapId bigint          NOT NULL IDENTITY PRIMARY KEY,
    SourceSystem          nvarchar(32)    NOT NULL,  -- 'Primavera'
    EntityKind            tinyint         NOT NULL,
    ExternalId            nvarchar(128)   NOT NULL,
    LocalEntityId         nvarchar(64)    NOT NULL,
    ProjectExternalId     nvarchar(128)   NULL,
    RemoteModifiedAtUtc   datetimeoffset  NULL,
    LocalModifiedAtUtc    datetimeoffset  NULL,
    RemoteRevision        nvarchar(64)    NULL,
    CreatedAtUtc          datetimeoffset  NOT NULL,
    LastSyncedAtUtc       datetimeoffset  NULL,
    LastSyncSessionId     uniqueidentifier NULL,

    CONSTRAINT FK_ExternalIdentityMap_Session
        FOREIGN KEY (LastSyncSessionId) REFERENCES dbo.SyncSession(SyncSessionId)
);

CREATE UNIQUE INDEX UX_ExternalIdentityMap_External
    ON dbo.ExternalIdentityMap (SourceSystem, EntityKind, ExternalId);

CREATE INDEX IX_ExternalIdentityMap_Local
    ON dbo.ExternalIdentityMap (EntityKind, LocalEntityId);


CREATE TABLE dbo.OutboundChangeOutbox (
    OutboundChangeOutboxId bigint          NOT NULL IDENTITY PRIMARY KEY,
    ProjectExternalId      nvarchar(128)   NOT NULL,
    EntityKind             tinyint         NOT NULL,
    LocalEntityId          nvarchar(64)    NOT NULL,
    ExternalId             nvarchar(128)   NULL,
    ChangeVersion          bigint          NOT NULL,
    ChangedFieldsJson      nvarchar(max)   NOT NULL,
    IdempotencyKey         nvarchar(256)   NOT NULL,
    Status                 tinyint         NOT NULL,
    AttemptCount           int             NOT NULL DEFAULT 0,
    NextAttemptAtUtc       datetimeoffset  NULL,
    CreatedAtUtc           datetimeoffset  NOT NULL,
    DeliveredAtUtc         datetimeoffset  NULL,
    LastError              nvarchar(max)   NULL,
    LastSyncSessionId      uniqueidentifier NULL,
    CorrelationId          nvarchar(128)   NOT NULL,

    CONSTRAINT FK_OutboundOutbox_Session
        FOREIGN KEY (LastSyncSessionId) REFERENCES dbo.SyncSession(SyncSessionId)
);

CREATE UNIQUE INDEX UX_OutboundOutbox_Idempotency
    ON dbo.OutboundChangeOutbox (IdempotencyKey);

CREATE INDEX IX_OutboundOutbox_Pending
    ON dbo.OutboundChangeOutbox (ProjectExternalId, Status, NextAttemptAtUtc)
    WHERE Status IN (1, 4);


CREATE TABLE dbo.SyncConflict (
    SyncConflictId        uniqueidentifier NOT NULL PRIMARY KEY,
    ProjectExternalId     nvarchar(128)    NOT NULL,
    EntityKind            tinyint          NOT NULL,
    ExternalId            nvarchar(128)    NOT NULL,
    LocalEntityId         nvarchar(64)     NOT NULL,
    FieldName             nvarchar(128)    NOT NULL,
    LocalValueJson        nvarchar(max)    NULL,
    RemoteValueJson       nvarchar(max)    NULL,
    LocalModifiedAtUtc    datetimeoffset   NULL,
    RemoteModifiedAtUtc   datetimeoffset   NULL,
    DetectedAtUtc         datetimeoffset   NOT NULL,
    DetectedBySyncSession uniqueidentifier NOT NULL,
    Status                tinyint          NOT NULL,
    ResolvedAtUtc         datetimeoffset   NULL,
    ResolvedBy            nvarchar(128)    NULL,
    ResolutionNote        nvarchar(max)    NULL,

    CONSTRAINT FK_SyncConflict_Session
        FOREIGN KEY (DetectedBySyncSession) REFERENCES dbo.SyncSession(SyncSessionId)
);

CREATE INDEX IX_SyncConflict_Open
    ON dbo.SyncConflict (ProjectExternalId, Status, DetectedAtUtc DESC)
    WHERE Status = 1;


CREATE TABLE dbo.SyncSessionEvent (
    SyncSessionEventId   bigint           NOT NULL IDENTITY PRIMARY KEY,
    SyncSessionId        uniqueidentifier NOT NULL,
    OccurredAtUtc        datetimeoffset   NOT NULL,
    EventType            nvarchar(64)     NOT NULL,
    EntityKind           tinyint          NULL,
    BatchNumber          int              NULL,
    Message              nvarchar(1000)   NULL,
    DetailsJson          nvarchar(max)    NULL,

    CONSTRAINT FK_SyncSessionEvent_Session
        FOREIGN KEY (SyncSessionId) REFERENCES dbo.SyncSession(SyncSessionId)
);

CREATE INDEX IX_SyncSessionEvent_SessionTime
    ON dbo.SyncSessionEvent (SyncSessionId, OccurredAtUtc);
```

---

## Idempotency key (no tenant in DB)

```text
{projectExternalId}:{entityKind}:{localEntityId}:{changeVersion}
```

---

## EF Core entities

```csharp
namespace PrimaveraSync.Infrastructure.Persistence;

public enum EntityKind : byte
{
    Project = 1,
    Wbs = 2,
    Activity = 3,
    Assignment = 4,
    Relation = 5
}

public enum SyncDirection : byte { Inbound = 1, Outbound = 2, Reconcile = 3 }
public enum SyncMode : byte { Full = 1, Incremental = 2 }
public enum SyncSessionStatus : byte { Pending = 1, Running = 2, Completed = 3, Failed = 4, Cancelled = 5 }
public enum SyncBatchPhase : byte { Fetch = 1, Map = 2, Persist = 3 }
public enum OutboxStatus : byte { Pending = 1, InProgress = 2, Delivered = 3, Failed = 4, Conflict = 5 }
public enum ConflictStatus : byte { Open = 1, ResolvedRemote = 2, ResolvedLocal = 3, Skipped = 4 }

public sealed class SyncSession
{
    public Guid SyncSessionId { get; set; }
    public string ProjectExternalId { get; set; } = null!;
    public SyncDirection Direction { get; set; }
    public SyncMode Mode { get; set; }
    public SyncSessionStatus Status { get; set; }
    public string CorrelationId { get; set; } = null!;
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string? InitiatedBy { get; set; }
    public int RecordsFetched { get; set; }
    public int RecordsApplied { get; set; }
    public int RecordsSkipped { get; set; }
    public int RecordsFailed { get; set; }
    public int ConflictsDetected { get; set; }
    public string? LastError { get; set; }
    public bool Retryable { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public ICollection<SyncBatch> Batches { get; set; } = new List<SyncBatch>();
    public ICollection<SyncSessionEvent> Events { get; set; } = new List<SyncSessionEvent>();
}

public sealed class SyncBatch
{
    public long SyncBatchId { get; set; }
    public Guid SyncSessionId { get; set; }
    public int BatchNumber { get; set; }
    public EntityKind EntityKind { get; set; }
    public SyncBatchPhase Phase { get; set; }
    public SyncSessionStatus Status { get; set; }
    public int? PageNumber { get; set; }
    public int RecordCount { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public string? Error { get; set; }

    public SyncSession Session { get; set; } = null!;
}

public sealed class SyncCursor
{
    public long SyncCursorId { get; set; }
    public string ProjectExternalId { get; set; } = null!;
    public EntityKind EntityKind { get; set; }
    public SyncDirection Direction { get; set; }
    public DateTimeOffset? LastSuccessfulAtUtc { get; set; }
    public DateTimeOffset? LastRemoteModifiedAt { get; set; }
    public int? LastPageNumber { get; set; }
    public string? CheckpointToken { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid? UpdatedBySyncSession { get; set; }
}

public sealed class ExternalIdentityMap
{
    public long ExternalIdentityMapId { get; set; }
    public string SourceSystem { get; set; } = null!;
    public EntityKind EntityKind { get; set; }
    public string ExternalId { get; set; } = null!;
    public string LocalEntityId { get; set; } = null!;
    public string? ProjectExternalId { get; set; }
    public DateTimeOffset? RemoteModifiedAtUtc { get; set; }
    public DateTimeOffset? LocalModifiedAtUtc { get; set; }
    public string? RemoteRevision { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastSyncedAtUtc { get; set; }
    public Guid? LastSyncSessionId { get; set; }
}

public sealed class OutboundChangeOutbox
{
    public long OutboundChangeOutboxId { get; set; }
    public string ProjectExternalId { get; set; } = null!;
    public EntityKind EntityKind { get; set; }
    public string LocalEntityId { get; set; } = null!;
    public string? ExternalId { get; set; }
    public long ChangeVersion { get; set; }
    public string ChangedFieldsJson { get; set; } = null!;
    public string IdempotencyKey { get; set; } = null!;
    public OutboxStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? DeliveredAtUtc { get; set; }
    public string? LastError { get; set; }
    public Guid? LastSyncSessionId { get; set; }
    public string CorrelationId { get; set; } = null!;
}

public sealed class SyncConflict
{
    public Guid SyncConflictId { get; set; }
    public string ProjectExternalId { get; set; } = null!;
    public EntityKind EntityKind { get; set; }
    public string ExternalId { get; set; } = null!;
    public string LocalEntityId { get; set; } = null!;
    public string FieldName { get; set; } = null!;
    public string? LocalValueJson { get; set; }
    public string? RemoteValueJson { get; set; }
    public DateTimeOffset? LocalModifiedAtUtc { get; set; }
    public DateTimeOffset? RemoteModifiedAtUtc { get; set; }
    public DateTimeOffset DetectedAtUtc { get; set; }
    public Guid DetectedBySyncSession { get; set; }
    public ConflictStatus Status { get; set; }
    public DateTimeOffset? ResolvedAtUtc { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNote { get; set; }
}

public sealed class SyncSessionEvent
{
    public long SyncSessionEventId { get; set; }
    public Guid SyncSessionId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string EventType { get; set; } = null!;
    public EntityKind? EntityKind { get; set; }
    public int? BatchNumber { get; set; }
    public string? Message { get; set; }
    public string? DetailsJson { get; set; }

    public SyncSession Session { get; set; } = null!;
}
```

---

## DbContext configuration

```csharp
public sealed class SyncDbContext : DbContext
{
    public SyncDbContext(DbContextOptions<SyncDbContext> options) : base(options) { }

    public DbSet<SyncSession> SyncSessions => Set<SyncSession>();
    public DbSet<SyncBatch> SyncBatches => Set<SyncBatch>();
    public DbSet<SyncCursor> SyncCursors => Set<SyncCursor>();
    public DbSet<ExternalIdentityMap> ExternalIdentityMaps => Set<ExternalIdentityMap>();
    public DbSet<OutboundChangeOutbox> OutboundChangeOutbox => Set<OutboundChangeOutbox>();
    public DbSet<SyncConflict> SyncConflicts => Set<SyncConflict>();
    public DbSet<SyncSessionEvent> SyncSessionEvents => Set<SyncSessionEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SyncSession>(e =>
        {
            e.ToTable("SyncSession");
            e.HasKey(x => x.SyncSessionId);
            e.Property(x => x.RowVersion).IsRowVersion();
            e.HasIndex(x => new { x.ProjectExternalId, x.StartedAtUtc });
        });

        modelBuilder.Entity<SyncCursor>(e =>
        {
            e.ToTable("SyncCursor");
            e.HasIndex(x => new { x.ProjectExternalId, x.EntityKind, x.Direction }).IsUnique();
        });

        modelBuilder.Entity<ExternalIdentityMap>(e =>
        {
            e.ToTable("ExternalIdentityMap");
            e.HasIndex(x => new { x.SourceSystem, x.EntityKind, x.ExternalId }).IsUnique();
            e.HasIndex(x => new { x.EntityKind, x.LocalEntityId });
        });

        modelBuilder.Entity<OutboundChangeOutbox>(e =>
        {
            e.ToTable("OutboundChangeOutbox");
            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasIndex(x => new { x.ProjectExternalId, x.Status, x.NextAttemptAtUtc });
        });

        modelBuilder.Entity<SyncConflict>(e =>
        {
            e.ToTable("SyncConflict");
            e.HasIndex(x => new { x.ProjectExternalId, x.Status, x.DetectedAtUtc });
        });
    }
}
```

---

## Host wiring (per-tenant connection)

```csharp
// Resolve tenant DB connection at request/job boundary — not inside actor ctor
services.AddDbContext<SyncDbContext>((sp, options) =>
{
    var tenantConnection = sp.GetRequiredService<ITenantConnectionResolver>()
        .GetConnectionString(); // one DB per tenant

    options.UseSqlServer(tenantConnection);
});
```

Akka actors resolve `SyncDbContext` via `IServiceScopeFactory` per message; the scope inherits the tenant connection already set on the ambient context or factory.

---

## Minimal v1

| Table | Required |
|-------|----------|
| `SyncSession` | Yes |
| `SyncCursor` | Yes |
| `ExternalIdentityMap` | Yes |
| `OutboundChangeOutbox` | Yes (if outbound) |
| `SyncConflict` | Yes (if two-way) |
| `SyncBatch` | When runs are large |
| `SyncSessionEvent` | Optional |
