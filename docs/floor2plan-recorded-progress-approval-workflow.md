# Floor2Plan — recorded progress approval workflow

**Status:** Design + SandBox POC (`RecordedProgressApprovalsPoc/`)  
**Audience:** Planning / MES / Timekeeping engineers modularizing Floor2Plan  
**Related:** `docs/floor2plan-planning-approval-data-model.md` (foreman plan adjustment), `docs/Modularization/02-bounded-context-map.md` (Planning, Timekeeping, Sync)

---

## 1. Problem

Shop-floor **recorded progress** (percent complete, booked hours, manual progress per assignment) must pass through a **multi-stage approval chain** before it is eligible for **export to an external ERP** as actuals / hourlines.

The system must:

1. Track each progress revision through configurable approval stages (check → review → final approve).
2. Keep a full **append-only audit** of who advanced or rejected each stage.
3. Block ERP export until all required stages are satisfied.
4. Keep **`Assignment` / `AssignmentProgressHistory` aggregates clean** — approval workflow lives in a separate bounded context.
5. Allow tenant-specific stage pipelines via **customization rules packs** without `if (tenant)` in core.

---

## 2. Bounded context: `RecordedProgressApprovals`

| Context | Owns | Does not own |
|---------|------|----------------|
| **Planning** | `Assignment`, `AssignmentProgressHistory`, recalculation | Approval stages, export eligibility |
| **RecordedProgressApprovals** | Submissions, stage decisions, export readiness | Progress % calculation, CPM |
| **Timekeeping** | `TimeSheet`, `ErpHourline`, balance side-effects | Stage definitions |
| **Sync / Integration** | Vendor protocol, file formats, retry | Business approval rules |

```text
Shop floor / timesheet / manual progress
        │
        ▼
Planning: AssignmentProgressHistory (immutable revision)
        │
        ├──► RecordedProgressRevisionCaptured (integration event)
        │
        ▼
RecordedProgressApprovals: open submission, advance stages
        │
        ├──► RecordedProgressFullyApproved (integration event)
        │
        ▼
Sync: ErpExportActor pipeline → external ERP
```

Integration uses **stable IDs** (`AssignmentId`, `ProgressRevisionId`, `SubmissionId`) — no EF navigation from `AssignmentProgressHistory` to approval tables.

---

## 3. Ubiquitous language

| Term | Meaning |
|------|---------|
| **Recorded progress** | Immutable progress point for one assignment (`AssignmentProgressHistory`) |
| **Progress revision** | Same as recorded progress; identified by `RevisionId` |
| **Submission** | Workflow instance for one progress revision entering the approval chain |
| **Stage** | Named checkpoint in the pipeline (e.g. foreman check, planning review) |
| **Stage decision** | Append-only grant/reject at a stage by an authorised person |
| **Approval pipeline** | Ordered list of stages required before ERP export (tenant-configurable) |
| **Export eligibility** | All required stages approved; submission not rejected |
| **ERP export batch** | Outbound integration unit sent after eligibility (idempotent) |

---

## 4. Default approval pipeline

Core ships a **default pipeline**. Tenants may replace or extend via customization rules pack.

| Order | Stage | Typical actor | Permission |
|-------|-------|---------------|------------|
| 0 | `Recorded` | System (on capture) | — |
| 1 | `Submitted` | Worker / foreman | `Progress.Submit` |
| 2 | `ForemanChecked` | Team lead | `Progress.Check.Foreman` |
| 3 | `PlanningReviewed` | Planner | `Progress.Review.Planning` |
| 4 | `Approved` | Project controller / PM | `Progress.Approve.Final` |
| 5 | `Exported` | Integration actor | system |
| — | `Rejected` | Any authorised stage actor | blocks export |
| — | `ExportFailed` | Integration actor | retry / manual fix |

```text
Recorded → Submitted → ForemanChecked → PlanningReviewed → Approved → Exported
                │              │                 │              │
                └──────────────┴─────────────────┴──────────────┴──► Rejected (terminal)
```

**ERP export gate:** submission `CurrentStage == Approved` and every pipeline stage except `Recorded` / `Exported` has an `Approved` decision.

---

## 5. Aggregates

### 5.1 `RecordedProgressSubmission` (root — one open submission per progress revision)

```
RecordedProgressSubmission
├── PublicId, AssignmentId, ProgressRevisionId
├── RecordedValues (percent, booked hours, recorded at, source)
├── CurrentStage
├── PipelineFingerprint (hash of ordered stage list — detects pack changes)
├── OpenedAt, ClosedAt?
└── Decisions[] (append-only StageDecision)
```

**Invariant:** at most one **open** submission per `(AssignmentId, ProgressRevisionId)`.

### 5.2 `StageDecision` (append-only, owned by submission)

```
StageDecision
├── Stage
├── Outcome: Approved | Rejected
├── DecidedBy (PersonId)
├── DecidedAtUtc
└── Comment?
```

Never updated or deleted.

### 5.3 `ErpExportAttempt` (optional — outbound audit)

```
ErpExportAttempt
├── SubmissionId
├── AttemptedAtUtc
├── Outcome: Succeeded | Failed
├── ExternalReference?
└── ErrorSummary?
```

Written by integration pack / `ErpExportActor` only after eligibility passes.

---

## 6. Value objects

### `ProgressRevisionRef`

```csharp
record ProgressRevisionRef(
    long AssignmentId,
    long RevisionId,
    DateTimeOffset RecordedAt,
    decimal PercentComplete,
    decimal BookedHours,
    string Source);
```

### `RecordedProgressValues`

Snapshot copied from Planning at submission open time — used for audit even if Planning recalculates later.

---

## 7. Stage transition rules

