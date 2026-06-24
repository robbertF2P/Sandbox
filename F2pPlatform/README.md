# F2pPlatform — V2 host + module template

Runnable **Platform 2.0 foundation** for SandBox: composed ASP.NET host, Akka orchestration shell, SignalR progress channel, and a **Reference** bounded-context module.

Copy or scaffold from here into the monolith per `docs/monolith-modularization/foundation-and-pilot-plan.md` (Phase A).

## Layout

```text
host/
  F2pPlatform.Host/              composition root (Program.cs)
  F2pPlatform.Host.Contracts/    platform events + actor messages
  F2pPlatform.Host.Core/         Akka root + SignalR bridge actors
src/Modules/Reference/           template backend module (Domain → Api)
tests/Modules/Reference/         unit + characterization tests
web/                             Angular f2p-shell + Reference UI libs
```

## Quick start

```bash
cd F2pPlatform
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
./scripts/scaffold-module.sh Import          # backend
./scripts/scaffold-frontend-module.sh Import # frontend libs
```

Creates `src/Modules/Import/` from the Reference template and adds projects to `F2pPlatform.slnx`.

## Design notes

- **OOP in the large:** host wires modules via `Add*Module` / `Map*Module`; Akka at host level for long-running workflows.
- **FP in detail:** domain rules in pure static functions (`ReferenceStatusRules`).
- **No ABP** in module projects; strangler adapters marked with `[StranglerAdapter]`.
- **No Hangfire** in target host — scheduled/long-running work routes through actors; UI progress via SignalR.

## Hour approvals POC (V2 slice)

Demonstrates supervisor/foreman hour approval with:

- `HourApprovals` module — approval records (`IRecordAudit`), permissions, in-memory tasks
- **Feature flag** — `Tenant:FeatureFlags:hours-progress-approval` (route/API hidden when off)
- **Acme customization pack** — `ShowPlannedStart` / `ShowPlannedFinish` via `HourApprovals.Packs.Acme`
- Angular route — `/hour-approvals` (login as `supervisor.demo` or `foreman.demo`)

```bash
dotnet run --project host/F2pPlatform.Host
cd web && npm start
```
