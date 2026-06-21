# Foundation-first modularization plan

**Purpose:** Actionable plan for refactoring the **external Floor2Plan monolith** (F2P / `Floor2Plan.Core2`) using a strangler-fig approach. Use this document as the primary Claude/Cursor instruction set when working **in the monolith repo**.

**This SandBox repo** holds reference POCs, analysis templates, and cross-cutting standards. The monolith lives elsewhere — do not expect monolith source here.

**Related (copy or link from monolith `docs/modularization/`):**

| Document | Role |
|----------|------|
| `copilot-analysis-instructions.md` | Phased analysis (inventory → slice plan) |
| `ai-assisted-delivery-quality-framework.md` | Gates, anti-slop, DoD |
| `copilot-instructions-snippet.md` | Short agent rules block |
| `../Modularization/00-inventory.md` | **Example** Phase 0 output for F2P (regenerate in monolith) |
| SandBox `03-modularization-roadmap.md` | POC cross-cutting standards (Serilog, NuGet boundaries) |
| `starter-kit/README.md` | **Module refactor starter kit** — copy into monolith (see below) |
| `../floor2plan-v2-connector-architecture.md` | Integration pack dependency rules |

---

## Executive summary

```text
Phase A  Foundation + starter kit (monolith)          ← no behaviour change
Phase B  Analysis gates (inventory → context map)     ← docs only, human validated
Phase C  Pilot module 1 (prove extract + test loop)   ← one strangler slice
Phase D  Pilot module 2 (prove integration OR UI)     ← second seam type
Phase E  Scale (remaining contexts, sunset adapters)  ← only after pilots pass
```

**Include the module refactor starter kit in Phase A.** Without it, every extraction becomes a one-off layout debate and Claude generates inconsistent project structure. The kit is copied once into the monolith; pilots and scale work **scaffold from it**, not reinvent it.

**Core rule:** AI drafts analysis and code; **humans and green tests decide**. No module move without characterization tests on P0 behaviour.

**Do not** start by splitting `Floor2PlanDbContext` or rewriting the UI. **Do** establish module shape, test harness, CI gates, and prove one vertical slice end-to-end.

---

## Two-repo model

| Repo | Contains | Your work there |
|------|----------|-----------------|
| **F2P monolith** (external) | Production code, tests, CI | Analysis artifacts, adapters, first extractions |
| **SandBox** (this repo) | POC modules, templates, standards | Reference implementations; publish shared NuGet packages |

Shared packages (e.g. `Platform.Serilog.Logging`, `ImportPipeline.Domain`) are developed in SandBox, published to your feed, and **consumed** by monolith modules — not project-referenced across repos.

---

## Phase A — Foundation (before any module extraction)

Goal: create the **scaffolding** every future module will reuse. No user-visible behaviour change.

### A1. Monolith repo hygiene

Create in the monolith repo:

```text
docs/modularization/
├── README.md                          ← links to this plan + quality framework
├── 00-inventory.md                    ← Phase 0 (see copilot-analysis-instructions)
├── templates/                         ← copy from SandBox templates/
└── contexts/                          ← per-context use cases + test plans (later)

.cursor/rules/monolith-modularization.mdc             ← paste copilot-instructions-snippet.md
```

**Claude prompt (monolith repo):**

```text
Read docs/modularization/README.md and copilot-instructions-snippet rules.

PHASE 0 ONLY: produce docs/modularization/00-inventory.md per
copilot-analysis-instructions.md. Cite every claim with file paths.
Mark unknowns [NEEDS REVIEW]. Do not propose architecture yet.
```

Human gate **G0**: tech lead reviews inventory completeness.

### A2. Target module project template

Define the **canonical layout** for every extracted bounded context:

