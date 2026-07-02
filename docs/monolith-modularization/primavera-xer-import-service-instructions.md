# Primavera XER project import service — build instructions

**Purpose:** Guide Claude (or any AI assistant) to design and implement a **fast, reliable** project import service that reads an Oracle Primavera **XER** file and persists ~9,000 components and ~250,000 activities (tree + relations) into EF Core with correct foreign keys.

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
| `docs/monolith-modularization/platform-actor-standard.md` | One persist actor; no `SaveChanges` orchestration |
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
4. **Identity column round-trips** — cannot assign FKs until each parent row exists.
5. **Single giant transaction** or **no transaction** — timeouts vs partial corrupt state.
6. **Fragile parsing** — assumes field order, missing null handling, encoding issues, duplicate external ids.
7. **SaveChanges / workflow handler side effects** during import (integration orchestration in wrong layer).

**Target:** Full greenfield import of a 250k-activity XER in **minutes, not hours**, with idempotent re-import and structured error reporting.

---

## Non-negotiable rules

1. **Behaviour preservation first** — run legacy import on a golden XER; capture row counts, timings, and sample entities before refactoring.
2. **Parse → canonical model → persist** — three layers; no EF entities in the parser.
3. **Bulk persist** — batched inserts; **never** `SaveChangesAsync` per row at this scale.
4. **HiLo (or equivalent) for greenfield inserts** — pre-assign integer PKs so FKs are set before `AddRange`.
5. **Deferred relations** — activity predecessors need both endpoints mapped; insert relations in a final pass (see POC `deferredRelations` pattern).
6. **One EF boundary** — single persist service/actor touches `DbContext` for the import workflow (`platform-actor-standard.md`).
7. **External ids** — every imported entity carries `Primavera:<task_id|wbs_id|…>`; upsert matches on external id, not name.
8. **No ABP in new modules** — `AddImportModule` / `MapImportEndpoints`; legacy bridged via `[StranglerAdapter]`.
9. **Cite evidence** — `path:line` for legacy claims; `[NEEDS REVIEW]` when uncertain.

---

## XER format (parser contract)

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

## Canonical intermediate model

Align with `ApiImportActorPoc` import payloads (`reference_only`):

```csharp
// Illustrative — adapt to monolith Import module contracts
public sealed record XerImportBatch(
    ProjectImportPayload Project,
    IReadOnlyList<ComponentImportRow> Components,      // flat, with ParentPrimaveraId
    IReadOnlyList<ActivityImportRow> Activities,       // flat, with ComponentPrimaveraId
    IReadOnlyList<AssignmentImportRow> Assignments,
    IReadOnlyList<ActivityRelationImportRow> Relations,
    IReadOnlyList<ParseIssue> Issues);
```

**Flat lists, not nested trees**, for bulk insert. Tree shape is recovered via `ParentPrimaveraId` / `ComponentPrimaveraId` foreign keys to Primavera ids, then mapped to internal ids.

External id namespace: **`Primavera`** (match `PrimaveraExcelReader` and POC).

---

## Persistence strategy — why HiLo

SQL Server `IDENTITY` requires insert-before-FK-assignment unless using `SET IDENTITY_INSERT` (slow, locks table) or **application-assigned keys**.

### HiLo configuration

Use **one HiLo sequence per entity type** with a **large increment** to minimize sequence round-trips:

```csharp
// In IEntityTypeConfiguration<T> — example
builder.Property(e => e.Id)
    .UseHiLo("component_hilo", schema: "Import")
    .HasValueGeneratorFactory<CustomHiLoGenerator>(); // optional: tune increment

// Or via modelBuilder in OnModelCreating:
modelBuilder.HasSequence<int>("activity_hilo", schema: "Import")
    .IncrementsBy(10_000); // tune: 1k–10k for 250k rows
```

| Entity | Suggested HiLo increment (starting point) |
|--------|-------------------------------------------|
| Component | 1,000 |
| Activity | 10,000 |
| Assignment | 5,000 |
| ActivityRelation | 10,000 |
| EntityExternalId | 10,000 |

