# Module refactor starter kit

**Purpose:** Copy-once scaffolding for the external F2P monolith. Every extracted bounded context starts from this kit — not from a blank Claude session.

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
| `templates/CharacterizationTest.cs` | **planned** | WebApplicationFactory smoke test with `Module`/`Tier` traits |
| `templates/StranglerAdapter.cs` | **planned** | Adapter marker + removal ticket comment |
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
│   └── <Context>.Api/              ← IModule + DI registration
└── tests/
    ├── <Context>.Unit.Tests/
    ├── <Context>.Integration.Tests/
    └── <Context>.Characterization.Tests/
```

### Dependency rules (enforced manually at first; CI architecture tests later)

- **Domain** — no EF, ASP.NET, Hangfire, vendor SDKs
- **Application** — ports only; no Infrastructure references
- **Infrastructure** — EF, Hangfire, external IO
- **Api** — composition for the module; registers services + endpoints
- **Legacy → new** allowed via feature flag; **new → legacy** only via `[StranglerAdapter]`

---

## Scaffold workflow

```bash
# 1. Create module tree (when scaffold-module.sh exists)
./scripts/scaffold-module.sh Import

# 2. Wire platform logging
./scripts/add-platform-logging-to-module.sh Src/Modules/Import

# 3. Register IModule in UI.Floor2Plan / host startup

# 4. Run smoke characterization test from templates/CharacterizationTest.cs

dotnet build && dotnet test
```

Until `scaffold-module.sh` exists, copy `_template/` manually and rename `Reference` → `<Context>`.

---

## Claude instruction (paste when scaffolding)

```text
Use docs/modularization/starter-kit/README.md and Src/Modules/_template/.
Do not invent project layout. Scaffold <Context> with Domain/Application/
Infrastructure/Api + three test projects. Register IModule. Add smoke
characterization test from starter-kit templates. Link to slice_id in PR template.
```

---

## NuGet vs copy

| Consume via NuGet | Copy into monolith |
|-------------------|-------------------|
| `Platform.Serilog.Logging` | MSBuild props, scripts, templates |
| `Platform.Serilog.Logging.Testing` | PR template, agent rules |
| `ImportPipeline.Domain` (Import pilot) | Module folder skeleton |

Do **not** project-reference SandBox POC repos from the monolith.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 0.1 | 2026-06-21 | Manifest + existing props/scripts; templates planned |
