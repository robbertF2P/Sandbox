# F2pPlatform — V2 host + module template

Runnable **Platform 2.0 foundation** for SandBox: composed ASP.NET host, Akka orchestration shell, SignalR progress channel, and a **Reference** bounded-context module.

Copy or scaffold from here into the monolith per `docs/monolith-modularization/foundation-and-pilot-plan.md` (Phase A).

## Layout

```text
host/
  F2pPlatform.Host/              composition root (Program.cs)
  F2pPlatform.Host.Contracts/    platform events + actor messages
  F2pPlatform.Host.Core/         Akka root + SignalR bridge actors
src/Modules/Reference/           template module (Domain → Api)
tests/Modules/Reference/           unit + characterization tests
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

## Scaffold a new module

```bash
./scripts/scaffold-module.sh Import
```

Creates `src/Modules/Import/` from the Reference template and adds projects to `F2pPlatform.slnx`.

## Design notes

- **OOP in the large:** host wires modules via `Add*Module` / `Map*Module`; Akka at host level for long-running workflows.
- **FP in detail:** domain rules in pure static functions (`ReferenceStatusRules`).
- **No ABP** in module projects; strangler adapters marked with `[StranglerAdapter]`.
- **No Hangfire** in target host — scheduled/long-running work routes through actors; UI progress via SignalR.
