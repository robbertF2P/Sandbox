# Copilot workspace instructions (snippet)

Copy the block below into the monolith repo as `.github/copilot-instructions.md` or Cursor rules.

---

You are assisting with **monolith-to-modular migration** using a strangler-fig approach.

## Primary reference

**Start here:** `docs/monolith-modularization/foundation-and-pilot-plan.md` — foundation-first sequence and dual-pilot strategy for the external F2P monolith.

**Starter kit:** `docs/monolith-modularization/starter-kit/README.md` — copy into monolith Phase A; scaffold every module from it.

**Module DI (no ABP):** `docs/monolith-modularization/module-composition-di.md` — register modules with `IServiceCollection` / `WebApplication` extension methods only; do not use `AbpModule` or new `Volo.Abp.*` in extracted modules.

Follow phases and output formats in `docs/monolith-modularization/copilot-analysis-instructions.md` (or the copy in this repo under `docs/modularization/`).

For **third-party external system integrations** (SAP, Kronos, PLM, lead vs follow, integration packs), use `docs/monolith-modularization/claude-external-integrations-deepdive-instructions.md` after Phase 0. Run against the **external F2P monolith repo**, not SandBox. Phase C produces epics, user stories, and ACs reverse-engineered from integration tests for domain expert validation.

For **team education** on why legacy connectors use git submodules against core (and why Platform 2.0 uses integration packs), see `docs/floor2plan-legacy-connector-submodule-antipattern.md`.

For a **concrete legacy → V2 pack proposal** on one connector, use:

- `docs/floor2plan-v2-connector-prompt-plm-planning.md` — **recommended first** (PLM structure → planning)
- `docs/floor2plan-v2-connector-prompt-eshare.md` — eShare (discovery-heavy; not in SandBox)

Generic template: `docs/floor2plan-v2-connector-migration-prompt.md`

## Non-negotiable rules

1. **Behaviour preservation first** — characterize legacy behaviour before refactoring.
2. **Bounded context ownership** — one aggregate owner; no cross-context DB writes in target design.
3. **Cite evidence** — every claim must reference file paths; mark unknowns `[NEEDS REVIEW]`.
4. **Phased work** — complete inventory and context map before use-case extraction.
5. **Test-gated extraction** — no module move without linked UC-/TC- IDs and green tests.
6. **No big-bang** — one strangler slice per change set.
7. **Quality framework** — follow `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md` for anti-slop rules, DoD, and CI gates.
8. **Module dashboards** — each bounded context publishes ADO test results per `azure-devops-module-test-dashboards.md`.
9. **Serilog platform logging** — `Platform.Serilog.Logging`: Development → Seq, Production → Application Insights; all tests → `Platform.Serilog.Logging.Testing` (xUnit sink). See `03-modularization-roadmap.md`.
10. **Module composition** — `Add<Context>Module` / `Map<Context>Endpoints` via `IServiceCollection` and `WebApplication` extensions; **no ABP** in new modules (`module-composition-di.md`).

## Target architecture

- Backend: composed ASP.NET host; bounded contexts as libraries registered with **`Add*Module` extension methods** (not ABP modules)
- Frontend: Nx Angular shell, lazy-loaded context libraries
- Data: one DbContext per context; integration via events/contracts
- Cross-screen reads: BFF endpoints in gateway only when needed

## Default outputs

Write analysis artifacts to `docs/modularization/` using schemas in `templates/`.

## When asked to implement

- Prefer characterization tests through existing public entry points.
- Register modules with `IServiceCollection.Add<Context>Module` and `WebApplication.Map<Context>Module` — explicit DI only; no `AbpModule`.
- Adapters may delegate to legacy; legacy must not depend on new modules.
- Tag temporary code `[StranglerAdapter]` with removal ticket.

## When uncertain

Stop and list specific questions. Do not invent business rules.
