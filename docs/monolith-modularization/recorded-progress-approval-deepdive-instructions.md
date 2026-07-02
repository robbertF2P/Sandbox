# Recorded progress approval workflow — deep-dive instructions

**Purpose:** Guide AI coding assistants through systematic analysis of legacy **recorded progress** capture, **multi-stage approval**, and **ERP export** behaviour in Floor2Plan — and produce a **judgment + V2 migration proposal** aligned with Platform 2.0 modules, actor pipelines, and customization packs.

**Audience:** Engineers and domain experts during strangler migration (post–Phase 0 inventory).

**Prerequisite:** Phase 0 inventory (`docs/modularization/00-inventory.md`). Bounded context map (`02-bounded-context-map.md`). Target design: `docs/floor2plan-recorded-progress-approval-workflow.md`.

**Outcome:** Per-tenant stage catalog, approval-chain traceability, export gate inventory, and V2 target design (core pipeline vs rules pack vs integration pack) with characterization test gaps.

---

## Important: analyze the external application repository

Run all phases against the **external Floor2Plan monolith repository** — **not** this SandBox workspace.

SandBox holds the target design doc, `RecordedProgressApprovalsPoc/`, and V2 standards. Legacy `AssignmentProgressHistory`, `TimeSheetStatus`, `ErpHourline`, Elsa workflows, and `Bool*` export gates live in the external repo.

```yaml
analysis_target:
  repo: "<path or url to external F2P monolith>"
  workspace_note: "Open the external repo as the workspace root before running any analysis phase."
  sandbox_repo_role: "Copy finished artifacts back here under docs/modularization/recorded-progress-approval/ if desired."
```

If the external repo is unavailable, stop and list what is missing. Label SandBox POC references `reference_only: true`.

---

## How to use this document

