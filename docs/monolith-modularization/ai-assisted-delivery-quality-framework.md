# AI-Assisted Delivery — Quality Framework

**Purpose:** Give management and engineering a shared, enforceable answer to: *How do we prevent nonsensical AI output and ensure new code is high quality, correct, and follows our guidelines?*

**Audience:** Management, tech leads, domain experts, engineers using AI assistants (GitHub Copilot, Claude Code, etc.) on the modularization program.

**Scope:** Analysis artifacts (story maps, use cases, integration catalogs) **and** implementation (module extraction, integration packs, new APIs).

---

## Executive summary (for management)

AI is used as a **drafting and search accelerator**, not as an authorizer of truth or behaviour.

| Layer | What stops "AI slop" |
|-------|----------------------|
| **Evidence** | Every claim must cite a file path, test method, or expert confirmation — otherwise marked `[NEEDS REVIEW]` and blocked from implementation |
| **Tests** | Behaviour is locked by **characterization** and **integration** tests *before* legacy code is moved; green tests are the merge gate |
| **Humans** | Domain experts validate story maps; tech leads validate context maps; PR reviewers reject uncited or untested changes |
| **Automation** | CI runs `dotnet build`, analyzers, and full test suite; PRs cannot merge on red |

**No AI-generated analysis proceeds to code without passing human gates G1–G3 and IG2** (see consolidated gates below).

**No AI-generated code merges without green tests on touched behaviour and adherence to coding standards enforced by analyzers + review.**

---

## Core principle: draft → validate → test → implement

```text
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ AI drafts    │ →  │ Human gates  │ →  │ Tests first  │ →  │ Small PR +   │
│ (cited only) │    │ (experts/TL) │    │ (char./int.) │    │ CI green     │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
```

AI must **never** skip the middle two stages. Implementation prompts must reference approved artifact IDs (`US-###`, `UC-###`, `INT-###`, `AC-###`).

---

## What AI is allowed to do

| Activity | Allowed | Output quality rule |
|----------|---------|---------------------|
| Repo inventory & search | Yes | Every row cites `path:line` or `class.method` |
| Reverse-engineer stories from tests | Yes | Every AC cites `project/class/method` or goes in `gaps` |
| Propose architecture | Yes | Label `DRAFT`; no code until gate passed |
| Write **new** tests for gaps | Yes, human-reviewed | Tests assert observable behaviour, not AI narrative |
| Extract/move code (strangler slice) | Yes, gated | Characterization tests green before **and** after |
| Invent business rules | **No** | Use `[UNDOCUMENTED]` / `[NEEDS REVIEW]` |
| "Improve" legacy behaviour silently | **No** | Requires explicit bug ticket + test change |
| Bulk codegen without linked UC/AC | **No** | Reject in PR review |

---

## Anti-slop checklist — reject if any apply

Use this in workshops and PR review to filter AI output quickly.

### Analysis artifacts (docs, YAML, story maps)

- [ ] **No citation** — narrative without file path or test reference
- [ ] **Invented actor or flow** — not in code, tests, or expert confirmation
- [ ] **Generic filler** — could apply to any ERP; no F2P-specific entities (WBS, Activity, Hour, ExternalId, tenant)
- [ ] **Confident vagueness** — "handles errors appropriately" without concrete outcome
- [ ] **Duplicate/conflicting** — same entity led by two systems with no `bidirectional` + conflict rule
- [ ] **Unbounded scope** — epic with 50+ stories in first pass (cap per phase instructions)
- [ ] **POC mistaken for production** — SandBox POC behaviour cited as legacy truth without `reference_only`

### Code PRs

- [ ] **No linked artifact** — PR does not reference `UC-###`, `US-###`, or strangler slice ID
- [ ] **No test change** on behaviour change (unless documented `known_quirk` preservation)
- [ ] **Analyzer warnings** introduced in touched files
- [ ] **Style drift** — does not match neighbouring files and workspace rules
- [ ] **Cross-context leakage** — vendor types in Domain, cross-context DbContext writes
- [ ] **Large PR** — multiple slices or unrelated contexts in one change

**Action:** Return to author with specific fixes. Do not "fix slop" in review without sending back for proper test/artifact linkage.

---

## Consolidated quality gates

### Analysis & domain validation

| Gate | When | Owner | Pass criteria |
|------|------|-------|---------------|
| G0 | After Phase 0 inventory | Tech lead | Every solution, DbContext, test project listed |
| G1 | After bounded context map | Domain + engineering | No duplicate aggregate owners; shared tables flagged |
| G2 | After P0 use cases / stories | Domain expert | P0 stories/ACs **confirmed** or **disputed with owner** |
| IG2 | After integration story map (Phase C) | Domain expert | Workshop complete; `story-map.yaml` status → `validated` |
| G3 | After test plan | Tech lead | Fixtures strategy agreed; top 10 gap tests scheduled |