**Greenfield import only** uses HiLo-assigned ids. **Re-import / upsert** resolves existing rows via `EntityExternalId` table first; only insert rows with no matching external id (do not re-HiLo existing rows).

### Migration note

Legacy tables may use `IDENTITY`. HiLo requires a migration that:

1. Adds SQL Server sequences (or EF HiLo tables).
2. Switches `UseIdentityColumn()` → `UseHiLo(...)` on import-target entities **or** uses a dedicated import staging schema.
3. Sets sequence start above `MAX(Id)` to avoid collisions.

Mark `[StranglerAdapter]` if legacy and new paths share tables during transition.

---

## Bulk insert algorithm (required)

Replace recursive `UpsertComponentsAsync` + per-row `SaveChanges` with a **multi-pass pipeline**:

```text
Pass 0 — Resolve project
  Upsert project row (single SaveChanges or merge statement)
  Build externalIdMap: PrimaveraId → InternalId (preload from DB for re-import)

Pass 1 — Components (topological order)
  Sort PROJWBS by depth (parents before children)
  Allocate HiLo ids for new components
  Set ParentComponentId from idMap[parent_wbs_id]
  AddRange(components) in batches of 2,000–5,000
  SaveChangesAsync per batch (ChangeTracker.Clear() between batches)
  Update idMap with new Primavera → internal ids
  Write EntityExternalId rows in same batch

Pass 2 — Activities
  For each activity: ComponentId = idMap[task.wbs_id]
  Allocate HiLo ids; AddRange in batches; SaveChanges; update idMap

Pass 3 — Assignments (if in scope)
  ActivityId = idMap[task_id]; batched AddRange

Pass 4 — Activity relations (deferred)
  SourceActivityId = idMap[task_id]
  TargetActivityId = idMap[pred_task_id]
  Map pred_type → FinishToStart / StartToStart / … (match legacy mapping — cite tests)
  AddRange in batches; single SaveChanges per batch

Pass 5 — External ids (if not written inline)
  Bulk insert remaining EntityExternalId rows
```

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

### When HiLo is not enough

If profiling shows EF insert still too slow after batching:

| Escalation | When |
|------------|------|
| `EFCore.BulkExtensions` / `BulkInsert` | EF-managed bulk with FK columns set |
| `SqlBulkCopy` via staging tables | >250k rows; merge into final tables in SQL |
| Table-valued parameter + stored proc | DBA-owned merge for re-import |

**Do not** jump to `SqlBulkCopy` without measuring EF batched HiLo first — it adds operational complexity.

---

## Re-import (upsert) vs greenfield

| Scenario | Strategy |
|----------|----------|
| First import | HiLo bulk insert (passes 1–4) |
| Re-import same XER | Load `EntityExternalId` for `System = 'Primavera'` into memory; match rows; **update** changed scalars; **insert** only new Primavera ids; **soft-delete or tombstone** removed entities only if legacy does (confirm in Phase 0) |

The POC `ProjectImportIdentityResolver` (`reference_only`) shows external-id-first resolution — port the **idea**, not the per-row persist.

---

## Service layout (target module)

```text
Import/
├── Import.Domain/              # XerImportBatch, ports, parse issues
├── Import.Application/
│   ├── IXerParser.cs
│   ├── IProjectImportPersistPort.cs
│   └── ProjectImportService.cs # orchestrates parse → persist
├── Import.Infrastructure/
│   ├── Xer/StreamingXerParser.cs
│   ├── Persistence/HiLoProjectImportWriter.cs
│   └── Strangler/LegacyXerImportAdapter.cs   # [StranglerAdapter] delegates to ImportXerJob during cutover
├── Import.Api/
│   └── MapImportEndpoints.cs
└── Import.Tests/
    ├── Golden/                  # real XER fixtures (sanitized)
    ├── XerParserTests.cs
    ├── HiLoBulkInsertTests.cs
    └── Characterization/LegacyXerImportBaselineTests.cs
```

### Actor orchestration (optional, recommended)

Per `platform-actor-standard.md`:

```text
POST /api/import/xer
  → ImportManagerActor
    → XerParseActor (CPU-bound, no EF)
    → ProjectImportDataActor (sole DbContext — calls HiLoProjectImportWriter)
  → ImportPersisted event
```

Hangfire `ImportXerJob` becomes a thin trigger that sends the same command during migration.

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
| Tests | Existing tests or `none found` |

### Phase 1 — Parser

- Implement `StreamingXerParser` with issue collection.
- Unit tests per table; golden file test asserting row counts match legacy parser.
- **No database** in parser tests.

### Phase 2 — HiLo migration + writer

- Add sequences / HiLo configuration.
- Implement `HiLoProjectImportWriter` with batched passes.
- Integration test: insert 10k activities in < N seconds (set N from local CI; e.g. < 30s).

### Phase 3 — Wire + strangler

- New `IProjectImportPersistPort` implementation.
- `[StranglerAdapter]` routes `ImportXerJob` to new writer behind feature flag.
- Characterization test: new import ≡ legacy import for golden XER (counts + sampled hashes).

### Phase 4 — Observability

- Structured logging per pass (batch number, rows/sec).
- Progress events for UI (SignalR / Hangfire progress) — do not log per row.
- Correlation id per import session (`platform-correlation`).

---

## Test requirements

| Test | Purpose |
|------|---------|
| `XerParser_ParsesGoldenFile_MatchesLegacyCounts` | Parser parity |
| `HiLoWriter_Inserts250kActivities_AllForeignKeysValid` | Scale test (use reduced fixture in CI; full file nightly) |
| `Reimport_SameXer_UpdatesByPrimaveraExternalId` | Idempotency |
| `Import_Relations_MapsPredTypesCorrectly` | TASKPRED → ActivityRelation |
| `Import_InvalidWbsReference_CollectsParseIssue` | Fail gracefully, no partial orphan rows |
| `LegacyXerImport_Baseline` | Characterization — must pass before and after |

Store golden XER under `Import.Tests/Golden/` (or legacy `TestData/` path — cite which).

---

## Acceptance criteria

- [ ] Parses real Primavera XER without loading entire file into DOM/XML.
- [ ] Inserts ~9k components + ~250k activities with valid `ParentComponentId`, `ComponentId`, relation FKs.
- [ ] Greenfield import completes in **< 10 minutes** on reference hardware (document specs) — stretch goal **< 3 minutes** with batching + HiLo.
- [ ] Re-import updates existing rows by `Primavera` external id; no duplicate activities.
- [ ] Single transaction per batch; failed batch rolls back without corrupt graph.
- [ ] No `SaveChanges` inside loops over entities.
- [ ] Legacy `ImportXerJob` delegable via feature flag.
- [ ] All claims cite `path:line` or test names.

---

## Copy-paste prompt for Claude

```text
@workspace Build a fast Primavera XER project import service

Read first:
- docs/modularization/primavera-xer-import-service-instructions.md (this file)
- docs/modularization/platform-actor-standard.md
- docs/modularization/module-composition-di.md
- Legacy: Src/Application/Application.Sync/ImportJobs/Xer/ImportXerJob.cs

Constraints:
- ~9,000 components, ~250,000 activities — batched HiLo bulk insert, NO per-row SaveChanges
- Parse XER streaming (%T/%F/%R); flat canonical model; deferred activity relations
- Upsert by external id namespace "Primavera"
- One persist boundary (service or ProjectImportDataActor)
- Characterization tests against golden XER before replacing legacy job

Start with Phase 0 forensic doc, then parser tests, then HiLoProjectImportWriter.
Cite file:line for every legacy claim.
```

---

## Related documents

| Document | Role |
|----------|------|
| `external-integrations-deepdive-instructions.md` | Integration discovery methodology |
| `floor2plan-v2-connector-migration-prompt.md` | Pack layout for vendor connectors |
| `foundation-and-pilot-plan.md` | Import pilot scope; Aspose/XER explicitly deferred from slice 1 |
| `docs/Modularization/02-bounded-context-map.md` | Sync → Planning bulk write context |
