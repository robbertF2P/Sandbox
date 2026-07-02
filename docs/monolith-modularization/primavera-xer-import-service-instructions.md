# Primavera XER project import service — build instructions

**Purpose:** Guide Claude (or any AI assistant) to design and implement:

1. A **vendor-neutral intermediate import model** (`ProjectStructureImportModel`) shared by all structure import sources.
2. A **reusable `ProjectImportService`** that reliably inserts and updates from that model (batched EF, correct FKs, idempotent upsert).
3. A **Primavera XER adapter** that maps XER → intermediate model (~9,000 components, ~250,000 activities).

**Design priority:** correctness and a clean reusable service over premature optimization. **Do not** introduce HiLo, schema migrations, or bulk-copy infrastructure unless profiling proves batched `SaveChanges` with existing `IDENTITY` columns is insufficient.

XER is the first adapter; Sciforma, Excel/Aspose, PLM packs, and JSON/API import must plug into the **same** persist service — not duplicate EF logic per vendor.

**Audience:** Engineers and AI agents implementing the Import / Sync bounded context in Platform 2.0.

**Prerequisite:** Phase 0 inventory and bounded-context map in the **external monolith** (`docs/modularization/00-inventory.md`, `02-bounded-context-map.md`).

---

## Critical: where the code lives

```yaml
implementation_target:
  repo: "<external Floor2Plan monolith — open as workspace root>"
  legacy_xer_entry_points:
    - "Src/Application/Application.Sync/ImportJobs/Xer/ImportXerJob.cs"      # EP-2001
    - "Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs" # EP-306 POST ImportXer*
  sandbox_role: reference_only
```

| Location | Role |
|----------|------|
| **External F2P monolith** | Legacy XER parser, import jobs, EF entities, `SaveChanges` paths — **implement here** |
| **This SandBox repo** | Target-state patterns only — label every citation `reference_only: true` |

**Do not** implement production import logic in SandBox. **Do not** infer legacy behaviour from POCs without validating against monolith code and characterization tests.

If the external repo is unavailable, **stop** and list missing paths. Do not guess at legacy table shapes or XER field mappings.

### SandBox reference implementations (`reference_only: true`)

| Path | Use for |
|------|---------|
| `ApiImportActorPoc/server/ApiImportActorPoc.Core/Import/ProjectImportUpsertService.cs` | **Anti-pattern at scale** — `SaveChangesAsync` per entity; correct ordering ideas only |
| `ApiImportActorPoc/README.md` | Canonical import model: Project → Component tree → Activity → Assignment → ActivityRelation; external id upsert |
| `ApiImportActorPoc/server/ApiImportActorPoc.Core/Import/ComponentPersistOrderer.cs` | Templates-first sibling ordering |
| `ApiImportActorPoc/server/ApiImportActorPoc.Core/Import/ProjectImportIdentityResolver.cs` | External id → internal id resolution |
| `PrimaveraExcelReader/` | Primavera field naming, `ExternalIds["Primavera"]` convention |
| `docs/monolith-modularization/fixtures/minimal-project-structure.xer` | Golden XER for embedded integration test |
| `docs/monolith-modularization/platform-actor-standard.md` | One persist actor; no `SaveChanges` orchestration |
| `docs/monolith-modularization/change-handler-migration.md` | Handler inventory, migration to explicit events — **mandatory read for bulk import** |
| `docs/Modularization/01-entry-points.md` | EP-2001, EP-306 |

---

## Problem statement

Typical Primavera XER imports for large shipbuilding / EPC projects:

| Dimension | Scale |
|-----------|-------|
| Components (WBS / PROJWBS) | ~9,000 |
| Activities (TASK) | ~250,000 |
| Activity relations (TASKPRED) | tens of thousands |
| Assignments (TASKRSRC) | varies |

The **legacy XER import is slow and fragile**. Root causes to confirm in Phase 0 (cite `file:line`):

1. **Per-entity `SaveChanges`** — N round-trips to SQL Server (POC does this explicitly; legacy likely similar).
2. **EF change-tracker overhead** on hundreds of thousands of tracked entities.
3. **N+1 queries** — load parent, insert child, reload graph.
4. **Per-row identity assignment** — legacy may `SaveChanges` after every insert just to obtain `IDENTITY` FKs; batched multi-pass persist fixes this **without** changing PK strategy.
5. **Single giant transaction** or **no transaction** — timeouts vs partial corrupt state.
6. **Fragile parsing** — assumes field order, missing null handling, encoding issues, duplicate external ids.
7. **SaveChanges change-handler chain** — `Floor2PlanDbContext` registers **55+ handlers** (`change-handler-migration.md`); debugging has shown **~28 handlers firing on a single `SaveChanges`** during XER import. Each batch save can re-run the full chain over thousands of tracked entities, multiplying work far beyond the insert itself.

**Target:** Reliable insert and update with correct relationships; acceptable performance at 250k activities through **batching + handler suppression** — not through invasive schema changes on day one.

---

## Design philosophy — minimal impact, expand later

| Priority | What to build first | Defer until proven necessary |
|----------|---------------------|------------------------------|
| 1 | **`ProjectStructureImportModel`** — vendor-neutral, validated, flat | Nested graph APIs in persist layer |
| 2 | **`IProjectImportService`** — correct insert, update, FK wiring, external-id upsert | HiLo / sequence migrations |
| 3 | **Batched `SaveChanges`** with existing `IDENTITY` columns | `SqlBulkCopy`, staging tables |
| 4 | **Handler suppression** + single post-import event | Per-handler rewrites |
| 5 | **Embedded XER integration test** | Nightly 250k scale benchmark |