```text
<F2P.Modules>/
├── src/
│   ├── <Context>.Domain/              ← entities, value objects, domain services; no EF, no HTTP
│   ├── <Context>.Application/         ← use cases, ports (interfaces), DTOs
│   ├── <Context>.Infrastructure/      ← EF, external adapters, Hangfire job impls
│   ├── <Context>.Api/                 ← IModule registration, minimal endpoints
│   └── <Context>.Contracts/           ← public integration surface (events, DTOs) if shared
└── tests/
    ├── <Context>.Unit.Tests/
    ├── <Context>.Integration.Tests/
    └── <Context>.Characterization.Tests/
```

**Dependency rules (enforce in CI later):**

| Layer | May reference | Must NOT reference |
|-------|---------------|-------------------|
| Domain | Domain only | EF, ASP.NET, Hangfire, vendor SDKs, other contexts' Domain |
| Application | Domain + own ports | Infrastructure, DbContext |
| Infrastructure | Application + Domain | Other contexts' Infrastructure |
| Api / IModule | Application | Other modules' internals |
| Integration pack | `Integration.Abstractions` only | Core Domain, DbContext |

Add a **blank reference module** scaffolded from the **starter kit** (see A7) — e.g. `Reference.Module` — with one trivial use case to validate the solution builds, tests run, and `IModule` registers in the host.

### A3. Composition host sketch

Do **not** replace `UI.Floor2Plan` yet. Add a **composition root** pattern the monolith can grow into:

- Existing host remains the traffic router.
- New modules register via `IModule` (or ABP module equivalent) from a single `ModuleCatalog`.
- Feature flags / route prefixes decide legacy vs new path per slice.

Document the adapter contract:

```csharp
// Legacy calls new (allowed during strangler)
// New calls legacy via [StranglerAdapter] (allowed, time-boxed)
// Legacy must NOT reference new module projects (forbidden)
```

### A4. Test infrastructure

Before moving code, standardize:

| Capability | Purpose |
|------------|---------|
| `WebApplicationFactory` / TestHost harness | Characterization through HTTP/MVC/API entry points |
| Test database strategy | LocalDB or container; migrations from existing `DatabaseDeployer` |
| Golden file folder | Import/export intermediate format fixtures |
| Test traits | `Module`, `Tier`, `UC`, `AC` for ADO dashboards |
| xUnit + existing assertion libs | Keep `AwesomeAssertions`, `Moq` — no parallel test stack |

**Sprint 0 deliverable:** one **smoke characterization test** that hits a known P0 entry point through the real host and asserts DB state — even before extraction. Proves the harness works.

### A5. Cross-cutting platform packages (from SandBox)

Adopt before pilot modules:

| Package | Why first |
|---------|-----------|
| `Platform.Serilog.Logging` | One log shape across legacy + new; Seq dev / App Insights prod |
| `Platform.Serilog.Logging.Testing` | xUnit sink — test output matches production logs |

Optional for import pilots:

| Package | When |
|---------|------|
| `ImportPipeline.Domain` | Config-driven row mapping kernel |
| Per-vendor readers (e.g. Primavera Excel) | When pilot touches import |

Wire via MSBuild props (`Platform.Logging.*.props`) — see SandBox `platform-logging-standard.md`.

### A6. CI merge gates (monolith)

Add or confirm as **required PR checks**:

```yaml
- dotnet build --configuration Release
- dotnet test --configuration Release
- warnings_as_errors: true (touched projects minimum)
- pr_template: links UC-/AC-/slice_id; characterization unchanged or green
```

See `ai-assisted-delivery-quality-framework.md` for full DoD.

### A7. Module refactor starter kit (required)

The starter kit is the **concrete Phase A deliverable** — not optional documentation. Copy it from SandBox into the monolith once; every new bounded context clones or scaffolds from it.

**Why include it:**

| Without starter kit | With starter kit |
|-------------------|------------------|
| Claude invents folder layout per session | One canonical layout; AI fills slots |
| Inconsistent test project wiring | Characterization + integration harness pre-wired |
| Logging/CI/analyzers renegotiated each PR | MSBuild props + traits baked in |
| "Reference module" stays hypothetical | Runnable proof the host accepts `IModule` |

