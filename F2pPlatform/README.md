# F2pPlatform — V2 host + module template

Runnable **Platform 2.0 foundation** for SandBox: composed ASP.NET host, Akka orchestration shell, SignalR progress channel, and bounded-context modules with paired Angular libs.

**Architecture overview (presentations):** `docs/monolith-modularization/platform-architecture-overview.md` — includes **tenant SPA (single application)** slides and diagram.

Copy or scaffold from here into the monolith per `docs/monolith-modularization/foundation-and-pilot-plan.md` (Phase A).

## Layout

```text
host/
  F2pPlatform.Host/              composition root (Program.cs)
  F2pPlatform.Host.Contracts/    platform events + actor messages
  F2pPlatform.Host.Core/         Akka root + SignalR bridge actors
src/
  Modules/<Context>/             backend bounded context (see MODULE.md in each)
  Packs/                         tenant customization + integration packs
  Shared/                        Platform.Shared.Domain, Platform.Shared.View
tests/Modules/<Context>/
tools/
  F2pPlatform.McpGateway/       stdio MCP server for Copilot / Cursor (Hour Approvals pilot)
web/
  apps/f2p-shell/                tenant SPA host
  libs/<context>/                frontend libs paired 1:1 with backend modules
```

## Module ↔ frontend index

Backend and UI are **paired by context name** in parallel trees. Each backend module has `MODULE.md` with the frontend path.

| Context | Backend | Frontend libs | API prefix | SPA route |
|---------|---------|---------------|------------|-----------|
| **Reference** (template) | `src/Modules/Reference/` | `web/libs/reference/` | `/api/reference` | `/reference` |
| **HourApprovals** | `src/Modules/HourApprovals/` | `web/libs/hour-approvals/` | `/api/hour-approvals` | `/hour-approvals` |
| **Identity** | `src/Modules/Identity/` | *(auth in shell + `libs/identity/`)* | `/api/identity` | `/login` |
| **ControlPlane** | `src/Modules/ControlPlane/` | `web/libs/control-plane/` | `/api/v1/platform` | *(admin shell)* |
| **PlatformConfig** | `src/Modules/PlatformConfig/` | — | `/api/v1/platform/config` | — |

**Shared frontend:** `web/libs/shared/` (`api-core`, `platform-events`, re-export of `@floorganise/ui`).

**Standards:** `docs/monolith-modularization/platform-frontend-standard.md`, `platform-ui-customization-standard.md`, `platform-pack-blueprint.md`.

## Quick start

### Podman / Docker (recommended — full UI + API)

Brings up Seq, the API host, and the tenant SPA with a fresh image build (includes the Floorboard hour-approvals UI):

```bash
cd F2pPlatform
./scripts/podman-up.sh
```

- Tenant UI: `http://localhost:5180`
- Hour approvals: `http://localhost:5180/hour-approvals` (login `supervisor.demo`, any password)
- API / Swagger: `http://localhost:5080/swagger`
- Seq: `http://localhost:8083`

Stop: `./scripts/podman-down.sh`

The script packs `Platform.Serilog.Logging` into `local-feed/` at the version pinned in `build/Platform.Logging.Versions.props`, then runs `podman compose` (falls back to Docker Compose).

### Local development (host)

```bash
cd F2pPlatform
../scripts/pack-local-platform-packages.sh   # once, or when logging package version changes
dotnet build
dotnet test
dotnet run --project host/F2pPlatform.Host
```

- API: `http://localhost:5080`
- Swagger: `/swagger`
- Reference status: `GET /api/reference/status`
- SignalR hub: `/hubs/platform-events`

### Frontend (template)

```bash
cd F2pPlatform/web
npm install
npm start   # http://localhost:5180 — proxies API/SignalR to :5080
```

See [web/README.md](web/README.md).

## Scaffold a new module

```bash
./scripts/scaffold-module.sh Planning          # backend + MODULE.md
./scripts/scaffold-frontend-module.sh Planning # frontend libs (data-access + feature-status)
```

Then register `AddPlanningModule` / `MapPlanningModule` in `host/F2pPlatform.Host/Program.cs`, add a lazy route and home tile in `web/apps/f2p-shell/`.

## Design notes

- **OOP in the large:** host wires modules via `Add*Module` / `Map*Module`; Akka at host level for long-running workflows.
- **FP in detail:** domain rules in pure static functions (`ReferenceStatusRules`).
- **No ABP** in module projects; strangler adapters marked with `[StranglerAdapter]`.
- **No Hangfire** in target host — scheduled/long-running work routes through actors; UI progress via SignalR.
- **Tenant display variance** via customization packs + `Platform.Shared.View` — see `platform-ui-customization-standard.md`.

## Hour approvals POC (V2 slice)

End-to-end reference for **schema-driven UI customization**:

- `HourApprovals` module — approval records (`IRecordAudit`), permissions, in-memory tasks
- **Feature flag** — `Tenant:FeatureFlags:hours-progress-approval` (route/API hidden when off)
- **Acme customization pack** — `labelKey` view schema, batch `extensions`, `computed` columns via `HourApprovals.Packs.Acme`
- Angular route — `/hour-approvals` (login as `supervisor.demo` or `foreman.demo`)

```bash
dotnet run --project host/F2pPlatform.Host
cd web && npm start
```

## MCP gateway (Copilot / Cursor)

Stdio MCP server that exposes Hour Approvals and platform health tools over the existing REST API — no duplicate business logic.

**Prerequisites:** F2P host running (`http://localhost:5080` via Podman or `dotnet run`).

| Tool | API |
|------|-----|
| `get_hour_approvals_capabilities` | `GET /api/hour-approvals/capabilities` |
| `list_hour_approval_tasks` | `GET /api/hour-approvals/tasks` |
| `get_hour_approval_task` | `GET /api/hour-approvals/tasks/{id}` |
| `approve_hour_approval_task` | `POST /api/hour-approvals/tasks/{id}/approve` |
| `get_platform_health` | `GET /health` |

### VS Code + GitHub Copilot

1. Open the `F2pPlatform` folder in VS Code.
2. Ensure your org has **MCP servers in Copilot** enabled (Business/Enterprise).
3. Start the API host (Podman or local `dotnet run`).
4. Open `.vscode/mcp.json` and click **Start** on the `floor2plan` server.
5. In Copilot Chat, switch to **Agent** mode and use the tools icon to verify tools are listed.

Environment variables (also set in `mcp.json`):

| Variable | Default | Purpose |
|----------|---------|---------|
| `F2P_API_BASE_URL` | `http://localhost:5080` | Platform host base URL |
| `F2P_USER_NAME` | `supervisor.demo` | `X-User-Name` header (POC auth) |
| `F2P_USER_PERMISSIONS` | `ApproveHoursProgress` | `X-User-Permissions` header |

```bash
dotnet run --project tools/F2pPlatform.McpGateway/F2pPlatform.McpGateway.csproj
```