A good intermediate model + reliable persist service is the **product**. XER is the first consumer. Once that works, wire Sciforma, Excel, PLM packs, and JSON API through the **same** `IProjectImportService` — no second persist path.

**HiLo is optional optimization**, not a v1 requirement. It touches entity configuration, migrations, and sequence ownership across shared legacy tables — higher blast radius than batched inserts on the current schema. Revisit only if, after handler suppression and batching, persist time still exceeds the agreed SLA.

---

## Change handlers — be critical (likely #1 performance killer)

> **Do not assume batched `SaveChanges` alone fixes import speed.** If handlers still run on every batch, import will remain slow or fail (timeouts, deadlocks, Hangfire storms).

Prior debugging observed **28 change handlers triggered on one `SaveChanges`** during XER import. The monolith documents **55+ handlers** on `Floor2PlanDbContext` overall. Claude must **inventory, measure, and design around** this chain — not ignore it.

### Why this destroys bulk import

With batched persist (~50 `SaveChanges` calls for 250k activities at 5k per batch):

| Handler pattern | Approx. handler executions | Effect |
|-----------------|----------------------------|--------|
| 28 handlers × **per `SaveChanges`** | 50 × 28 = **1,400** chain runs | Each run may scan entire change tracker, enqueue jobs, write audit rows |
| Handlers × **per changed entity** | 250k × 28 = **7M** invocations | Import becomes hours or never finishes |
| Handler enqueues Hangfire job per activity | 250k jobs | Queue meltdown |

Even “cheap” handlers become expensive at this scale. **Handler bypass or deferred effects are not optional** for production-scale XER.

### Phase 0 — mandatory handler audit

Add to `primavera-xer-import-phase0.md` (cite `change-handler-migration.md`):

```text
Scan Floor2PlanDbContext and related infrastructure for:
- SaveChanges interceptors / overrides
- IChangeHandler / ChangeHandler / OnBeforeSave / OnAfterSave
- ABP domain event dispatch on SaveChanges
- Workflow handlers registered for Activity, Component, Assignment, Project

For EACH handler triggered during XER import, record:
| Handler | Trigger entity | Runs on | Effect (DB / Hangfire / HTTP) | Needed on bulk import? |
```

**Agent searches (external repo):**

```text
SaveChanges
IChangeHandler
ChangeHandler
OnBeforeSave
OnAfterSave
Floor2PlanDbContext
Handler.*Activity
Handler.*Component
SuppressChangeHandler
ImportMode
IsImporting
```

Deliverable: `docs/modularization/integrations/primavera-xer-import-handler-audit.md` with:

- Total handler count registered on `Floor2PlanDbContext`
- Subset that fires for `Activity` / `Component` INSERT during XER (use debugger or temporary counter — **measure, do not guess**)
- Per-handler classification: `required-on-import` | `defer-to-post-import` | `never-on-bulk-import`
- Evidence: `file:line` + test name or debug log snippet

Mark `[NEEDS REVIEW]` for any handler whose business effect is unclear. **Do not delete handlers** without AC coverage per `change-handler-migration.md`.

### Acceptable strategies (pick explicitly in design doc)

| Strategy | When to use | Risk |
|----------|-------------|------|
| **`ImportExecutionScope` flag** on `DbContext` — handlers no-op when `IsBulkImport == true` | Strangler on same `Floor2PlanDbContext` | Must list suppressed handlers; post-import must replay deferred effects |
| **Dedicated import `DbContext`** without handler registration | New Import module on extracted tables or schema | Cleanest for V2; may need `[StranglerAdapter]` mapping |
| **Single post-import event** — `ProjectStructureImported` → one rollup/recalc actor | Replace per-row handler effects (progress, planning sync, notifications) | Requires handler inventory + AC tests for deferred behaviour |
| **`SqlBulkCopy` / staging tables** | Handlers cannot be suppressed safely | Bypasses EF handler pipeline entirely; merge in SQL |

**Unacceptable:**

- “We'll optimize handlers later” while shipping batched `SaveChanges` through the full chain
- Silently skipping handlers without inventory and AC sign-off
- Adding a **new** SaveChanges handler for import orchestration (`platform-actor-standard.md` forbids this)

### Required design in `IProjectImportService`

```csharp
public sealed record ImportExecutionOptions(
    bool SuppressChangeHandlers = true,   // default true for bulk
    bool DeferPlanningRecalculation = true,
    IReadOnlyList<string>? AllowedHandlers = null);  // optional allow-list for gradual cutover
```

Persist flow:

```text
1. Begin ImportExecutionScope (SuppressChangeHandlers = true)
2. Batched inserts/updates (passes 1–4) using existing IDENTITY columns
3. Commit
4. End scope
5. Publish ProjectStructureImported (single integration/domain event)
6. Post-import actor: planning rollup, progress recalc, notifications — ONCE per import, not per row
```

### Tests for handler behaviour

| Test | Asserts |
|------|---------|
| `Import_minimal_embedded_xer_does_not_enqueue_per_row_jobs` | Hangfire/job counter unchanged or exactly 1 post-import job |
| `Import_bulk_scope_suppresses_activity_handlers` | Test double / spy: handler count = 0 during persist (or only allow-listed) |
| `Import_post_import_recalculates_planning_once` | Deferred effect still happens via explicit event |