**Kit contents** (maintained in SandBox under `docs/monolith-modularization/starter-kit/`):

| Artifact | Purpose | Lives in |
|----------|---------|----------|
| `templates/module/` | Domain / Application / Infrastructure / Api / Tests `.csproj` skeletons | SandBox → copied to monolith `Src/Modules/_template/` |
| `templates/integration-pack/` | Pack manifest + abstractions-only layout (Pilot 2 option) | SandBox → monolith `Src/IntegrationPacks/_template/` |
| `build/Platform.Logging.*.props` | Serilog adoption (or NuGet-only equivalent) | SandBox `build/` → monolith `Build/Platform/` |
| `scripts/scaffold-module.sh` | `./scaffold-module.sh Import` → project tree + solution entries | SandBox → monolith `scripts/` |
| `scripts/add-platform-logging-to-module.sh` | Wire logging props to a module root | SandBox → monolith `scripts/` |
| `templates/pr-module-extraction.md` | PR checklist (UC/AC, slice_id, characterization) | SandBox → monolith `.github/` |
| `templates/CharacterizationTest.cs` | Smoke test stub (`WebApplicationFactory`, traits) | In module template |
| `templates/StranglerAdapter.cs` | Marked adapter base with removal ticket placeholder | In module template |
| `templates/azure-pipelines-module-tests.yml` | Per-module ADO jobs | SandBox → monolith pipelines folder |
| Analysis YAML schemas | use-case, test-case, integration schemas | Already in `templates/` |

**Consume via NuGet (do not copy source):**

| Package | Source repo |
|---------|-------------|
| `Platform.Serilog.Logging` (+ `.Testing`) | SandBox → internal feed |
| `ImportPipeline.Domain` | SandBox → internal feed (Import pilot) |

**Read-only reference (do not copy into monolith deployment):**

| POC | Use when |
|-----|----------|
| `ApiImportActorPoc/` | Import pilot considers actor orchestration |
| `AkkaSignalRVuePoc/` | Reactive hosting patterns |
| `PrimaveraExcelReader/` | Excel import ACL |

**Monolith install target** (after one-time copy):

```text
Floor2Plan.Core2/
├── Build/Platform/                    ← Platform.Logging.*.props
├── Src/Modules/
│   ├── _template/                     ← starter kit module skeleton (do not deploy)
│   └── Reference/                     ← first runnable module from kit
├── Src/IntegrationPacks/_template/      ← optional; Pilot 2 if pack-based
├── scripts/scaffold-module.sh
├── docs/modularization/                 ← plan + analysis artifacts
└── .github/pull_request_template.md     ← module extraction section
```

**Scaffold workflow (every new context):**

```bash
./scripts/scaffold-module.sh Import          # creates Src/Modules/Import/...
./scripts/add-platform-logging-to-module.sh Src/Modules/Import
dotnet build && dotnet test
```

**Claude prompt (after kit is installed):**

```text
Scaffold bounded context "<Context>" using Src/Modules/_template/ and
docs/modularization/starter-kit/README.md. Do not invent layout.
Register IModule in the host. Add one smoke characterization test from
templates/CharacterizationTest.cs. No domain logic yet.
```

### Foundation exit criteria (Gate G4-ready)

- [ ] Starter kit copied to monolith (`Build/Platform/`, `scripts/`, `_template/`)
- [ ] `docs/modularization/00-inventory.md` reviewed (G0)
- [ ] Reference module **scaffolded from kit** builds in monolith solution
- [ ] Reference module registers in host without breaking existing startup
- [ ] One smoke characterization test green in CI (from kit template)
- [ ] Platform logging props adopted on reference module + its tests
- [ ] PR template + agent rules committed
- [ ] `scaffold-module.sh` verified (dry run → `Import` test tree builds)

---

## Phase B — Analysis gates (docs only)

Run **sequentially** in the monolith repo. Do not extract code until G1 + G2 pass.

