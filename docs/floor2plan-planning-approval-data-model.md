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
| **Stale approval** | Snapshot no longer matches current progress or proposed plan |

---

## 4. Aggregates

### 4.1 `AssignmentApprovalRequest` (root — one per assignment workflow)

One **open** pending request per assignment at a time.

```
AssignmentApprovalRequest
├── PublicId, ProjectId, AssignmentId
├── Status: Pending | Approved | Rejected | Superseded
├── RequiredBecause: ProgressChanged | PlanRecalculated | Both
├── ProgressRevisionRef
├── PlanSnapshot (proposed)
├── LastApprovedSnapshotId?
├── OpenedAt, OpenedByProcess
└── ClosedAt?
```

### 4.2 `ApprovalDecision` (append-only)

Never updated or deleted. Full audit copy of progress + plan at decision time.

### 4.3 `ApprovedPlanSnapshot` (immutable)

Written only on **Approved**. Baseline for staleness detection.

### 4.4 `ForemanApprovalBatch` (workflow only)

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

## 6. Staleness rules

Re-approval required when **either**:

1. `currentProgress.Fingerprint != lastApproved.ProgressRevision.Fingerprint`, or  
2. `currentProposedPlan.Fingerprint != lastApproved.PlanSnapshot.Fingerprint`.

No prior approval → always required.

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

## 7. Persistence (`planning_approvals` schema)

| Table | Notes |
|-------|-------|
| `assignment_approval_requests` | Index `(project_id, status, opened_at)`, `(assignment_id, status)` |
| `approval_decisions` | Append-only; index `(assignment_id, decided_at)` |
| `approved_plan_snapshots` | Index `(assignment_id, approved_at DESC)` |
| `foreman_approval_batches` | Optional UI batching |

Separate **`PlanningApprovalsDbContext`** — not `Floor2PlanDbContext`.

---

## 8. CQRS read model (foreman queue)

Denormalized `AssignmentApprovalQueueItem`:

- `AssignmentId`, `ActivityId`, `ProjectId`, `Status`, `IsStale`, `ForemanScope`, `OpenedAt`
- Rebuilt from domain events or projected from aggregates
- Paginated list endpoints — do not join Planning tables in domain layer

---

## 9. Authorization

Extend operational roles (see platform auth standard):

| Persona | Permission |
|---------|------------|
| Team lead / foreman | `Planning.ApproveAdjustments` (new) |
| Planner | `Planning.Read` — see pending state, not approve |
| Viewer | Read-only queue |

---

## 10. SandBox POC

Runnable reference: [`PlanningApprovalsPoc/`](../PlanningApprovalsPoc/)

```bash
cd PlanningApprovalsPoc
dotnet run --project tests/PlanningApprovals.Tests
```

Key types:

| POC | Path |
|-----|------|
| Staleness | `PlanningApprovals.Domain/Services/ApprovalStalenessEvaluator.cs` |
| Coordinator | `PlanningApprovals.Domain/Services/PlanningApprovalCoordinator.cs` |
| EF | `PlanningApprovals.Infrastructure/PlanningApprovalsDbContext.cs` |
| Scenarios | `PlanningApprovals.Tests/Support/Floor2PlanApprovalScenario.cs` |

---

## 11. Open points [NEEDS REVIEW]

1. **Reject** — block schedule commit or only record dissent?
2. **Auto-approve** below a progress delta threshold?
3. **Foreman scope** — by `Organisation`, discipline, or `PersonInTeam`?
4. **Planning commit** — does `PlanningAdjustmentApproved` event commit the recalculated plan to execution baseline?

---

## 12. Monolith adoption path

1. Extract `PlanningApprovals` module with `AddPlanningApprovalsModule` + `MapPlanningApprovalsEndpoints`.
2. Publish `AssignmentProgressRevisionRecorded` / `AdjustedPlanProposed` from Planning (outbox).
3. Subscribe in PlanningApprovals application handler → call `PlanningApprovalCoordinator`.
4. Foreman UI in `@f2p/planning` feature module — approval queue read API.
5. Retire any approval flags added to `Assignment` during legacy experiments.

See `docs/monolith-modularization/module-composition-di.md` for module registration rules.
