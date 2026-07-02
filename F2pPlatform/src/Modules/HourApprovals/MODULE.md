# HourApprovals module

Foreman approval of selected work tasks (hours to go, planned dates, assigned user).

## Backend

| Layer | Path |
|-------|------|
| Domain | `HourApprovals.Domain/` |
| Application | `HourApprovals.Application/` |
| Infrastructure | `HourApprovals.Infrastructure/` |
| Migrations | `HourApprovals.Data.Migrations/` |
| API | `HourApprovals.Api/` |
| Tests | `tests/Modules/HourApprovals/` |

## Frontend

| Lib | Path |
|-----|------|
| data-access | `web/libs/hour-approvals/data-access/` |
| feature | `web/libs/hour-approvals/feature-tasks/` |

| Route | `/hour-approvals` |
| API prefix | `/api/hour-approvals` |
| Feature flag | `Tenant:FeatureFlags:hours-progress-approval` |

## Core workflow

1. Foreman lists active tasks and selects rows to approve.
2. Optional edits to hours to go, planned start/finish, or assigned user are saved without recording approval.
3. **Submit** records approval for selected tasks only.
4. Approval is compared to current values; any change marks the task as needing re-approval.
5. At most **one approval record per task per calendar day (UTC)**; repeat approvals the same day update that record.

## Application boundaries

| Folder | Port | Implemented by |
|--------|------|----------------|
| `Application/Ports/` | `IHourApprovalsService`, `IHourApprovalsCustomizationPack`, `IHourApprovalsFeatureGate` | Application / pack / Infrastructure |
| `Application/Persistence/` | `IHourApprovalsRepository` | `EfHourApprovalsRepository` (Infrastructure) |

**Persistence:** SQL Server + `HourApprovals.Data.Migrations` for dev; SQLite when `HourApprovals:UseSqlite` is true (docker stack, tests).

Connection string: `ConnectionStrings:HourApprovals`. Host runs migrate/seed on startup.

## Customization

| Port | `IHourApprovalsCustomizationPack` |
| Default pack | `DefaultHourApprovalsPack` (Infrastructure) |
| Example client pack | `HourApprovals.Packs.Acme` (`acme-hour-approvals-v1`) — see `PACK.md` in pack folder |

See `docs/monolith-modularization/platform-ui-customization-standard.md` and `platform-pack-blueprint.md`.

Scaffold a new pack: `./scripts/scaffold-customization-pack.sh <Context> <Client> <pack-id>`.

## Scaffold sibling UI

```bash
./scripts/scaffold-frontend-module.sh HourApprovals   # if starting fresh
```