Add a **diagnostic test** (can be `[Explicit]`) that runs one `SaveChanges` with a single `Activity` and logs how many handlers ran — baseline for regression (“must be 0 inside bulk scope”).

### Performance expectation with handlers

| Scenario | 250k activities (indicative) |
|----------|------------------------------|
| Batched EF + **handlers suppressed** + post-import event | **3–8 minutes** |
| Batched EF + **28 handlers per batch** | **30+ minutes to hours** — treat as **broken** |
| Per-row SaveChanges + handlers | **Hours** — current legacy failure mode |

---

## Non-negotiable rules

1. **Behaviour preservation first** — run legacy import on a golden XER; capture row counts, timings, and sample entities before refactoring.
2. **Three layers, strict boundaries** — `Source adapter` (XER, Excel, PLM, …) → **`ProjectStructureImportModel`** → `ProjectImportService` (EF). No EF entities in adapters; **no vendor types in the persist service**.
3. **One persist service for all sources** — `IProjectImportService.PersistAsync(ProjectStructureImportModel)` is the only bulk-write entry point. XER, Sciforma (EP-2002), Aspose (EP-2004), and integration packs each supply a mapper; they do **not** call `DbContext` directly.
4. **Bulk persist** — batched inserts/updates; **never** `SaveChangesAsync` per row at this scale.
5. **Keep existing `IDENTITY` PKs** — use multi-pass batching; refresh `importKeyMap` from the change tracker (or external-id lookup) after each `SaveChanges`. **No HiLo migration in v1** unless profiling proves it necessary.
6. **Correct upsert** — insert new entities by external id; update existing scalars on re-import; no duplicate rows.
7. **Deferred relations** — activity predecessors need both endpoints mapped; insert relations in a final pass (see POC `deferredRelations` pattern).
8. **One EF boundary** — `ProjectImportService` (or its persist actor) is the sole `DbContext` writer for structure import (`platform-actor-standard.md`).
9. **External ids** — every entity carries `IReadOnlyDictionary<string, string> ExternalIds` (e.g. `"Primavera"`, `"PLM"`, `"Sciforma"`). Upsert matches on `(system, value)`, not name. Vendor-specific ids stay in adapters until mapped into `ExternalIds`.
10. **Import keys inside the batch** — intermediate model links rows with opaque `ImportKey` strings (unique within the batch). Adapters set keys from vendor ids; the persist service never reads Primavera `wbs_id` / `task_id` directly.
11. **No ABP in new modules** — `AddImportModule` / `MapImportEndpoints`; legacy bridged via `[StranglerAdapter]`.
12. **Change handlers** — inventory every handler that runs on `SaveChanges` during import; **suppress or defer** bulk-inappropriate handlers; replay needed effects **once** via `ProjectStructureImported` — never 250k handler executions.
13. **Cite evidence** — `path:line` for legacy claims; `[NEEDS REVIEW]` when uncertain.

---

## XER format (parser contract — adapter input only)

The streaming parser is **XER-specific**. It outputs `XerParseResult` (internal to `Import.Infrastructure.Sources.Xer`). The **`XerProjectStructureAdapter`** maps that to `ProjectStructureImportModel`. The persist service never sees XER tables or field names.

Primavera XER is a line-oriented, tab-delimited text file:

```text
%T    TABLE_NAME
%F    field1    field2    field3
%R    value1    value2    value3
```

### Tables required for structure import (validate names in legacy parser)

| XER table | Maps to | Key fields (typical) |
|-----------|---------|----------------------|
| `PROJECT` | Project | `proj_id`, `proj_short_name` |
| `PROJWBS` | Component (WBS node) | `wbs_id`, `wbs_short_name`, `parent_wbs_id`, `proj_id` |
| `TASK` | Activity | `task_id`, `task_name`, `wbs_id`, `proj_id` |
| `TASKPRED` | ActivityRelation | `task_id`, `pred_task_id`, `pred_type`, `lag_hr_cnt` |
| `TASKRSRC` | Assignment | `taskrsrc_id`, `task_id`, `rsrc_id`, … |
| `RSRC` | Resource metadata | `rsrc_id`, `rsrc_name` (if assignments imported) |

### Parser requirements

- **Stream** the file line-by-line; do not load 250k rows into a single `List` without memory budget (acceptable: build compact structs / arrays; avoid heavy object graphs during parse).
- Tolerate **missing optional fields**; record `ParseIssue` with line number, table, field.
- Build **indexes** during parse: `Dictionary<string, WbsRow>` by `wbs_id`, `Dictionary<string, TaskRow>` by `task_id`.
- Validate **referential integrity** before persist: every `TASK.wbs_id` exists in `PROJWBS`; every `TASKPRED` endpoint exists in `TASK`.
- Support **project filter** when XER contains multiple projects (legacy behaviour — confirm in Phase 0).

---

## Intermediate import model (vendor-neutral contract)

**This is the centre of the design.** All import sources converge here before any database write.

Place types in **`Import.Domain`** (or `Import.Contracts`) — referenced by adapters, the persist service, API, and integration packs. **No** XER, Excel, or PLM types in this project.

### Design principles

| Principle | Detail |
|-----------|--------|
| **Flat lists** | Not nested trees — optimised for bulk insert and 250k+ rows |
| **ImportKey linking** | Opaque string keys unique within the batch (`"cmp:1"`, `"act:42"`). Adapters assign keys; persist service maps `ImportKey` → internal `int` id |
| **ExternalIds for upsert** | Multi-namespace dictionary per entity; persist service resolves existing rows by any `(system, value)` pair |
| **Optional nested JSON** | API/POC may accept nested `ComponentImportPayload` trees; deserializer **flattens** to this model before persist |
| **Parse issues stay upstream** | `ImportValidationResult` wraps model + issues; persist service rejects invalid models |

