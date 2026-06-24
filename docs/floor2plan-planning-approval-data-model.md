# Floor2Plan — foreman planning adjustment approvals

**Status:** Design + SandBox POC (`PlanningApprovalsPoc/`)  
**Audience:** Planning / MES engineers modularizing Floor2Plan  
**Related:** `docs/Modularization/02-bounded-context-map.md` (Planning context), `docs/monolith-modularization/platform-authentication-standard.md` (roles)

---

## 1. Problem

Shipyard foremen must **approve adjusted planning** after **progress is reported per assignment** (timesheet, manual progress). The system must:

1. Track approval state and full audit history.
2. Detect when **new progress** or **recalculated plans** invalidate prior approvals.
3. Scale to **hundreds of assignments** per review.
4. Keep **`Assignment` / `Activity` aggregates clean** — approvals live elsewhere.

---

## 2. Bounded context: `PlanningApprovals`

| Context | Owns | Does not own |
|---------|------|----------------|
| **Planning** | `Assignment`, schedule, `AssignmentProgressHistory`, recalculation | Approval flags, foreman decisions |
| **PlanningApprovals** | Requests, decisions, approved snapshots, staleness | Progress %, hours booked, CPM |
| **Timesheet** | `TimesheetEntry`, `ManualHoursAndProgress` | Plan approval |

```text
Timesheet / ManualHoursAndProgress
        │
        ▼
Planning: AssignmentSummary + AssignmentProgressHistory
        │
        ├──► AdjustedPlanProposed (integration event)
        │
        ▼
PlanningApprovals: open / supersede requests, record decisions
```

Integration uses **stable IDs** (`AssignmentId`, `ProgressRevisionId`) and integration events — no EF navigation from `Assignment` to approval tables.

---

## 3. Ubiquitous language

| Term | Meaning |
|------|---------|
| **Progress revision** | Immutable progress point for one assignment (`AssignmentProgressHistory`) |
| **Proposed plan** | Planning output after recalculation — fields the foreman signs off |
| **Approval request** | Assignment needs foreman review |
| **Approval decision** | Append-only grant/reject |
| **Approved plan snapshot** | Frozen progress + plan at approval time |
| **Lookback baseline** | Progress + plan from ~1 week ago — foreman comparison starting point |
| **Planning checkpoint** | Append-only capture of assignment state used to resolve lookback |
| **Stale approval** | Current state differs from lookback baseline and is not yet foreman-approved |

---

## 4. Aggregates

### 4.1 `AssignmentPlanningCheckpoint` (append-only)

Planning publishes nightly or on-event captures. Approvals stores them for lookback resolution.

```
AssignmentPlanningCheckpoint
├── PublicId, AssignmentId
├── CapturedAt
├── ProgressRevisionRef
├── PlanSnapshot
└── CaptureSource
```

### 4.2 `AssignmentApprovalRequest` (root — one per assignment workflow)

One **open** pending request per assignment at a time.

```
AssignmentApprovalRequest
├── PublicId, ProjectId, AssignmentId
├── Status: Pending | Approved | Rejected | Superseded
├── RequiredBecause: ProgressChanged | PlanRecalculated | Both
├── ProgressRevisionRef
├── PlanSnapshot (proposed)
├── LookbackBaseline (PlanningStateSnapshot — ~1 week ago)
├── LastApprovedSnapshotId?
├── OpenedAt, OpenedByProcess
└── ClosedAt?
```

### 4.3 `ApprovalDecision` (append-only)

Never updated or deleted. Full audit copy of progress + plan at decision time.

### 4.4 `ApprovedPlanSnapshot` (immutable)

Written only on **Approved**. Baseline for staleness detection.

### 4.5 `ForemanApprovalBatch` (workflow only)

Groups request IDs for one foreman session. Not the consistency boundary.

---

## 5. Value objects

### `ProgressRevisionRef`

```csharp
record ProgressRevisionRef(
    long AssignmentId,
    long RevisionId,              // AssignmentProgressHistory.Id
    DateTimeOffset RecordedAt,
    decimal PercentComplete,
    decimal BookedHours,
    string Source)
{
    string Fingerprint { get; }  // SHA-256 over key fields
}
```

### `PlanSnapshot`

```csharp
record PlanSnapshot(
    DateOnly PlannedStart,
    DateOnly PlannedFinish,
    decimal PlannedHours,
    string ProfileFingerprint,    // hash of AssignmentProfile
    string CalculationRunId)
{
    string Fingerprint { get; }
}
```

---

## 6. One-week lookback

Default window: **`ApprovalLookbackWindow.OneWeek`** (7 days, tenant-configurable [NEEDS REVIEW]).