| Step | Output | Gate |
|------|--------|------|
| Phase 1 | `01-entry-points.md` | — |
| Phase 2 | `02-bounded-context-map.md` + Mermaid | **G1** domain + engineering |
| Phase 3 | `contexts/<slug>/use-cases.yaml` (pilot contexts only first) | **G2** domain expert per P0 |
| Phase 4 | `contexts/<slug>/test-plan.md` + `test-cases.yaml` | **G3** fixtures agreed |
| Phase 5 | `03-modularization-roadmap.md` (monolith-specific slice order) | **G4** first slice chosen |

Use prompts from `copilot-analysis-instructions.md` — one phase per Claude session.

**Scope control:** Phase 3/4 for **pilot contexts only** on first pass. Expand to all ~7 contexts after the pilot loop works.

---

## Phase C & D — Two pilot modules (prove the process)

Pick **two pilots** that exercise **different seam types**. One slice per PR; characterization tests green before and after every merge.

### Pilot selection criteria

Score candidate contexts (1–5 each):

| Criterion | Weight | Good signal |
|-----------|--------|-------------|
| Low coupling | High | Few cross-context DB writes; minimal change handlers |
| Existing tests | High | Unit/integration coverage you can characterize |
| Business clarity | Medium | Domain expert available; P0 flows documented |
| SandBox reference | Medium | POC exists (import, logging, actor host) |
| Customer visibility | Low for pilot | Prefer **internal** slice first; avoid revenue-critical until loop proven |

**Avoid as first pilot:** `Floor2PlanDbContext` core aggregates (Activity, Assignment, Planning), SaveChanges handler chains, full UI rewrite.

### Recommended pilot pair for F2P

Based on inventory (`00-inventory.md`) and SandBox POCs:

#### Pilot 1 — **Import / Sync kernel** (vertical slice, same repo)

| Aspect | Detail |
|--------|--------|
| **Why** | `Application.Sync` is already a seam; Hangfire `sync` queue; SandBox has `ImportPipeline.Domain`, `ApiImportActorPoc`; strangler-friendly (route one import type to new path) |
| **Scope (slice 1)** | One import job end-to-end — e.g. **Discipline domain model Excel** or **Organisation import** (pick smallest with tests) |
| **Not in slice 1** | All 20+ import jobs, Aspose/XER, API `ImportController`, full actor migration |
| **Entry points** | One `Import*Job` + provider + reader from Phase 1 catalog |
| **Target projects** | `Import.Domain`, `Import.Application`, `Import.Infrastructure`, `Import.Api` |
| **Adapter** | `[StranglerAdapter]` job wrapper; legacy Hangfire registration delegates when flag off |
| **Proof** | Golden file in → same DB rows as legacy path; sync log parity |

#### Pilot 2 — **Integration pack OR read-heavy context** (different seam)

Choose **one** based on immediate pain:

| Option | Seam proved | Suggested scope |
|--------|-------------|-----------------|
| **A. PLM → Planning pack** (recommended) | External integration pack; canonical exchange model; no EF in pack | Inbound PLM structure file → intermediate JSON → planning port (see `floor2plan-v2-connector-prompt-plm-planning.md`) |
| **B. Runtime Configuration** | Small separate `ConfigurationDbContext`; low coupling | `AppSetting` read/write via new module; legacy `IAppSettings` adapter |
| **C. Reporting read model** | Separate `ReportingDbContext`; mostly reads | Report resource CRUD slice; no Telerik designer yet |

**Recommendation:** Pilot 1 = Import slice; Pilot 2 = **PLM pack** if integrations are the strategic driver, else **Configuration** if you want lowest risk.

### Per-pilot workflow (repeat twice)

```text
1. Phase 3 use cases for pilot context only        → G2
2. Implement 5–10 P0 characterization tests       → G3
3. Phase 5 slice plan for pilot (1–3 slices max)  → G4
4. Slice 1 PR: extract without behaviour change
5. Slice 2 PR (optional): route traffic / enable flag
6. Pilot sign-off: parity + domain expert review  → G6 for that slice
7. Retrospective: update templates, CI, agent rules
```