### Core types (illustrative — implement in monolith)

```csharp
namespace Import.Domain.ProjectStructure;

/// <summary>Vendor-neutral project structure batch. Sole input to IProjectImportService.</summary>
public sealed record ProjectStructureImportModel(
    ProjectImportPart Project,
    IReadOnlyList<ComponentImportPart> Components,
    IReadOnlyList<ActivityImportPart> Activities,
    IReadOnlyList<AssignmentImportPart> Assignments,
    IReadOnlyList<ActivityRelationImportPart> Relations);

public sealed record ProjectImportPart(
    string ImportKey,
    string Name,
    IReadOnlyDictionary<string, string> ExternalIds);

public sealed record ComponentImportPart(
    string ImportKey,
    string? ParentImportKey,          // null = root under project
    string Name,
    bool IsTemplate,
    IReadOnlyDictionary<string, string> ExternalIds);

public sealed record ActivityImportPart(
    string ImportKey,
    string ComponentImportKey,
    string Name,
    IReadOnlyDictionary<string, string> ExternalIds);

public sealed record AssignmentImportPart(
    string ImportKey,
    string ActivityImportKey,
    string PersonName,
    string? Description,
    decimal? BudgetedHours,
    IReadOnlyDictionary<string, string> ExternalIds);

public sealed record ActivityRelationImportPart(
    string SourceActivityImportKey,
    string TargetActivityImportKey,
    ActivityRelationType Type,        // FinishToStart, StartToStart, …
    int? LagDays);

public sealed record ImportValidationResult(
    ProjectStructureImportModel? Model,
    IReadOnlyList<ImportIssue> Issues)
{
    public bool IsValid => Model is not null && Issues.Count == 0;
}
```

Align field names and external-id rules with `ApiImportActorPoc` import payloads (`reference_only`) — same semantics, **flat** shape for scale.

### Persist port (single entry point)

```csharp
namespace Import.Application;

public interface IProjectImportService
{
    Task<ProjectImportResult> PersistAsync(
        ProjectStructureImportModel model,
        IProgress<ImportPersistProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
```

**Only** `IProjectImportService` (and its implementation) touches EF for structure import. Vendor jobs call adapters, then this interface.

### Source adapters (one per vendor/format)

Each adapter implements **`IProjectStructureSourceAdapter`** or a dedicated mapper — lives in Infrastructure or an integration pack:

```csharp
public interface IProjectStructureSourceAdapter<TSource>
{
    Task<ImportValidationResult> MapAsync(TSource source, CancellationToken cancellationToken = default);
}
```

| Adapter | Input | Maps to | Legacy entry |
|---------|-------|---------|--------------|
| `XerProjectStructureAdapter` | XER file stream | `ProjectStructureImportModel` | `ImportXerJob` (EP-2001) |
| `SciformaProjectStructureAdapter` | Sciforma export | same | `ImportSciformaJob` (EP-2002) |
| `PrimaveraExcelProjectStructureAdapter` | Excel workbook | same | Aspose / `PrimaveraExcelReader` pattern |
| `PlmProjectStructureAdapter` | PLM canonical JSON | same | PLM pack / `PlmImport` |
| `JsonProjectStructureAdapter` | Nested API JSON | same (flatten) | `ApiImportActorPoc` round-trip |

**XER adapter example** — Primavera ids become import keys and external ids; persist service never sees `wbs_id`:

```csharp
// Inside XerProjectStructureAdapter only:
ComponentImportPart.FromWbs(wbs) => new(
    ImportKey: $"wbs:{wbs.WbsId}",
    ParentImportKey: wbs.ParentWbsId is null ? null : $"wbs:{wbs.ParentWbsId}",
    Name: wbs.ShortName,
    IsTemplate: false,
    ExternalIds: new Dictionary<string, string> { ["Primavera"] = wbs.WbsId });
```

### Validation before persist

Run on the intermediate model (adapter or shared `ProjectStructureImportValidator`):

- Unique `ImportKey` per entity kind within the batch
- Every `ParentImportKey` / `ComponentImportKey` / activity relation endpoint references an existing key
- No duplicate `(system, value)` in `ExternalIds` across the batch
- Component graph is acyclic

Reject persist when `ImportValidationResult.IsValid` is false; return issues to caller (UI / job log).

---

## Persistence strategy — batched EF on existing schema

**Default v1 approach:** keep legacy `UseIdentityColumn()` / SQL Server `IDENTITY`. No HiLo sequences, no PK strategy migration.

### Why this is enough for v1

Multi-pass batching solves the FK problem without pre-assigning ids:

1. Insert a batch of parent components → `SaveChanges` → SQL Server assigns ids → read ids from change tracker into `importKeyMap`.
2. Insert child components with `ParentComponentId` from `importKeyMap`.
3. Insert activity batches with `ComponentId` from `importKeyMap`.
4. Insert relations after all activities exist in `importKeyMap`.

~9k components and ~250k activities at 2k–5k per batch ⇒ **tens of `SaveChanges` calls**, not hundreds of thousands. With handlers suppressed, that is acceptable for v1.

### When to consider HiLo or bulk copy (later only)

