# Floor2Plan planning adjustment approvals POC

Reference implementation for **foreman approval of adjusted planning** after assignment progress changes — kept separate from the Planning aggregate model.

Demonstrates:

- Supporting bounded context **`PlanningApprovals`** beside Planning
- Per-assignment **`AssignmentApprovalRequest`** aggregates (scales to hundreds)
- Append-only **`ApprovalDecision`** audit trail + immutable **`ApprovedPlanSnapshot`**
- Append-only **`AssignmentPlanningCheckpoint`** history + **~1 week lookback baseline** on each request
- Optional **`ForemanApprovalBatch`** for bulk UI workflow

## Run

```bash
cd PlanningApprovalsPoc
dotnet run --project tests/PlanningApprovals.Tests
```

## Projects

| Project | Role |
|---------|------|
| `PlanningApprovals.Domain` | Aggregates, value objects, staleness evaluator, coordinator |
| `PlanningApprovals.Infrastructure` | EF Core `PlanningApprovalsDbContext` (separate schema) |
| `PlanningApprovals.Tests` | Domain scenarios + SQLite persistence characterization |

## Design doc

Full architecture and Floor2Plan integration notes:

[`docs/floor2plan-planning-approval-data-model.md`](../docs/floor2plan-planning-approval-data-model.md)

## Floor2Plan mapping

| POC type | Floor2Plan concept |
|----------|-------------------|
| `ProgressRevisionRef` | `AssignmentProgressHistory` + `AssignmentSummary` |
| `PlanSnapshot` | Recalculated assignment dates/profile after progress |
| `AssignmentApprovalRequest` | New — not on `Assignment` entity |
| Integration events | Replace cross-context SaveChanges handlers over time |

**POC / reference only** — not production Floor2Plan behaviour.