1. Open the **external monolith repository** as the workspace root.
2. Run **one phase at a time**. Do not skip Phase A (discovery) or Phase C (tenant stage matrix).
3. Store outputs under `docs/modularization/recorded-progress-approval/` in the **external repo**.
4. Human review required after Phases B, **C**, and F before changing domain models or shipping V2 modules.
5. Cross-reference `docs/modularization/analysis-instructions.md` for entry-point IDs (EP-###).
6. Before implementation PRs, satisfy `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md`.

### V2 target references (SandBox — read before Phase F)

| Document | Use for |
|----------|---------|
| `floor2plan-recorded-progress-approval-workflow.md` | Bounded context, stages, aggregates, ERP actor pipeline |
| `platform-actor-standard.md` | `ErpExportActor`, pack stages, correlation |
| `platform-pack-blueprint.md` | Rules pack for stage pipelines; integration pack for ERP |
| `tenant-workflow-fields-deepdive-instructions.md` | `Bool*` export gates, Text* display fields |
| `external-integrations-deepdive-instructions.md` | ERP connectors, hourline export |

---

## Required inputs (fill before Phase A)

```yaml
program:
  name: "F2P Platform 2.0 — Recorded Progress Approval Deep-Dive"
  monolith_repo: "<url or local path>"
  product_name: Floor2Plan

seed_entry_points:                 # validate and extend
  progress_capture:
    - EP-604   # Floorboard progress
    - EP-602   # Planboard assignment update
    - EP-310   # ImportHoursAndProgress
  approval:
    - EP-606   # WeeklyTimesheet Submit
    - EP-609   # EmployeeTimesheet approvals
    - EP-111   # External approval feedback
  export:
    - EP-705   # ErpWeeklyTimesheet check
    - EP-5208  # CreateErpHourlinesProcessor
    - EP-5201  # Assign hour types to ERP hourlines

seed_entities:
  - AssignmentProgressHistory
  - TimeSheet / TimeSheetStatus
  - ErpHourline
  - PersonTimesheetApprover
  - ManualHoursAndProgress
```

---

## Phase A — Discovery sweep

**Goal:** Find every code path that records progress, gates approval, or triggers ERP export.

### Search patterns

```text
AssignmentProgressHistory
ManualHoursAndProgress
CreateErpHourlines
ErpHourline
TimeSheetStatus
Submit*Approval
Export*Hour
ImportHoursAndProgress
Bool* + export|approv
ProgressRevision
BookHours
```

### Deliverable: `phase-a-discovery.md`

| Column | Content |
|--------|---------|
| EP-### | Entry point id |
| Path | File + method |
| Trigger | UI / job / event / processor |
| Writes | Tables / entities |
| Reads approval? | Y/N + how |
| Triggers export? | Y/N + how |

---

## Phase B — Map to bounded contexts

**Goal:** Attach each discovery item to Planning, Timekeeping, Sync, or Workflow.

### Deliverable: `phase-b-context-map.md`

- Progress **capture** → Planning (`AssignmentProgressHistory`)
- Manager **timesheet approval** → Timekeeping (`TimeSheetStatus`) — note overlap with progress
- **ERP hourlines** → Timekeeping (`ErpHourline`) + Sync processors
- Elsa / `Bool*` gates → classify Workflow vs tenant extension field

Flag **hidden coupling**: SaveChanges handlers that create `ErpHourline` on progress insert without explicit approval state.

---

## Phase C — Tenant stage matrix (expert workshop)

**Goal:** Document how many approval stages each tenant uses and who acts at each.

### Deliverable: `phase-c-tenant-stages.yaml`

```yaml
tenants:
  - slug: example-shipyard
    stages:
      - id: foreman_check
        legacy_signal: "TimeSheetStatus = Submitted → ForemanApproved"
        actor_role: foreman
      - id: planning_review
        legacy_signal: "[NEEDS REVIEW]"
        actor_role: planner
      - id: erp_export
        legacy_signal: "CreateErpHourlinesProcessor when Bool3"
        blocked_until: [foreman_check, planning_review]
```

Interview prompts:

1. How many human checks between progress entry and ERP export?
2. Can stages be skipped for certain project types or hour types?
3. What happens on reject — rework UI, correction timesheet, or silent hold?
4. Does ERP send feedback that reopens approval (EP-111)?

---

## Phase D — Export gate inventory

**Goal:** List every condition that must be true before ERP export runs.

### Deliverable: `phase-d-export-gates.md`

| Gate | Location | Tenant-specific? | V2 placement |
|------|----------|------------------|--------------|
| All timesheet lines approved | | | Timekeeping / RecordedProgressApprovals |
| Bool3 on activity | | | Rules pack stage |
| Balance policy satisfied | | | Timekeeping domain |
| … | | | |

---

## Phase E — Story map from integration tests

**Goal:** Reverse-engineer user stories from tests touching progress + ERP.

### Deliverable: `phase-e-story-map.md`

Use `docs/monolith-modularization/templates/integration-story-map.schema.yaml` if available.

Minimum stories:

- US-001 Worker records progress on assignment
- US-002 Foreman checks recorded progress
- US-003 … (per tenant stages)
- US-00N Approved progress exported to ERP
- US-00N+1 ERP rejection feeds back into platform

---

## Phase F — V2 target design

**Goal:** Produce migration proposal per tenant and default core pipeline.

### Deliverable: `phase-f-v2-proposal.md`

1. **Core module:** `RecordedProgressApprovals` with default pipeline (see design doc §4).
2. **Rules pack:** tenant stage order, skip rules, `Bool*` replacements.
3. **Integration pack:** ERP map + `ErpExportActor` per vendor.
4. **Strangler:** delegate to `CreateErpHourlinesProcessor` until parity tests pass.
5. **Test gaps:** characterization tests for each stage transition and export idempotency.

### Placement decision tree

```text
Behaviour needed?
├─ Universal stage machine (open → advance → reject) → Core module
├─ Tenant stage count / skip rules → Customization rules pack
├─ Vendor ERP protocol → Integration pack
└─ Legacy-only during migration → [StranglerAdapter] + removal ticket
```

---

## Phase G — Quality gate checklist

Before any V2 implementation PR:

- [ ] Every stage in tenant matrix mapped to permission + API endpoint
- [ ] Export idempotency key documented (`SubmissionId` + `ProgressRevisionId`)
- [ ] No `IQueryable` leakage; specifications for queue filters
- [ ] Correlation ID through approval UI → event → `ErpExportActor`
- [ ] Characterization tests for default pipeline in SandBox module tests
- [ ] Expert sign-off on Phase C matrix

---

## Relationship to related flows

| Analysis | When to run |
|----------|-------------|
| `floor2plan-planning-approval-data-model.md` | Foreman approves **plan recalculation**, not ERP export |
| `tenant-workflow-fields-deepdive-instructions.md` | `Bool*` / `Text*` on activities for export gates |
| `external-integrations-deepdive-instructions.md` | Full ERP connector inventory |

Do not conflate **plan adjustment approval** with **recorded progress approval** — separate bounded contexts in V2.