Implemented in `RecordedProgressApprovalRules` (domain) + `RecordedProgressApprovalCoordinator` (application service).

1. **Open submission** when `RecordedProgressRevisionCaptured` arrives and no open submission exists for that revision.
2. **Advance** only to the **next** stage in the tenant pipeline (no skipping unless pack registers override).
3. **Reject** at any check/review/approve stage → `CurrentStage = Rejected`; close submission.
4. **Re-record progress** on same assignment → supersede open submission (status `Superseded`) and open new submission for new revision.
5. **Export** only when `IsEligibleForErpExport(submission, pipeline)` is true.

Idempotency key for open: `(AssignmentId, ProgressRevisionId)`.

---

## 8. Actor pipeline (ERP export)

After `RecordedProgressFullyApproved` integration event:

```text
RecordedProgressFullyApproved
    → IntegrationRouterActor (tenant profile)
        → [EligibilityGuardActor]           ← core / rules pack
        → [MapToErpActualsActor]            ← integration pack (SAP, file, …)
        → [ErpExportActor]                  ← HTTP/file outbound
        → PersistActor (ErpExportAttempt)
        → RecordedProgressExported | RecordedProgressExportFailed
```

See `docs/monolith-modularization/platform-actor-standard.md` — **one EF boundary** per export workflow; correlation ID from HTTP/event through actors.

Legacy mapping:

| Legacy | V2 |
|--------|-----|
| `CreateErpHourlinesProcessor` after manual approval flags | `ErpExportActor` after stage eligibility |
| `Bool3` gates export | Rules pack `TenantRulesActor` stage before outbound |
| Hangfire export job | Supervised actor pipeline; Hangfire optional as scheduler |

---

## 9. Persistence (`recorded_progress_approvals` schema)

| Table | Notes |
|-------|-------|
| `recorded_progress_submissions` | Index `(assignment_id, progress_revision_id)` unique open; `(project_id, current_stage, opened_at)` |
| `stage_decisions` | Append-only; index `(submission_id, stage)` |
| `erp_export_attempts` | Index `(submission_id, attempted_at DESC)` |

Separate **`RecordedProgressApprovalsDbContext`** — not `Floor2PlanDbContext`.

---

## 10. CQRS read model (approval queue)

Denormalized `RecordedProgressQueueItem`:

- `SubmissionId`, `AssignmentId`, `ActivityId`, `ProjectId`, `CurrentStage`, `RecordedAt`, `PercentComplete`, `IsEligibleForExport`, `ForemanScope`
- Paginated list per stage — do not join Planning tables in domain layer

---

## 11. Authorization

| Persona | Permission |
|---------|------------|
| Shop-floor worker | `Progress.Submit` |
| Foreman | `Progress.Check.Foreman` |
| Planner | `Progress.Review.Planning` |
| Project controller | `Progress.Approve.Final` |
| Integration service account | system (export actors) |
| Viewer | read-only queue |

Extend `docs/monolith-modularization/platform-authentication-standard.md` operational roles.

---

## 12. SandBox POC

Runnable reference: [`RecordedProgressApprovalsPoc/`](../RecordedProgressApprovalsPoc/)

```bash
cd RecordedProgressApprovalsPoc
dotnet run --project tests/RecordedProgressApprovals.Tests
```

Key types:

| POC | Path |
|-----|------|
| Stage rules | `RecordedProgressApprovals.Domain/Rules/RecordedProgressApprovalRules.cs` |
| Coordinator | `RecordedProgressApprovals.Domain/Services/RecordedProgressApprovalCoordinator.cs` |
| Submission aggregate | `RecordedProgressApprovals.Domain/Models/RecordedProgressSubmission.cs` |
| Scenarios | `RecordedProgressApprovals.Tests/Support/RecordedProgressScenario.cs` |

**POC / reference only** — not production Floor2Plan behaviour.

---

## 13. Relationship to other approval flows

| Flow | Scope | Doc |
|------|-------|-----|
| **Recorded progress approval** (this doc) | Progress revision → multi-stage → ERP export | here |
| **Planning adjustment approval** | Foreman signs off recalculated plan vs lookback | `floor2plan-planning-approval-data-model.md` |
| **Hour approvals (V2)** | Foreman approves task hours/dates/user on floorboard | `F2pPlatform/src/Modules/HourApprovals/MODULE.md` |

These are **separate bounded contexts**. Do not merge approval flags into `Assignment` or `TimeSheet` aggregates.

---

## 14. Open points [NEEDS REVIEW]

1. **Parallel vs serial stages** — can planning review start before foreman check completes?
2. **Auto-advance** — skip `Submitted` when foreman records progress directly?
3. **Partial export** — export approved revisions while newer unapproved revisions exist on same assignment?
4. **ERP feedback loop** — EP-111 timesheet status webhook → reopen submission on ERP reject?
5. **Stage pack versioning** — behaviour when pipeline changes mid-flight?

---

## 15. Monolith adoption path

1. Extract `RecordedProgressApprovals` module with `AddRecordedProgressApprovalsModule` + `MapRecordedProgressApprovalsEndpoints`.
2. Publish `RecordedProgressRevisionCaptured` from Planning on `AssignmentProgressHistory` insert.
3. Subscribe → open submission; expose stage queue APIs per role.
4. On final approval publish `RecordedProgressFullyApproved` → wire `ErpExportActor` in Sync integration pack.
5. Retire `Bool*` export gates and SaveChanges-driven `CreateErpHourlinesProcessor` triggers where pack pipeline replaces them.

See `docs/monolith-modularization/module-composition-di.md`.

**Legacy analysis:** `docs/monolith-modularization/recorded-progress-approval-deepdive-instructions.md`.
