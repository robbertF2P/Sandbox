# F2P Platform Web — frontend module template

Angular workspace structured like the monolith **floor2plan-web** Nx layout (path aliases instead of Nx for SandBox simplicity).

## Layout

```text
apps/f2p-shell/                 host SPA (router, home tiles)
apps/f2p-admin-shell/           platform operator admin SPA (tenant provisioning)
libs/shared/api-core/           HTTP client + correlation headers
libs/shared/platform-events/    SignalR hub client
libs/control-plane/data-access/ Admin API client for control plane
libs/control-plane/feature-tenants/ Tenant list + create UI
libs/reference/data-access/     API client for Reference backend
libs/reference/feature-status/  lazy-loaded feature route (template)
libs/platform-events/feature-live/  SignalR hub live feed page
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

Open http://localhost:5180 → **Reference** tile (module status) or **Platform events** (SignalR feed).

### Admin console

```bash
# Terminal 2 — operator admin SPA (proxies /admin to :5080)
npm run start:admin
```

Open http://localhost:5181 → create and list tenants.

## Scaffold a new context UI

```bash
./scripts/scaffold-frontend-module.sh Planning
```

Then add a lazy route in `apps/f2p-shell/src/app/app.routes.ts` and a home tile.

## Monolith migration

Copy `web/` into `floor2plan-web/` or convert path aliases to Nx `project.json` libs per `docs/floor2plan-v2-read-model-playbook.md`.

When `@floorganise/ui` ships, replace inline shell markup with shared components — do not fork tokens.
