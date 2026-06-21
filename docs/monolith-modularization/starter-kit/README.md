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
# cp -R "$SB/docs/monolith-modularization/starter-kit/templates/module" "$MONO/Src/Modules/_template/"
```

Point monolith `nuget.config` at your internal feed for `Platform.Serilog.Logging` packages (published from SandBox).

---

## Contents

| Path | Status | Description |
|------|--------|-------------|
| `README.md` | **done** | This manifest |
| `templates/module/` | **planned** | Domain / Application / Infrastructure / Api / test `.csproj` + stub types |
| `templates/integration-pack/` | **planned** | Pack manifest + ports-only layout (Pilot 2) |
| `templates/DependencyInjection.cs` | **planned** | `Add<Context>Module` + `Map<Context>Endpoints` stubs (no ABP) |
| `templates/CharacterizationTest.cs` | **planned** | WebApplicationFactory smoke test with `Module`/`Tier` traits |
| `../module-composition-di.md` | **done** | DI standard — extension methods, no ABP |
| `../platform-frontend-standard.md` | **done** | `@floorganise/css` + `@floorganise/ui` for V2 modules |
| `templates/pr-module-extraction.md` | **planned** | PR body checklist |
| SandBox `build/Platform.Logging.*.props` | **done** | Serilog MSBuild imports |
| SandBox `scripts/add-platform-logging-to-module.sh` | **done** | Wire logging to module root |
| SandBox `scripts/scaffold-module.sh` | **planned** | Generate `Src/Modules/<Context>/` from template |
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
# 1. Create module tree (when scaffold-module.sh exists)
./scripts/scaffold-module.sh Import

# 2. Wire platform logging
./scripts/add-platform-logging-to-module.sh Src/Modules/Import

# 3. Register in host: builder.Services.Add<Context>Module(config); app.Map<Context>Module();

# 4. Run smoke characterization test from templates/CharacterizationTest.cs

dotnet build && dotnet test
```

Until `scaffold-module.sh` exists, copy `_template/` manually and rename `Reference` → `<Context>`.

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
| 0.1 | 2026-06-21 | Manifest + existing props/scripts; templates planned |