### Implementation

| Gate | When | Owner | Pass criteria |
|------|------|-------|---------------|
| G4 | Before first slice | Tech lead | Slice scope + prerequisite tests listed |
| G5 | Every merge to main | CI + reviewer | All tests green; analyzers clean on touched code |
| G6 | Before retiring legacy path | Tech lead + domain | Parity tests + pilot sign-off |

**Hard rule:** Gates G2 / IG2 and G5 are non-negotiable for production-bound work.

---

## Definition of Done

### Analysis artifact (story map, use case, integration record)

- [ ] All claims cite `file:line` or `TestClass.TestMethod` or expert `confirmed` flag
- [ ] Unknowns explicitly tagged `[NEEDS REVIEW]` / `[UNDOCUMENTED]` — not omitted
- [ ] Linked IDs: `INT-###` ↔ `US-###` ↔ `UC-###` where applicable
- [ ] Reviewed by named human (`reviewed_by` in YAML metadata)
- [ ] No implementation PR opened until status `reviewed` or `validated`

### Code PR (module extraction, integration pack, API)

- [ ] Links to approved `UC-###` / strangler `slice_id` in PR description
- [ ] `dotnet build` clean (warnings-as-errors if repo policy applies)
- [ ] `dotnet test` green for affected projects
- [ ] New/changed behaviour covered by characterization or integration test
- [ ] Characterization tests pass **before and after** move (same commit or two-step PR with proof)
- [ ] Coding guidelines satisfied (see below)
- [ ] One strangler slice per PR
- [ ] Adapters tagged `[StranglerAdapter]` with removal ticket
- [ ] Human reviewer approval — AI does not satisfy this gate

---

## Coding guidelines enforcement

AI and human authors follow the **same** standards. Guidelines are not optional "suggestions."

### Binding sources (apply in order)

1. **Repo-local rules** — agent config and rules in the workspace (e.g. `docs/modularization/agent-rules.md`, `.github/copilot-instructions.md`, `.cursor/rules/*.mdc` where used)
2. **Skills** — `.cursor/skills/` / `.github/skills/` (`dotnet-core-csharp-development`, `dotnet-ef-core`, `akka-net`)
3. **Solution style guides** — e.g. `AkkaSignalRVuePoc/STYLEGUIDE.md`
4. **Neighbouring code** — match patterns in the module being edited
5. **MSBuild / analyzers** — `Directory.Build.props`, `EnforceCodeStyleInBuild`, `AnalysisLevel`

### Minimum C# conventions (Platform 2.0 / POCs)

| Area | Rule |
|------|------|
| Layout | One public type per file; file-scoped namespaces; `sealed` by default |
| Naming | PascalCase types; `_camelCase` private fields; `Async` suffix |
| API | Minimal APIs in static `*Endpoints` classes; DTOs as records where appropriate |
| DI | Constructor injection; explicit lifetimes; `Add<Context>Module` extension methods; no service locator; **no ABP in new modules** |
| Domain | No vendor-specific types in `*.Domain`; ports in `*.Application` |
| Actors | Messages/events in Contracts; `Tell`/`Forward` inside actors; `Ask` only at boundaries |
| EF | One DbContext per bounded context; review migrations before commit |
| Tests | xUnit; descriptive `Method_Scenario_Expected` names; assert behaviour not internals |
| **Logging** | `Platform.Serilog.Logging`: Development → Seq, Production → Application Insights; tests → `Platform.Serilog.Logging.Testing` (xUnit sink). See `03-modularization-roadmap.md`. |

### AI agent instruction (paste for implementation work)

```text
IMPLEMENTATION QUALITY (mandatory):

1. Read and follow docs/modularization/agent-rules.md (or .github/copilot-instructions.md) and repo skills before writing code.
2. Match style of files you edit; run dotnet build and dotnet test before finishing.
3. Fix all analyzer warnings in touched files.
4. Link every change to UC-### or US-### / AC-### from approved artifacts.
5. No behaviour change without a test; no test change without explaining which AC it serves.
6. One strangler slice per PR; no drive-by refactors.
7. If uncertain about a business rule, stop — do not invent. Mark [NEEDS REVIEW].
8. Module registration: IServiceCollection Add*Module + WebApplication Map*Module only — no AbpModule (module-composition-di.md).
```

---

## Tests: how we know it works as intended

### Test pyramid for this program

```text
                    ┌─────────────────────┐
                    │  Contract tests     │  module public API
                    ├─────────────────────┤
                    │  Integration tests  │  P0/P1 use cases, DB, messaging
                    ├─────────────────────┤
                    │  Characterization   │  legacy behaviour lock (golden master)
                    ├─────────────────────┤
                    │  Unit tests         │  pure domain rules
                    └─────────────────────┘
```

