# Floor2Plan — Primavera EPPM MES sync: Copilot implementation prompt

**Purpose:** Copy-paste prompt for GitHub Copilot (or Claude Code) in the **external Floor2Plan monolith repo** to implement on-premises Primavera EPPM ↔ MES sync per the architecture design.

**Source design (copy into monolith or link):**

- `docs/modularization/integrations/mes-primavera-eppm-sync-architecture.md` (copy from SandBox `docs/mes-primavera-eppm-sync-architecture.md`)
- `docs/modularization/integrations/primavera-eppm-sync-tables.md` (copy from SandBox `docs/primavera-eppm-sync-tables.md`)

---

## Copy-paste prompt for Copilot

```text
@workspace Implement Primavera EPPM ↔ Floor2Plan/MES sync (on-premises, Akka.NET, mapping-first)

## Context

Floor2Plan is a shipyard MES/planning application with its own domain models (Project, Component, Activity).
Primavera P6 EPPM runs **on-premises** and is the system of record for schedule structure.
We need inbound sync (EPPM → map → MES/F2P models) and limited feedback (F2P/MES changes → EPPM).
Start with **1–2 pilot Primavera projects**.

**This is NOT a raw DB mirror.** Mapping is the product. Akka orchestrates; existing MES application services persist domain data.

## Read first (mandatory)

1. docs/modularization/integrations/mes-primavera-eppm-sync-architecture.md
2. docs/modularization/integrations/primavera-eppm-sync-tables.md
3. docs/modularization/platform-actor-standard.md
4. docs/modularization/module-composition-di.md
5. docs/modularization/agent-rules.md (or .github/copilot-instructions.md)
6. docs/coding-standards/csharp-coding-standards.md

SandBox reference only (do not copy behaviour blindly — validate against monolith):
- ApiImportActorPoc/ — external ID upsert ideas, actor import patterns [reference_only]
- docs/floor2plan-akka-actor-integration-design.md — P6 actor + mapper pack shape [reference_only]

## Phase 0 — Forensic (do this before writing new code)

Search the monolith and document with file:line citations in
`docs/modularization/integrations/primavera-eppm-sync-phase0.md`:

- Existing Primavera / P6 / EPPM integration code (REST clients, Hangfire jobs, submodules)
- Existing Project, Component, Activity application services and repositories
- EntityExternalId or equivalent external ID registry
- Akka hosting setup (ActorSystem, Akka.Hosting registration)
- SaveChanges handlers that fire on Activity/Component updates (avoid hidden orchestration)
- Floor2Plan events published after planning/activity changes (feedback triggers)
- Nearest bounded context for Sync/Import (likely Application.Sync or extracted Import module)

Stop and list gaps as [NEEDS REVIEW] if EPPM endpoints or MES service boundaries are unclear.

## Architecture (non-negotiable)

### Pipeline

Inbound:
  Collect (parallel EPPM fetch) → EppmRawStaging → Map (IEppmToMesMapper pack)
  → Apply (IMesSyncApplier → existing MES services) → Advance SyncCursor

Feedback:
  Post-commit domain event → FeedbackOutbox → MesToEppmMapper → EppmGateway PUT/POST

### EPPM (on-premises only)

- Base URL: configurable internal host, e.g. https://p6.yard.local/p6ws/services/restapi
- Auth: POST /restapi/login → JSESSIONID cookie; re-login on 401
- Pagination: Limit + Offset on GET
- Parallel fetch: start maxParallelRequests = 4
- Page size: 500 default
- All HTTP through EppmGatewayActor + IEppmClient — nowhere else

### Akka actors

SyncSupervisor
├── ProjectCatalogActor              # GET /project summaries, pilot sizing
├── ProjectSyncOrchestratorActor     # inbound session per project
│   ├── EppmCollectCoordinatorActor  # parallel page fetch per entity kind
│   ├── EppmToMesMappingActor        # invokes mapper pack
│   └── MesApplyActor                # IServiceScopeFactory per message → MES services
├── FeedbackRouterActor
│   └── MesToEppmFeedbackActor
├── EppmGatewayActor                 # sole EPPM HTTP boundary
└── SyncSessionRegistryActor

Rules:
- Ask only at HTTP/API boundary; Tell/Forward inside actors
- MesApplyActor uses IServiceScopeFactory — never inject scoped DbContext into actor ctor
- No SaveChanges orchestration for sync workflow
- CorrelationId + SyncSessionId on all messages and logs (platform-correlation standard)
- Client/yard variance via integration pack (IEppmToMesMapper, IMesToEppmMapper), not if (tenant) in core

### Entity kind order (inbound)

1. Wbs → Component
2. Activity
3. Relation
(Assignment optional later)

Within each kind:
  parallel collect → sequential map → sequential apply → advance cursor for that kind → NextKind

Cursor advances ONLY after MES apply succeeds for ALL pages of that entity kind.

### Full vs incremental sync (see architecture §12)

A **full** inbound run bootstraps a pilot project. **Incremental** (partial) sync is the steady state afterward.
Both use the same pipeline — only SyncMode and the EPPM filter differ.

```csharp
public enum SyncMode { Full, Incremental }
```

| Mode | When | EPPM collect filter |
|------|------|---------------------|
| Full | First sync of pilot project; rebuild one entity kind after mapper change | All records for project + kind (ignore cursor) |
| Incremental | Scheduled runs after successful full sync | LastUpdateDate >= SyncCursor.LastRemoteModifiedAt - 2min overlap |

Lifecycle:
  1. StartInboundSync(Full)   → populate MES + ExternalIdentityMap → set SyncCursor per kind
  2. StartInboundSync(Incremental) → fetch changed rows only → upsert → advance cursor

Rules:
- Incremental is safe because ExternalIdentityMap upserts by external id (no duplicates)
- Cursor advances ONLY after MES apply succeeds for all pages of that entity kind
- Failed incremental run does NOT advance cursor — retry re-processes same window
- Feedback (FeedbackOutbox) is always partial — independent of inbound full/incremental
- API: POST /api/primavera-sync/inbound { projectObjectId, mode: "Full" | "Incremental" }

Implement SyncMode on StartInboundSync, SyncSession.Mode column, and orchestrator filter logic in Slice 4–5.

### Pilot field policy (v1)

| Entity / field | Direction |
|----------------|-----------|
| WBS structure, activity name, codes | Inbound only; EPPM wins |
| Percent complete, planned/actual dates from Floor2Plan | Feedback outbound |
| Full bidirectional structure | Out of scope v1 |

### Database (per-tenant DB — NO TenantId columns)

Implement tables from primavera-eppm-sync-tables.md plus:

- EppmRawStaging (SyncSessionId, EntityKind, PageNumber, PayloadJson, ...)
- EppmProjectCatalog (project sizing for pilot selection)
- MappingIssue (mapping failures)
- FeedbackOutbox (post-commit outbound queue)

ExternalIdentityMap: (SourceSystem='Primavera', EntityKind, ExternalId) → LocalEntityId

## Module layout (target)

Create or extend bounded context (name after Phase 0 — e.g. PrimaveraSync or extend Import):

PrimaveraSync/
├── PrimaveraSync.Contracts/       # commands, events, MesChangeSet, DTOs
├── PrimaveraSync.Domain/          # SyncSession, SyncCursor, policies (no EPPM types)
├── PrimaveraSync.Application/     # ports: IEppmClient, IEppmToMesMapper, IMesSyncApplier, IMesToEppmMapper
├── PrimaveraSync.Infrastructure/
│   ├── Eppm/                      # EppmClient, session auth, paging
│   ├── Persistence/               # EF entities, sync tables
│   ├── Actors/                    # all actors listed above
│   └── Strangler/                 # [StranglerAdapter] legacy P6 paths if any
├── PrimaveraSync.Api/             # MapPrimaveraSyncEndpoints
└── PrimaveraSync.Tests/

Registration (host Program.cs only):
  services.AddPrimaveraSyncModule(configuration);
  services.AddAkka(..., registry => registry.RegisterPrimaveraSyncActors(...));
  app.MapPrimaveraSyncEndpoints();

No Volo.Abp.*, AbpModule, or AbpDbContext in new code.

## Key interfaces (implement in Application/Infrastructure)

IEppmClient
  - LoginAsync, FetchPageAsync(EntityKind, projectObjectId, offset, limit, modifiedSince)
  - GetProjectSummariesAsync (catalog)
  - WriteAsync (feedback)

IEppmToMesMapper (integration pack — yard-specific)
  - Map(EntityKind, IReadOnlyList<EppmRecordDto>, MesMappingContext) → MesMappingResult

IMesSyncApplier
  - ApplyAsync(MesChangeSet) → calls existing IProjectService / IComponentService / IActivityService (wire to real monolith services from Phase 0)

IMesToEppmMapper (integration pack)
  - Map(MesPlanningChanged) → EppmFeedbackPayload?

MesChangeSet:
  - Components, Activities, Relations changes with ExternalId + attributes dictionary

## API endpoints (v1)

POST /api/primavera-sync/catalog/refresh
GET  /api/primavera-sync/catalog
POST /api/primavera-sync/catalog/{projectObjectId}/enrich

POST /api/primavera-sync/inbound              # StartInboundSync(projectId, Full|Incremental)
GET  /api/primavera-sync/sessions/{syncId}
POST /api/primavera-sync/inbound/test         # collect + map preview, no MES apply

POST /api/primavera-sync/feedback/drain       # drain FeedbackOutbox for project

## Configuration (appsettings)

PrimaveraSync:
  Eppm:
    BaseUrl: ...
    Username: ...
    Password: ...                              # secret / KeyVault
    SessionRenewalMinutes: 25
    PageSize: 500
    MaxParallelRequests: 4
    TimeoutSeconds: 120
  Pilot:
    ProjectExternalIds: ["PILOT-1"]
    EnabledEntityKinds: [Wbs, Activity, Relation]
    OutboundEnabled: true

## Delivery order (one PR slice at a time)

### Slice 1 — Phase 0 doc + foundations
- primavera-eppm-sync-phase0.md with evidence
- IEppmClient + session login + GET /project (catalog)
- Sync tables migration
- EppmGatewayActor + ProjectCatalogActor
- Tests: login integration test (mock or test EPPM), catalog maps SummaryActivityCount

### Slice 2 — Collect + staging
- EppmCollectCoordinatorActor + FetchWorkerActor
- EppmRawStaging read/write
- Parallel fetch with max 4 workers
- POST inbound/test returns mapped preview (mapper stub)

### Slice 3 — Mapper pack + mapping issues
- IEppmToMesMapper implementation for pilot yard (start with Wbs + Activity)
- MappingIssue persistence
- Unit tests with golden EPPM JSON fixtures

### Slice 4 — MES apply + cursor
- MesApplyActor + IMesSyncApplier → real MES services
- ExternalIdentityMap upsert
- SyncCursor advance per entity kind after apply
- ProjectSyncOrchestratorActor full inbound state machine (NextKind)
- Integration test: inbound pilot project → MES rows + identity map

### Slice 5 — Incremental (partial) inbound
- StartInboundSync(Incremental) uses SyncCursor per entity kind
- EPPM filter: LastUpdateDate >= cursor.LastRemoteModifiedAt - 2min overlap
- Verify second run fetches fewer pages than full sync (mock or integration test)
- Tests: Inbound_full_then_incremental_fetches_subset, Inbound_incremental_does_not_duplicate,
  Inbound_incremental_failure_does_not_advance_cursor

### Slice 6 — Feedback
- FeedbackOutbox + post-commit hook on planning/activity changes (find existing event bus from Phase 0)
- MesToEppmFeedbackActor + IMesToEppmMapper (limited fields only)
- Manual drain endpoint

## Tests required

| Test | Type |
|------|------|
| EppmClient_login_obtains_session | Integration (mock HTTP or test server) |
| ProjectCatalog_maps_summary_activity_count | Integration |
| CollectCoordinator_stores_pages_in_staging | Unit/actor test |
| EppmToMesMapper_maps_wbs_and_activity | Unit (golden files) |
| MesSyncApplier_upserts_by_external_id | Integration (DB) |
| Inbound_full_pilot_project_persists_mes_models | Integration |
| Inbound_full_then_incremental_fetches_subset | Integration (mock EPPM) |
| Inbound_incremental_does_not_duplicate | Integration |
| Inbound_incremental_failure_does_not_advance_cursor | Integration |
| Inbound_full_sets_cursor_per_entity_kind | Integration |
| SyncCursor_advances_only_after_apply | Unit |
| Feedback_outbox_delivers_to_eppm | Integration (mock EPPM) |

## Quality gates

- Cite file:line for every legacy claim; [NEEDS REVIEW] when uncertain
- dotnet build && dotnet test on touched projects — fix failures
- No AutoMapper in new module unless trivial profile + AssertConfigurationIsValid test
- Platform.Serilog.Logging + correlation props on actor logs
- Explicit mapping preferred over reflection magic
- Do not add SaveChanges handlers for sync orchestration

## Acceptance criteria

- [ ] On-prem EPPM session auth works from F2P host
- [ ] Project catalog lists projects ranked by SummaryActivityCount
- [ ] Pilot project inbound: Wbs → Activity → Relation collect/map/apply
- [ ] Parallel collect (4 workers) + EppmRawStaging
- [ ] MesChangeSet applied via existing MES services, not raw EF in actors
- [ ] Full inbound bootstraps pilot project; SyncCursor set per entity kind
- [ ] Incremental inbound fetches changed rows only (partial sync after full)
- [ ] ExternalIdentityMap populated; incremental re-run idempotent (no duplicates)
- [ ] SyncCursor per (ProjectExternalId, EntityKind, Direction); not advanced on failure
- [ ] Mapping issues recorded without failing entire batch (policy TBD in Phase 0)
- [ ] Feedback pushes limited fields to EPPM from FeedbackOutbox
- [ ] Mapper pack separable from core (integration pack pattern)
- [ ] All tests above passing

Begin with Phase 0 forensic. Output phase0.md before implementation PRs.
Do not invent MES service APIs — wire to services found in the monolith or mark [NEEDS REVIEW].
```

---

## Where to put this in the monolith

| File | Action |
|------|--------|
| `.github/copilot-instructions.md` | Add a short pointer: "For Primavera EPPM MES sync, see `docs/modularization/integrations/floor2plan-primavera-eppm-mes-sync-copilot-prompt.md`" |
| `docs/modularization/integrations/floor2plan-primavera-eppm-mes-sync-copilot-prompt.md` | Copy this file |
| `docs/modularization/integrations/mes-primavera-eppm-sync-architecture.md` | Copy from SandBox |
| `docs/modularization/integrations/primavera-eppm-sync-tables.md` | Copy from SandBox |

---

## Optional: shorten trigger for day-to-day Copilot

```text
@workspace Primavera EPPM MES sync — continue current slice

Read docs/modularization/integrations/mes-primavera-eppm-sync-architecture.md and primavera-eppm-sync-phase0.md.
Follow floor2plan-primavera-eppm-mes-sync-copilot-prompt.md delivery order.
Mapping-first; on-prem EPPM session auth; Akka actors per platform-actor-standard.md; no ABP in new code.
Cite file:line; run dotnet test on touched projects.
```