| Signal | Escalation |
|--------|------------|
| Persist still > SLA after batching **and** handler suppression | Profile; consider HiLo **or** `SqlBulkCopy` — pick smaller blast radius |
| Cannot suppress handlers safely | Staging tables + merge (bypasses EF handlers) |
| Shared table ownership blocks import flags | Dedicated import schema |

Document the decision in `primavera-xer-import-phase0.md` with timings. **Do not start with HiLo** because it is theoretically faster.

---

## Bulk insert algorithm (required)

`ProjectImportService` implements this — **vendor-agnostic**. Input is always `ProjectStructureImportModel`.

Replace recursive `UpsertComponentsAsync` + per-row `SaveChanges` with a **multi-pass pipeline**:

```text
Pass 0 — Resolve project + existing external ids
  Upsert project row
  Preload externalIdMap: (system, value) → InternalId from EntityExternalId (for re-import)
  Seed importKeyMap from matched existing entities where external ids overlap

Pass 1 — Components (topological order)
  Sort by depth using ParentImportKey (parents before children)
  For each batch:
    - INSERT new components (no matching external id) OR UPDATE existing scalars
    - Set ParentComponentId from importKeyMap[parentImportKey]
    - AddRange → SaveChangesAsync
    - Refresh importKeyMap from tracker: ImportKey → entity.Id (new rows get IDENTITY ids here)
    - Upsert EntityExternalId rows for the batch
    - ChangeTracker.Clear()

Pass 2 — Activities (same insert/update split)
  ComponentId = importKeyMap[activity.ComponentImportKey]
  Batch → SaveChanges → refresh importKeyMap → external ids → Clear()

Pass 3 — Assignments
  ActivityId = importKeyMap[assignment.ActivityImportKey]
  Batch → SaveChanges → refresh maps → Clear()

Pass 4 — Activity relations (deferred; usually insert-only on greenfield)
  SourceActivityId / TargetActivityId from importKeyMap
  Batch → SaveChanges → Clear()
```

### Refreshing `importKeyMap` after `SaveChanges` (IDENTITY)

```csharp
private static void AbsorbGeneratedIds<T>(
    IEnumerable<T> batch,
    Func<T, string> importKeySelector,
    Func<T, int> idSelector,
    Dictionary<string, int> importKeyMap)
{
    foreach (var entity in batch)
        importKeyMap[importKeySelector(entity)] = idSelector(entity);
}
```

Call **before** `ChangeTracker.Clear()`. For updates, `importKeyMap` was seeded in pass 0 from `externalIdMap`.

### EF Core performance settings (per persist operation)

```csharp
await using var db = await factory.CreateDbContextAsync(ct);
await using var tx = await db.Database.BeginTransactionAsync(ct);

db.ChangeTracker.AutoDetectChangesEnabled = false;
db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll; // only for batched Adds

try
{
    // passes 1–4 ...
    await tx.CommitAsync(ct);
}
catch
{
    await tx.RollbackAsync(ct);
    throw;
}
```

### When batched EF is not enough (measure first)

| Escalation | When |
|------------|------|
| HiLo sequences | Profiling shows id-generation round-trips dominate; team accepts migration cost |
| `EFCore.BulkExtensions` | Need faster inserts without changing PK strategy |
| `SqlBulkCopy` / staging | Handlers cannot be suppressed; or SLA still missed after batching |

**Do not** add HiLo or bulk copy in v1 without documented profiling on a real XER file.

---

## Re-import (upsert) vs greenfield

| Scenario | Strategy |
|----------|----------|
| First import | Batched insert (passes 1–4); IDENTITY assigns ids per batch |
| Re-import | Preload `EntityExternalId`; match by `(system, value)`; **update** changed scalars on existing rows; **insert** only new keys; tombstone removed entities only if legacy does (confirm in Phase 0) |

The POC `ProjectImportIdentityResolver` (`reference_only`) shows external-id-first resolution — port the **idea**, not the per-row persist.

---

## Service layout (target module)

```text
Import/
├── Import.Domain/
│   └── ProjectStructure/           # ProjectStructureImportModel, ImportKey, ImportIssue
├── Import.Application/
│   ├── IProjectImportService.cs    # sole persist port — vendor-agnostic
│   ├── ProjectStructureImportValidator.cs
│   └── ImportPersistProgress.cs
├── Import.Infrastructure/
│   ├── Persistence/
│   │   └── EfProjectImportService.cs     # implements IProjectImportService (batched IDENTITY)
│   ├── Sources/
│   │   ├── Xer/
│   │   │   ├── StreamingXerParser.cs     # XER-only; outputs XerParseResult
│   │   │   └── XerProjectStructureAdapter.cs  # XerParseResult → ProjectStructureImportModel
│   │   ├── Json/
│   │   │   └── JsonProjectStructureAdapter.cs # nested API payload → flat model
│   │   └── Sciforma/                     # future; same pattern
│   └── Strangler/
│       └── LegacyXerImportAdapter.cs     # [StranglerAdapter] feature-flag cutover
├── Import.Api/
│   └── MapImportEndpoints.cs             # POST /import/structure (model) + /import/xer (file)
└── Import.Tests/
    ├── Fixtures/
    │   └── minimal-project-structure.xer   # embedded resource — see below
    ├── Infrastructure/
    │   └── EmbeddedXerFixture.cs           # loads embedded .xer streams
    ├── Integration/
    │   └── XerProjectImportEndToEndTests.cs  # REQUIRED — full pipeline + DB
    ├── ProjectStructure/
    ├── Persistence/ProjectImportServiceTests.cs
    ├── Sources/Xer/
    └── Characterization/LegacyXerImportBaselineTests.cs
```

