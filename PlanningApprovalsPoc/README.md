# Floor2Plan planning adjustment approvals POC

Reference implementation for **foreman approval of selected work assignments** — kept separate from the Planning aggregate model.

Aligned with the simplified **HourApprovals** V2 slice (`F2pPlatform/src/Modules/HourApprovals/`).

## Core workflow

1. **Active assignments** carry hours to go, planned start/finish, and assigned user.
2. Foreman **selects** assignments and records approval for current values.
3. If values change after approval → assignment needs **re-approval**.
4. **One approval record per assignment per UTC day**; repeat approval the same day **updates** that record.

## Run

```bash
cd PlanningApprovalsPoc
dotnet run --project tests/PlanningApprovals.Tests
```

## Projects

| Project | Role |
|---------|------|
| `PlanningApprovals.Domain` | `ActiveAssignment`, `AssignmentApprovalRecord`, rules, coordinator |
| `PlanningApprovals.Infrastructure` | EF Core `PlanningApprovalsDbContext` (SQLite) |
| `PlanningApprovals.Tests` | Domain rules + SQLite persistence |

## Design doc

Broader Floor2Plan integration notes (historical / target architecture):

[`docs/floor2plan-planning-approval-data-model.md`](../docs/floor2plan-planning-approval-data-model.md)

The runnable POC intentionally stays thin; the design doc describes the fuller production model (checkpoints, integration events, etc.).

**POC / reference only** — not production Floor2Plan behaviour.