### Pilot exit criteria

- [ ] Two modules exist with Domain / Application / Infrastructure / Api layout
- [ ] ≥5 characterization tests per pilot; all green on `main`
- [ ] At least one `[StranglerAdapter]` with documented removal ticket
- [ ] ADO module dashboard shows P0 pass rate (see `azure-devops-module-test-dashboards.md`)
- [ ] No new cross-module Domain references
- [ ] Team agrees second context can reuse the same playbook without template changes

---

## Phase E — Scale (after pilots)

Only after both pilots pass:

1. Run Phase 3/4 for remaining bounded contexts (from G1 map).
2. Order extractions by roadmap: low coupling → P0 tests green → shared-table resolution.
3. Retire SaveChanges handlers **per entity family**, not per handler file.
4. Split `Floor2PlanDbContext` only when every entity in a group has a single context owner.
5. Frontend: Nx library per context; lazy routes — **after** backend API for that context is stable.
6. Sunset adapters on published dates (avoid "two systems forever").

---

## Monolith-specific constraints (from inventory)

Keep these visible in every Claude session:

| Constraint | Implication |
|------------|-------------|
| ABP + `DisableTransitiveProjectReferences=true` | Explicit project refs at every layer; module `.csproj` must list all deps |
| 55+ EF change handlers on `Floor2PlanDbContext` | Do not extract entities still wired to handlers until handler strategy defined |
| Hangfire `default` + `sync` queues | Import pilot uses `sync`; don't merge queue semantics early |
| Multiple DbContexts already | Reporting, Files, Auth, etc. — align module cuts with existing DB boundaries where possible |
| Hybrid UI (Razor + Vue + API) | Backend-first; UI strangler per screen family later |
| Tests are the spec | Characterize before refactor; `[UNDOCUMENTED]` blocks implementation |

---

## Claude session playbook (monolith repo)

Copy the **session header** into every Claude/Cursor chat:

```text
CONTEXT: Floor2Plan monolith modularization (strangler fig).
RULES: docs/modularization/README.md + ai-assisted-delivery-quality-framework.md
MODE: [analysis | characterization-tests | extraction-slice-N]
BOUNDED CONTEXT: [name]
SLICE ID: [from 03-modularization-roadmap.md]

Non-negotiable:
- Cite file:line for every claim; [NEEDS REVIEW] if uncertain
- No behaviour change without test; no test change without AC linkage
- One slice per PR; legacy must not reference new modules
- No vendor types in Domain; no cross-context DbContext writes in target design

IMPLEMENTATION QUALITY (when coding):
1. Read .cursor/rules and repo skills before writing code
2. Match neighbouring file style; dotnet build + dotnet test before done
3. Fix analyzer warnings in touched files
4. Link changes to UC-### / AC-### / slice_id
5. Tag adapters [StranglerAdapter] with removal ticket
```

### Session types

| When | Prompt focus |
|------|--------------|
| Starting program | Phase 0 inventory prompt (A1) |
| After inventory | Phase 1 → 2 → validate workshop |
| Picking pilots | "Score contexts by pilot criteria table in foundation-and-pilot-plan.md" |
| Before coding | Phase 4 test plan; implement GAP tests first |
| Extraction PR | "Slice `<slice_id>` only; characterization before/after; list files moved" |
| Integration pack | `claude-external-integrations-deepdive-instructions.md` Phase C |
| Coupling check | "Find all references from ContextA to ContextB; classify direct DB / service / message" |

---

## What to copy into the monolith repo

Minimum bootstrap (one-time):