| Test type | Proves | When required |
|-----------|--------|---------------|
| **Characterization** | Legacy output unchanged | Before every extraction PR |
| **Integration** | Use case / AC end-to-end | P0/P1 features; story-map ACs |
| **Golden file** | Converter → intermediate format | Every integration pack inbound path |
| **Contract** | Module boundary stable | After `Add<Context>Module` extraction |

### Story map → test traceability

Each **confirmed** AC from Phase C should map to at least one test:

```yaml
# In test-cases.yaml or story-map.yaml
traceability:
  ac_id: AC-IMPORT-001-02
  test_name: Import_SamePlmFileTwice_UpdatesByExternalId
  type: characterization
  blocks_merge_if_red: true
```

Gap ACs (expert-confirmed, no test yet) go on the **sprint 1 test backlog** — implementation of that behaviour is blocked until a test exists or expert explicitly accepts interim risk with ticket.

### Module dashboards (Azure DevOps)

Each bounded-context module should expose a **simple ADO dashboard** with latest automated test results — filtered by `testRunTitle: 'Module: <Name>'` and test traits (`Module`, `Tier`, `UC`, `TC`, `AC`).

See **`azure-devops-module-test-dashboards.md`** for pipeline template, widget layout, and module DoD.

---

## CI/CD requirements (external monolith + Platform 2.0)

Recommend these as **merge blockers** on `main`:

```yaml
ci_required_checks:
  - dotnet build --configuration Release
  - dotnet test --no-build --configuration Release
  - analyzer_treat_warnings_as_errors: true   # on touched projects minimum
  optional_but_recommended:
  - dotnet format --verify-no-changes
  - architecture_tests: no Domain -> Infrastructure reference violations
  - pr_size_limit: 400 lines changed (excluding tests/migrations)
```

### PR template fields (enforce discipline)

```markdown
## Artifact linkage
- UC / US / AC IDs:
- Strangler slice:
- [ ] Characterization tests added or unchanged and green
- [ ] Domain expert confirmed (if behaviour-visible change)

## Quality
- [ ] dotnet build clean on touched projects
- [ ] dotnet test green
- [ ] Coding guidelines / analyzers satisfied
- [ ] No vendor types in Domain
```

---

## Roles and accountability

| Role | Responsibility |
|------|----------------|
| **Domain expert** | Confirms/disputes story map ACs; lead/follow; accepts residual risk for gap ACs |
| **Tech lead** | Approves context map, slice plan, merge to main |
| **Author (human or AI-assisted)** | Citations, tests, guidelines, small PRs |
| **Reviewer** | Anti-slop checklist; rejects uncited or untested work |
| **CI** | Objective build/test gate; no exceptions without tech lead + ticket |

AI has **no** approval role.

---

## Metrics management can track

| Metric | What it indicates | Healthy trend |
|--------|-------------------|---------------|
| % ACs with test evidence | Story map grounded in reality | ↑ |
| % ACs expert-confirmed vs draft | Workshop progress | ↑ confirmed |
| Characterization tests per extracted module | Refactoring safety | ≥ P0 coverage before slice |
| PR merge rate with test changes | Tests driving work | Stable; not zero |
| Analyzer warnings per PR | Slop / drift | ↓ or flat |
| Regressions post-merge in pilot | Gate effectiveness | → 0 |
| Median PR size (lines) | Slice discipline | Small, stable |
| `[NEEDS REVIEW]` items open > 2 sprints | Hidden uncertainty | ↓ |
| P0 pass rate per module (ADO dashboard) | Per-context health | Stable 100% on `main` |

---

## Relationship to other docs

| Document | Role |
|----------|------|
| `analysis-instructions.md` | Phased analysis; characterization-first |
| `external-integrations-deepdive-instructions.md` | Integration story maps; Phase C expert validation |
| `agent-instructions-snippet.md` | Short rules for AI agents in monolith repo |
| `03-modularization-roadmap.md` | SandBox POC cross-cutting standards (Serilog, NuGet import package, extraction order) |
| `azure-devops-module-test-dashboards.md` | Per-module test dashboards in Azure DevOps |
| `ApiImportActorPoc/docs/platform-rebuild-proposal-summary.md` | Strategic context; tests as specification |

---

## One-page policy (paste into wiki / slide)

> **AI drafts. Humans and tests decide.**
>
> 1. No code from AI without approved use cases / user stories and linked IDs.
> 2. No merge without green tests on affected behaviour.
> 3. No claim without a citation or expert confirmation.
> 4. One small strangler slice per PR.
> 5. Analyzers + coding guidelines apply to all code, AI-written or not.
> 6. Domain experts validate integration stories before we encode lead/follow in the model.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-18 | Initial quality framework for management + engineering |
