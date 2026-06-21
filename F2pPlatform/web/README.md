# F2P Platform Web — frontend module template

Angular workspace structured like the monolith **floor2plan-web** Nx layout (path aliases instead of Nx for SandBox simplicity).

## Layout

```text
apps/f2p-shell/                 host SPA (router, home tiles)
libs/shared/api-core/           HTTP client + correlation headers
libs/shared/platform-events/    SignalR hub client
libs/reference/data-access/     API client for Reference backend
libs/reference/feature-status/  lazy-loaded feature route (template)
```

Matches `docs/monolith-modularization/platform-frontend-standard.md`: `@floorganise/css`, semantic `f2ps-*` classes, context `data-access` + `feature-*` split.

## Run (two terminals)

```bash
# Terminal 1 — API host
dotnet run --project ../host/F2pPlatform.Host

# Terminal 2 — SPA (proxies /api and /hubs to :5080)
npm install
npm start
```

Open http://localhost:5180 → **Reference** tile → status + live platform events.

## Scaffold a new context UI

```bash
./scripts/scaffold-frontend-module.sh Planning
```

Then add a lazy route in `apps/f2p-shell/src/app/app.routes.ts` and a home tile.

## Monolith migration

Copy `web/` into `floor2plan-web/` or convert path aliases to Nx `project.json` libs per `docs/floor2plan-v2-read-model-playbook.md`.

When `@floorganise/ui` ships, replace inline shell markup with shared components — do not fork tokens.
