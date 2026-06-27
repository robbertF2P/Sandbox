# Reference module

Template bounded context — copy via `./scripts/scaffold-module.sh <Context>`.

## Backend

| Layer | Path |
|-------|------|
| Domain | `Reference.Domain/` |
| Application | `Reference.Application/` |
| Infrastructure | `Reference.Infrastructure/` |
| API | `Reference.Api/` |
| Tests | `tests/Modules/Reference/` |

## Frontend

| Lib | Path |
|-----|------|
| data-access | `web/libs/reference/data-access/` |
| feature | `web/libs/reference/feature-status/` |

| Route | `/reference` |
| API prefix | `/api/reference` |

## Scaffold sibling UI

```bash
./scripts/scaffold-frontend-module.sh Reference   # template only — use a new context name
./scripts/scaffold-frontend-module.sh Planning
```

See `F2pPlatform/README.md` for the full module ↔ frontend index.
