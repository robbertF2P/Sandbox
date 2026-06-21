# Modularization agent instructions (snippet)

Copy the block below into the **monolith repo** so any AI assistant can pick it up:

| Tool | Suggested location |
|------|-------------------|
| GitHub Copilot | `.github/copilot-instructions.md` |
| Claude Code | `CLAUDE.md` or `docs/modularization/agent-rules.md` |
| Other assistants | `docs/modularization/agent-rules.md` |

Use the **same content** in each file you enable — one source of truth for Copilot, Claude, and humans.

---

You are assisting with **monolith-to-modular migration** using a strangler-fig approach.

## Primary reference

**Start here:** `docs/modularization/foundation-and-pilot-plan.md` — foundation-first sequence and dual-pilot strategy.

**Starter kit:** `docs/modularization/starter-kit/README.md` — copy in Phase A; scaffold every module from it.

**Module DI (no ABP):** `docs/modularization/module-composition-di.md` — `IServiceCollection` / `WebApplication` extension methods only; no `AbpModule` or new `Volo.Abp.*` in extracted modules.

Follow phased analysis in `docs/modularization/analysis-instructions.md`.

For **third-party integrations** (SAP, Kronos, PLM, lead vs follow, integration packs), use `docs/modularization/external-integrations-deepdive-instructions.md` after Phase 0. Run against the **external F2P monolith repo**, not SandBox.

For **team education** on legacy connector submodules vs integration packs: `docs/floor2plan-legacy-connector-submodule-antipattern.md`.

For a **concrete legacy → V2 pack proposal** on one connector:

- `docs/floor2plan-v2-connector-prompt-plm-planning.md` — **recommended first** (PLM structure → planning)
- `docs/floor2plan-v2-connector-prompt-eshare.md` — eShare (discovery-heavy)

Generic template: `docs/floor2plan-v2-connector-migration-prompt.md`

## Non-negotiable rules

1. **Behaviour preservation first** — characterize legacy behaviour before refactoring.
2. **Bounded context ownership** — one aggregate owner; no cross-context DB writes in target design.
3. **Cite evidence** — every claim must reference file paths; mark unknowns `[NEEDS REVIEW]`.
4. **Phased work** — complete inventory and context map before use-case extraction.
5. **Test-gated extraction** — no module move without linked UC-/TC- IDs and green tests.
6. **No big-bang** — one strangler slice per change set.
7. **Quality framework** — follow `docs/modularization/ai-assisted-delivery-quality-framework.md`.
8. **Module dashboards** — per-context ADO test results per `azure-devops-module-test-dashboards.md`.
9. **Serilog** — `Platform.Serilog.Logging` (Seq dev / App Insights prod); tests → `Platform.Serilog.Logging.Testing`.
10. **Module composition** — `Add<Context>Module` / `Map<Context>Endpoints`; **no ABP** in new modules.

## Target architecture

- Backend: composed ASP.NET host; bounded contexts as libraries with **`Add*Module` extension methods**
- Frontend: Nx Angular shell, lazy-loaded context libraries
- Data: one DbContext per context; integration via events/contracts
- Cross-screen reads: BFF endpoints in gateway only when needed

## Default outputs

Write analysis artifacts to `docs/modularization/` using schemas in `templates/`.

## When asked to implement

- Prefer characterization tests through existing public entry points.
- Register modules with `IServiceCollection.Add<Context>Module` and `WebApplication.Map<Context>Module` — no `AbpModule`.
- Adapters may delegate to legacy; legacy must not depend on new modules.
- Tag temporary code `[StranglerAdapter]` with removal ticket.

## When uncertain

Stop and list specific questions. Do not invent business rules.