```bash
# From SandBox repo — adjust paths to your clone layout
MONO=/path/to/Floor2Plan.Core2
SB=/path/to/SandBox

# Analysis + agent docs
mkdir -p "$MONO/docs/modularization/templates"
cp "$SB/docs/monolith-modularization/foundation-and-pilot-plan.md"     "$MONO/docs/modularization/"
cp "$SB/docs/monolith-modularization/copilot-analysis-instructions.md" "$MONO/docs/modularization/"
cp "$SB/docs/monolith-modularization/ai-assisted-delivery-quality-framework.md" "$MONO/docs/modularization/"
cp "$SB/docs/monolith-modularization/copilot-instructions-snippet.md" "$MONO/docs/modularization/"
cp "$SB/docs/monolith-modularization/templates/"*                      "$MONO/docs/modularization/templates/"
cp -R "$SB/docs/monolith-modularization/starter-kit/"*                   "$MONO/docs/modularization/starter-kit/"
cp "$SB/docs/floor2plan-v2-connector-architecture.md"                  "$MONO/docs/modularization/"

# Module refactor starter kit (Phase A)
mkdir -p "$MONO/Build/Platform" "$MONO/scripts"
cp "$SB/build/Platform.Logging."*.props                                 "$MONO/Build/Platform/"
cp "$SB/scripts/scaffold-module.sh"                                     "$MONO/scripts/" 2>/dev/null || true
cp "$SB/scripts/add-platform-logging-to-module.sh"                      "$MONO/scripts/"
cp -R "$SB/docs/monolith-modularization/starter-kit/templates/"*         "$MONO/Src/Modules/_template/" 2>/dev/null || true

# NuGet: point monolith at internal feed for Platform.Serilog.Logging packages
# (configure nuget.config — do not project-reference SandBox)
```

Optional: symlink SandBox POC folders for local reading — **do not** submodule SandBox into production deployment paths.

---

## Anti-patterns (stop if you see these)

| Anti-pattern | Instead |
|--------------|---------|
| Big-bang `Floor2PlanDbContext` split | Entity groups after context ownership proven |
| Extract by folder without context map | Complete Phase 2 + G1 first |
| Idealized tests | Characterize actual legacy output |
| Multiple slices in one PR | One strangler slice; smaller is safer |
| New features on legacy stack during pilot | Freeze scope; only extraction + parity |
| Skip foundation to "move faster" | Reference module + smoke test first |
| AI-authored business rules | Expert confirms or `[UNDOCUMENTED]` |

---

## Success metrics (track from pilot onward)

| Metric | Target |
|--------|--------|
| P0 ACs with test evidence | ↑ toward 100% for pilot contexts |
| Characterization tests per extracted module | ≥5 before first slice merge |
| Regressions post-merge in pilot | 0 |
| Median extraction PR size | <400 LOC excluding tests/migrations |
| Open `[NEEDS REVIEW]` > 2 sprints | ↓ |
| Adapters without sunset ticket | 0 |

---

## Suggested first sprint backlog

| # | Task | Owner | Gate |
|---|------|-------|------|
| 1 | Copy docs + **starter kit** + agent rules to monolith | Eng | — |
| 2 | Run Phase 0 inventory (refresh `00-inventory.md`) | AI + TL review | G0 |
| 3 | Phase 1 entry points | AI + TL | — |
| 4 | Phase 2 context map workshop | AI draft + domain/eng | G1 |
| 5 | Scaffold **Reference** module from starter kit | Eng | — |
| 6 | Smoke characterization test in CI (kit template) | Eng | — |
| 7 | Adopt Platform.Serilog.Logging on reference module | Eng | — |
| 8 | Verify `scaffold-module.sh Import` produces buildable tree | Eng | — |
| 9 | Phase 3/4 for Pilot 1 (Import) only | AI + domain | G2, G3 |
| 10 | Implement 5 characterization tests for import slice | Eng | G3 |
| 11 | Slice 1 extraction PR (Import) — scaffold from kit | AI + review | G5 |

Second sprint: Pilot 1 slice 2 (routing/flag) + begin Pilot 2 analysis.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-21 | Foundation + dual-pilot plan for external F2P monolith |
| 1.1 | 2026-06-21 | Added required module refactor starter kit (Phase A7) |