### Resolve baseline

```text
cutoff = asOf - 7 days
baseline = latest AssignmentPlanningCheckpoint where CapturedAt <= cutoff
```

Implemented in `LookbackBaselineResolver`.

### Approval rules

1. If current matches **last foreman-approved snapshot** → no approval needed.
2. Else if current matches **lookback baseline** (~1 week ago) → no approval needed.
3. Else → open `AssignmentApprovalRequest` with `LookbackBaseline` embedded for foreman UI ("since [date]: progress X→Y, plan shifted…").

```text
AssignmentPlanningCheckpointCaptured (Planning, nightly/event)
  → stored in assignment_planning_checkpoints

AdjustedPlanProposed / AssignmentProgressRevisionRecorded
  → LookbackBaselineResolver.Resolve(history, asOf, OneWeek)
  → ApprovalStalenessEvaluator.Evaluate(current, lookback, lastApproved)
```

---

## 7. Staleness rules (summary)

Re-approval required when current state **differs from lookback baseline** and is **not already covered** by the last foreman approval.

No checkpoint before cutoff → treat as requiring approval (`Both`) [NEEDS REVIEW].

### Event flow

```text
AssignmentProgressRevisionRecorded / AdjustedPlanProposed
  → ApprovalStalenessEvaluator.Evaluate(...)
  → if stale:
       supersede open Pending request (if any)
       open new AssignmentApprovalRequest(Pending)
```

Idempotency key: `(AssignmentId, ProgressRevisionId, CalculationRunId)`.

---

## 8. Persistence (`planning_approvals` schema)

| Table | Notes |
|-------|-------|
| `assignment_planning_checkpoints` | Append-only; index `(assignment_id, captured_at)` |
| `assignment_approval_requests` | Index `(project_id, status, opened_at)`, `(assignment_id, status)` |
| `approval_decisions` | Append-only; index `(assignment_id, decided_at)` |
| `approved_plan_snapshots` | Index `(assignment_id, approved_at DESC)` |
| `foreman_approval_batches` | Optional UI batching |

Separate **`PlanningApprovalsDbContext`** — not `Floor2PlanDbContext`.

---

## 9. CQRS read model (foreman queue)

Denormalized `AssignmentApprovalQueueItem`:

- `AssignmentId`, `ActivityId`, `ProjectId`, `Status`, `IsStale`, `ForemanScope`, `OpenedAt`
- Rebuilt from domain events or projected from aggregates
- Paginated list endpoints — do not join Planning tables in domain layer

---

## 10. Authorization

Extend operational roles (see platform auth standard):

| Persona | Permission |
|---------|------------|
| Team lead / foreman | `Planning.ApproveAdjustments` (new) |
| Planner | `Planning.Read` — see pending state, not approve |
| Viewer | Read-only queue |

---

## 11. SandBox POC

Runnable reference: [`PlanningApprovalsPoc/`](../PlanningApprovalsPoc/)

```bash
cd PlanningApprovalsPoc
dotnet run --project tests/PlanningApprovals.Tests
```

Key types:

| POC | Path |
|-----|------|
| Lookback resolver | `PlanningApprovals.Domain/Services/LookbackBaselineResolver.cs` |
| Staleness | `PlanningApprovals.Domain/Services/ApprovalStalenessEvaluator.cs` |
| Coordinator | `PlanningApprovals.Domain/Services/PlanningApprovalCoordinator.cs` |
| EF | `PlanningApprovals.Infrastructure/PlanningApprovalsDbContext.cs` |
| Scenarios | `PlanningApprovals.Tests/Support/Floor2PlanApprovalScenario.cs` |

---

## 12. Open points [NEEDS REVIEW]

1. **Reject** — block schedule commit or only record dissent?
2. **Auto-approve** below a progress delta threshold?
3. **Foreman scope** — by `Organisation`, discipline, or `PersonInTeam`?
4. **Planning commit** — does `PlanningAdjustmentApproved` event commit the recalculated plan to execution baseline?

---

## 13. Monolith adoption path

1. Extract `PlanningApprovals` module with `AddPlanningApprovalsModule` + `MapPlanningApprovalsEndpoints`.
2. Publish `AssignmentPlanningCheckpointCaptured` (nightly job from Planning) + `AdjustedPlanProposed` / `AssignmentProgressRevisionRecorded`.
3. Subscribe in PlanningApprovals application handler → call `PlanningApprovalCoordinator`.
4. Foreman UI in `@f2p/planning` feature module — approval queue read API.
5. Retire any approval flags added to `Assignment` during legacy experiments.

See `docs/monolith-modularization/module-composition-di.md` for module registration rules.