Integration packs (PLM, SAP, …) ship **`IProjectStructureSourceAdapter`** implementations that reference **`Import.Domain` only** — not `Import.Infrastructure` or EF.

### Data flow

```text
┌─────────────┐   ┌──────────────────────┐   ┌─────────────────────────┐   ┌──────────┐
│ XER file    │──▶│ XerProjectStructure  │──▶│ ProjectStructureImport  │──▶│ Project  │
│ Excel / PLM │──▶│ Adapter (per source) │──▶│ Model                   │──▶│ Import   │
│ JSON API    │──▶│                      │──▶│ (vendor-neutral)        │──▶│ Service  │
└─────────────┘   └──────────────────────┘   └─────────────────────────┘   └──────────┘
     parse              map only                    validate                    EF only
```

### Actor orchestration (optional, recommended)

Per `platform-actor-standard.md`:

```text
POST /api/import/xer
  → ImportManagerActor
    → XerParseActor (CPU-bound, no EF)
    → XerMapActor → ProjectStructureImportModel
    → ProjectImportDataActor (sole DbContext — calls IProjectImportService)
  → ImportPersisted event

POST /api/import/structure          # same persist path for JSON / pack submissions
  → ImportManagerActor
    → ProjectImportDataActor
```

Hangfire `ImportXerJob` / `ImportSciformaJob` become thin triggers: **adapter → `IProjectImportService`**.

---

## Embedded golden XER integration test (required)

Claude **must** implement an end-to-end integration test that loads a real XER file from an **embedded resource**, runs parse → adapter → `IProjectImportService`, and asserts the database state. This is the primary proof that the pipeline works — not optional.

### Reference fixture (copy into monolith)

SandBox ships a minimal golden file for structure and expected counts:

| File | Contents |
|------|----------|
| `docs/monolith-modularization/fixtures/minimal-project-structure.xer` | 1 project, 3 WBS nodes, 5 tasks, 2 predecessors, 1 assignment |

Copy (or trim from a legacy `TestData` XER) into `Import.Tests/Fixtures/minimal-project-structure.xer` in the **external monolith**.

**Expected counts after import:**

| Entity | Count | Sample external id (`Primavera`) |
|--------|-------|----------------------------------|
| Project | 1 | project keyed via `proj_id` / name `MV-ALPHA` |
| Component | 3 | `1000`, `1001`, `1002` |
| Activity | 5 | `2000` … `2004` |
| ActivityRelation | 2 | `2000→2001` (FS), `2002→2004` (FS) |
| Assignment | 1 | task `2000` → resource `4000` |

Adjust assertions if legacy maps `proj_id` differently — cite Phase 0 evidence or align adapter to fixture.

### Embed the file in the test project

```xml
<!-- Import.Tests/Import.Tests.csproj -->
<ItemGroup>
  <EmbeddedResource Include="Fixtures\minimal-project-structure.xer" />
</ItemGroup>
```

Use logical name `Fixtures.minimal-project-structure.xer` (default) or set explicitly:

```xml
<EmbeddedResource Include="Fixtures\minimal-project-structure.xer">
  <LogicalName>Import.Tests.Fixtures.minimal-project-structure.xer</LogicalName>
</EmbeddedResource>
```

### Fixture loader helper

```csharp
namespace Import.Tests.Infrastructure;

internal static class EmbeddedXerFixture
{
    public static Stream OpenMinimalProjectStructure() =>
        typeof(EmbeddedXerFixture).Assembly
            .GetManifestResourceStream("Import.Tests.Fixtures.minimal-project-structure.xer")
        ?? throw new InvalidOperationException(
            "Embedded XER not found. Add Fixtures/minimal-project-structure.xer as EmbeddedResource.");

    public static async Task<Stream> OpenMinimalProjectStructureAsync(CancellationToken ct = default)
    {
        var stream = OpenMinimalProjectStructure();
        var memory = new MemoryStream();
        await stream.CopyToAsync(memory, ct);
        memory.Position = 0;
        await stream.DisposeAsync();
        return memory;
    }
}
```

List manifest names in a one-time diagnostic if the stream is null:

```csharp
[assembly: CollectionBehavior(DisableTestParallelization = true)] // optional for shared DB
```

### Required integration test

Place in `Import.Tests/Integration/XerProjectImportEndToEndTests.cs`. Use **real SQL Server** (Testcontainers or shared test DB pattern from existing monolith tests — match nearest project; cite `reference_only` from `ApiImportActorPoc` SQL test infra if needed).

