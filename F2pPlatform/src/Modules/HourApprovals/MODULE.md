# HourApprovals module

Supervisor/foreman hour approval workflow (V2 slice).

## Backend

| Layer | Path |
|-------|------|
| Domain | `HourApprovals.Domain/` |
| Application | `HourApprovals.Application/` |
| Infrastructure | `HourApprovals.Infrastructure/` |
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

## Customization

| Port | `IHourApprovalsCustomizationPack` |
| Default pack | `DefaultHourApprovalsPack` (Infrastructure) |
| Example client pack | `HourApprovals.Packs.Acme` (`acme-hour-approvals-v1`) |

See `docs/monolith-modularization/platform-ui-customization-standard.md`.

## Scaffold sibling UI

```bash
./scripts/scaffold-frontend-module.sh HourApprovals   # if starting fresh
```
