# Recorded progress approval workflow POC

Reference implementation for **multi-stage approval of recorded progress** before **ERP export eligibility**.

Aligned with `docs/floor2plan-recorded-progress-approval-workflow.md`.

## Core workflow

1. **Recorded progress** is captured per assignment (immutable revision).
2. A **submission** opens and advances through stages: Submitted → ForemanChecked → PlanningReviewed → Approved.
3. Each stage records an append-only **decision** (approved or rejected).
4. **ERP export** is allowed only when all pipeline stages are approved.
5. **Reject** at any stage closes the submission; export remains blocked.

## Run

```bash
cd RecordedProgressApprovalsPoc
dotnet run --project tests/RecordedProgressApprovals.Tests
```

## Projects

| Project | Role |
|---------|------|
| `RecordedProgressApprovals.Domain` | Submission aggregate, stage rules, coordinator |
| `RecordedProgressApprovals.Tests` | Domain rules + coordinator scenarios |

## Design doc

[`docs/floor2plan-recorded-progress-approval-workflow.md`](../docs/floor2plan-recorded-progress-approval-workflow.md)

**POC / reference only** — not production Floor2Plan behaviour.