```csharp
public sealed class XerProjectImportEndToEndTests : IAsyncLifetime
{
    private ImportTestDatabase _db = null!;
    private IProjectImportService _importService = null!;
    private XerProjectStructureAdapter _adapter = null!;

    public async ValueTask InitializeAsync()
    {
        _db = await ImportTestDatabase.CreateAsync();
        _importService = _db.CreateProjectImportService();
        _adapter = new XerProjectStructureAdapter(new StreamingXerParser());
    }

    [Fact]
    public async Task Import_minimal_embedded_xer_persists_project_tree_and_relations()
    {
        await using var xer = await EmbeddedXerFixture.OpenMinimalProjectStructureAsync();

        var validation = await _adapter.MapAsync(xer);
        Assert.True(validation.IsValid, string.Join("; ", validation.Issues.Select(i => i.Message)));

        var result = await _importService.PersistAsync(validation.Model!);

        await using var verify = await _db.CreateDbContextAsync();

        var project = await verify.Projects.SingleAsync();
        Assert.Equal("MV-ALPHA", project.Name);

        Assert.Equal(3, await verify.Components.CountAsync());
        Assert.Equal(5, await verify.Activities.CountAsync());
        Assert.Equal(2, await verify.ActivityRelations.CountAsync());
        Assert.Equal(1, await verify.Assignments.CountAsync());

        // Hierarchy: hull and outfit under root
        var root = await verify.Components.SingleAsync(c => c.Name == "wbs-root");
        var hull = await verify.Components.SingleAsync(c => c.Name == "wbs-hull");
        Assert.Equal(root.Id, hull.ParentComponentId);

        // Activity under correct component
        var erection = await verify.Activities.SingleAsync(a => a.Name == "Hull Block Erection");
        Assert.Equal(hull.Id, erection.ComponentId);

        // Finish-to-start: erection → welding
        var welding = await verify.Activities.SingleAsync(a => a.Name == "Hull Welding");
        Assert.True(await verify.ActivityRelations.AnyAsync(r =>
            r.SourceActivityId == erection.Id && r.TargetActivityId == welding.Id));

        // External id round-trip
        var primaveraActivityId = await verify.EntityExternalIds
            .Where(e => e.System == "Primavera" && e.Value == "2000")
            .Select(e => e.InternalEntityId)
            .SingleAsync();
        Assert.Equal(erection.Id, primaveraActivityId);
    }

    [Fact]
    public async Task Reimport_same_embedded_xer_updates_without_duplicates()
    {
        await using var xer1 = await EmbeddedXerFixture.OpenMinimalProjectStructureAsync();
        var model1 = (await _adapter.MapAsync(xer1)).Model!;
        var first = await _importService.PersistAsync(model1);

        await using var xer2 = await EmbeddedXerFixture.OpenMinimalProjectStructureAsync();
        var model2 = (await _adapter.MapAsync(xer2)).Model!;
        var second = await _importService.PersistAsync(model2);

        Assert.Equal(first.ProjectId, second.ProjectId);

        await using var verify = await _db.CreateDbContextAsync();
        Assert.Equal(5, await verify.Activities.CountAsync());
        Assert.Equal(5, await verify.EntityExternalIds.CountAsync(e => e.EntityKind == ImportEntityKind.Activity));
    }
}
```

### What the integration test must assert

| Assertion | Why |
|-----------|-----|
| Row counts match fixture | Parser + adapter completeness |
| `ParentComponentId` / `ComponentId` correct | Tree FK integrity |
| Activity relation source/target ids exist | Deferred relation pass |
| `EntityExternalId` rows for `Primavera` | Upsert / re-import contract |
| Re-import does not duplicate rows | Idempotency |
| No orphan rows after failed import (separate test) | Transaction boundaries |

### CI vs nightly

| Test | Fixture | When |
|------|---------|------|
| `XerProjectImportEndToEndTests` | `minimal-project-structure.xer` (embedded) | **Every CI build** |
| `XerImport_LargeFile_*` | Full production-scale XER (not embedded; file share or `[Explicit]`) | Nightly / manual |

Keep the embedded fixture **small** (< 50 KB) so CI stays fast. Scale tests use a separate file path.

---

## Phased workflow for Claude

### Phase 0 — Forensic (external repo)

Execute searches and document with `file:line`:

```text
ImportXerJob
ImportXer
XerParser / XerReader / XerFile
PROJWBS | TASK | TASKPRED
Aspose
SaveChanges
IChangeHandler | ChangeHandler
Floor2PlanDbContext
Handler.*Activity | Handler.*Component
EntityExternalId | SourceSystem
```

Deliverable: `docs/modularization/integrations/primavera-xer-import-phase0.md` containing:

| Section | Content |
|---------|---------|
| Legacy flow | Numbered steps from file upload → parse → EF → side effects |
| Pain points | Cite each slow/fragile pattern |
| Entity mapping | XER table.column → EF entity.property |
| Golden fixture | Path to test XER or production sample (sanitized) |
| Baseline metrics | Component count, activity count, duration, failure modes |
| **Change-handler audit** | Handler count per `SaveChanges`; which fire on Activity/Component INSERT; link to `primavera-xer-import-handler-audit.md` |
| Tests | Existing tests or `none found` |

### Phase 1 — Intermediate model + persist service

- Define `ProjectStructureImportModel` and `IProjectImportService` in Domain/Application.
- Implement `ProjectStructureImportValidator` with unit tests (no DB).
- Implement **`ImportExecutionScope`** (or equivalent) to suppress change handlers during bulk persist.
- Implement `EfProjectImportService` with batched multi-pass insert/update (existing IDENTITY — no HiLo).
- Integration test: persist a hand-built `ProjectStructureImportModel`; assert correct FKs and re-import idempotency.

### Phase 1b — Handler audit + post-import event (parallel or immediately after Phase 0)

- Complete `primavera-xer-import-handler-audit.md`.
- Implement `ProjectStructureImported` event + post-import actor for deferred effects (planning rollup, etc.).
- Add handler suppression / spy tests.

### Phase 2 — XER adapter + embedded integration test

- Implement `StreamingXerParser` (XER-only; outputs `XerParseResult`).
- Implement `XerProjectStructureAdapter` → `ProjectStructureImportModel`.
- Copy `minimal-project-structure.xer` into `Import.Tests/Fixtures/` as **EmbeddedResource**.
- Implement `EmbeddedXerFixture` + **`XerProjectImportEndToEndTests`** (parse → adapter → persist → DB assertions). **Must pass in CI.**
- Wire `ImportXerJob` → parse → adapter → **`IProjectImportService`** (not direct EF).

