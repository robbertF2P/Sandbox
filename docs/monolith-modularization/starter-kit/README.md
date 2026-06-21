# Module refactor starter kit

**Purpose:** Copy-once scaffolding for the external F2P monolith. Every extracted bounded context starts from this kit — not from a blank AI chat session.

**Parent plan:** `../foundation-and-pilot-plan.md` (Phase A7)

---

## Install (monolith repo)

```bash
SB=/path/to/SandBox
MONO=/path/to/Floor2Plan.Core2

mkdir -p "$MONO/Build/Platform" "$MONO/scripts" "$MONO/docs/modularization/starter-kit"
cp "$SB/build/Platform.Logging."*.props "$MONO/Build/Platform/"
cp "$SB/scripts/add-platform-logging-to-module.sh" "$MONO/scripts/"
cp -R "$SB/docs/monolith-modularization/starter-kit/" "$MONO/docs/modularization/"
# When available:
# cp "$SB/scripts/scaffold-module.sh" "$MONO/scripts/"
cp "$SB/scripts/scaffold-module.sh" "$MONO/scripts/"
# Runnable reference host + module:
# cp -R "$SB/F2pPlatform/host" "$MONO/Src/Platform/"   # adjust paths per monolith layout
```

Point monolith `nuget.config` at your internal feed for `Platform.Serilog.Logging` packages (published from SandBox).

---

## Contents

| Path | Status | Description |
|------|--------|-------------|
| `README.md` | **done** | This manifest |
| `F2pPlatform/` (SandBox) | **done** | Runnable V2 host + Reference backend + **web/** frontend template |
| `F2pPlatform/web/` | **done** | `f2p-shell` + `libs/reference/{data-access,feature-status}` |
| `templates/frontend/` | **done** | Data-access + lazy-route stubs |
| `templates/CharacterizationTest.cs` | **done** | WebApplicationFactory smoke test stub |
| `templates/DependencyInjection.Api.cs` | **done** | `Add<Context>Module` + `Map<Context>Endpoints` stub |
| `templates/StranglerAdapter.cs` | **done** | `[StranglerAdapter]` marker attribute |
| `templates/integration-pack/` | **planned** | Pack manifest + ports-only layout (Pilot 2) |
| `templates/pr-module-extraction.md` | **planned** | PR body checklist |
| `../module-composition-di.md` | **done** | DI standard — extension methods, no ABP |
| `../platform-frontend-standard.md` | **done** | `@floorganise/css` + `@floorganise/ui` for V2 modules |
| SandBox `build/Platform.Logging.*.props` | **done** | Serilog MSBuild imports |
| SandBox `scripts/add-platform-logging-to-module.sh` | **done** | Wire logging to module root |
| SandBox `scripts/scaffold-module.sh` | **done** | Generate `F2pPlatform/src/Modules/<Context>/` from Reference |
| SandBox `scripts/scaffold-frontend-module.sh` | **done** | Generate `F2pPlatform/web/libs/<context>/` from reference |
| `../templates/azure-pipelines-module-tests.yml` | **done** | ADO per-module test jobs |
| `../templates/*.schema.yaml` | **done** | Analysis artifact schemas |

---

## Module template layout (target)

```text
Src/Modules/<Context>/
├── src/
│   ├── <Context>.Domain/
│   ├── <Context>.Application/
│   ├── <Context>.Infrastructure/
│   └── <Context>.Api/              ← Add<Context>Module + Map<Context>Endpoints
└── tests/
    ├── <Context>.Unit.Tests/
    ├── <Context>.Integration.Tests/
    └── <Context>.Characterization.Tests/
```

### Dependency rules (enforced manually at first; CI architecture tests later)

- **Domain** — no EF, ASP.NET, Hangfire, vendor SDKs
- **Application** — ports only; no Infrastructure references
- **Infrastructure** — EF, Hangfire, external IO
- **Api** — `DependencyInjection.cs` with `Add<Context>Module`; endpoint mapping via `Map<Context>Endpoints`
- **Legacy → new** allowed via feature flag; **new → legacy** only via `[StranglerAdapter]`
- **No ABP** — no `AbpModule`, `Volo.Abp.*`, or `AbpDbContext` in new module projects (see `../module-composition-di.md`)

---

## Scaffold workflow

```bash
# 1. Create module tree from Reference template
./scripts/scaffold-module.sh Import

# 2. Wire platform logging (monolith path example)
./scripts/add-platform-logging-to-module.sh F2pPlatform/src/Modules/Import

# 3. Register in host: builder.Services.Add<Context>Module(config); app.Map<Context>Module();

# 4. Run smoke characterization test

dotnet build F2pPlatform && dotnet test F2pPlatform
```

Runnable reference: `F2pPlatform/` — `dotnet build`, `dotnet test`, `dotnet run --project F2pPlatform/host/F2pPlatform.Host`.

---

## Agent prompt (paste when scaffolding)

```text
Use docs/modularization/starter-kit/README.md and module-composition-di.md.
Do not invent project layout. Scaffold <Context> with Domain/Application/
Infrastructure/Api + three test projects. Use Add<Context>Module and
Map<Context>Endpoints — no AbpModule. Add smoke characterization test.
```

---

## NuGet vs copy

| Consume via NuGet / npm | Copy into monolith |
|-------------------------|-------------------|
| `Platform.Serilog.Logging` | MSBuild props, scripts, templates |
| `Platform.Serilog.Logging.Testing` | PR template, agent rules |
| `ImportPipeline.Domain` (Import pilot) | Module folder skeleton |
| `@floorganise/css` | — (npm feed only) |
| `@floorganise/ui` | — (npm feed only; scaffold in SandBox `FloorganiseCss/ui/`) |

Do **not** project-reference SandBox POC repos from the monolith.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 0.2 | 2026-06-21 | `F2pPlatform/` host + Reference module; `scaffold-module.sh`; template stubs |
| 0.1 | 2026-06-21 | Manifest + existing props/scripts |