### Phase 3 — Wire + strangler

- `[StranglerAdapter]` routes legacy job behind feature flag.
- Characterization test: XER path ≡ legacy import for golden file (counts + sampled hashes).
- Add `JsonProjectStructureAdapter` for API round-trip (optional; proves reuse).

### Phase 4 — Observability

- Structured logging per pass (batch number, rows/sec).
- Progress events for UI (SignalR / Hangfire progress) — do not log per row.
- Correlation id per import session (`platform-correlation`).

---

## Test requirements

| Test | Purpose |
|------|---------|
| **`XerProjectImportEndToEndTests.Import_minimal_embedded_xer_*`** | **Required** — embedded XER, full pipeline, DB assertions |
| **`XerProjectImportEndToEndTests.Reimport_same_embedded_xer_*`** | **Required** — idempotency on same fixture |
| `Import_bulk_scope_suppresses_change_handlers` | **Required** — handlers do not run during bulk persist |
| `Import_post_import_fires_recalc_once` | Deferred handler effects via explicit event |
| `ProjectStructureValidator_RejectsOrphanActivity` | Model validation without DB |
| `ProjectImportService_PersistsHandBuiltModel` | Persist service in isolation (no XER) |
| `JsonAdapter_FlattensNestedPayload_ToSameModelAsFlat` | API reuse path |
| `XerAdapter_MapsGoldenFile_MatchesLegacyCounts` | Adapter-only parity (optional if E2E covers it) |
| `XerImport_LargeFile_*` | Scale test — nightly, not embedded |
| `Import_Relations_MapsPredTypesCorrectly` | TASKPRED → `ActivityRelationImportPart` |
| `LegacyXerImport_Baseline` | Characterization — must pass before and after |

Golden XER fixture: embed `minimal-project-structure.xer` in `Import.Tests` (see SandBox `docs/monolith-modularization/fixtures/`). Large production XER stays on disk / test share only.

---

## Acceptance criteria

- [ ] `ProjectStructureImportModel` defined in Domain; no vendor types in Application persist layer.
- [ ] `IProjectImportService` is the **only** bulk EF entry point for project structure (XER, JSON, future Sciforma/PLM).
- [ ] XER adapter maps to intermediate model; persist service has zero Primavera-specific code.
- [ ] Parses real Primavera XER without loading entire file into DOM/XML.
- [ ] Re-import **updates** existing rows and **inserts** only new external ids; no duplicate activities.
- [ ] Greenfield import acceptable on reference hardware after handler suppression (document measured time; no fixed HiLo requirement).
- [ ] Inserts ~9k components + ~250k activities with valid `ParentComponentId`, `ComponentId`, relation FKs.
- [ ] **No HiLo / sequence migration in v1** unless profiling doc justifies it.
- [ ] Single transaction per batch; failed batch rolls back without corrupt graph.
- [ ] No `SaveChanges` inside loops over entities.
- [ ] **Change-handler audit complete**; bulk import runs with handlers suppressed or allow-listed; post-import event covers deferred effects.
- [ ] Legacy `ImportXerJob` delegable via feature flag.
- [ ] **`XerProjectImportEndToEndTests` passes** using `minimal-project-structure.xer` as embedded resource (counts, FKs, relations, external ids, re-import).
- [ ] All claims cite `path:line` or test names.

---

## Copy-paste prompt for Claude

```text
@workspace Build project structure import (intermediate model + XER adapter)

Read first:
- docs/modularization/primavera-xer-import-service-instructions.md (this file)
- docs/modularization/platform-actor-standard.md
- docs/modularization/module-composition-di.md
- Legacy: Src/Application/Application.Sync/ImportJobs/Xer/ImportXerJob.cs

Architecture (mandatory):
1. ProjectStructureImportModel in Import.Domain — vendor-neutral, flat lists, ImportKey linking
2. IProjectImportService — sole EF persist; batched SaveChanges on existing IDENTITY columns; reliable insert + update
3. XerProjectStructureAdapter — XER parse → intermediate model only
4. Future sources add adapters; reuse same persist service — expand usage once service is proven

Constraints:
- ~9,000 components, ~250,000 activities — NO per-row SaveChanges; NO HiLo in v1 unless profiling proves need
- CRITICAL: Floor2PlanDbContext change handlers (~28+ per SaveChanges observed) — inventory, suppress during bulk, defer effects to ProjectStructureImported
- Upsert by ExternalIds dictionary (e.g. "Primavera", "PLM") — not by name
- REQUIRED: XerProjectImportEndToEndTests with minimal-project-structure.xer as EmbeddedResource
- Read docs/modularization/change-handler-migration.md before touching handlers

Order: Phase 0 forensic + handler audit → intermediate model + ImportExecutionScope → IProjectImportService → post-import event → XER adapter + embedded E2E test → strangler.
Cite file:line for every legacy claim.
```

---

## Related documents

| Document | Role |
|----------|------|
| `external-integrations-deepdive-instructions.md` | Integration discovery methodology |
| `floor2plan-v2-connector-migration-prompt.md` | Pack layout for vendor connectors |
| `change-handler-migration.md` | Handler inventory template; migration to explicit events |
| `foundation-and-pilot-plan.md` | Import pilot scope; Aspose/XER explicitly deferred from slice 1 |
| `docs/Modularization/02-bounded-context-map.md` | Sync → Planning bulk write context |
