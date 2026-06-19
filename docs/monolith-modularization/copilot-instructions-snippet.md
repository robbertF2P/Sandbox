# Copilot workspace instructions (snippet)

Copy the block below into the monolith repo as `.github/copilot-instructions.md` or Cursor rules.

---

You are assisting with **monolith-to-modular migration** using a strangler-fig approach.

## Primary reference

Follow phases and output formats in `docs/monolith-modularization/copilot-analysis-instructions.md` (or the copy in this repo under `docs/modularization/`).

For **third-party external system integrations** (SAP, Kronos, PLM, lead vs follow, integration packs), use `docs/monolith-modularization/claude-external-integrations-deepdive-instructions.md` after Phase 0. Run against the **external F2P monolith repo**, not SandBox. Phase C produces epics, user stories, and ACs reverse-engineered from integration tests for domain expert validation.

## Non-negotiable rules

1. **Behaviour preservation first** — characterize legacy behaviour before refactoring.
2. **Bounded context ownership** — one aggregate owner; no cross-context DB writes in target design.
3. **Cite evidence** — every claim must reference file paths; mark unknowns `[NEEDS REVIEW]`.
4. **Phased work** — complete inventory and context map before use-case extraction.
5. **Test-gated extraction** — no module move without linked UC-/TC- IDs and green tests.
6. **No big-bang** — one strangler slice per change set.

## Target architecture

- Backend: composed ASP.NET gateway host, 7 bounded contexts as `IModule` libraries
- Frontend: Nx Angular shell, lazy-loaded context libraries
- Data: one DbContext per context; integration via events/contracts
- Cross-screen reads: BFF endpoints in gateway only when needed

## Default outputs

Write analysis artifacts to `docs/modularization/` using schemas in `templates/`.

## When asked to implement

- Prefer characterization tests through existing public entry points.
- Adapters may delegate to legacy; legacy must not depend on new modules.
- Tag temporary code `[StranglerAdapter]` with removal ticket.

## When uncertain

Stop and list specific questions. Do not invent business rules.
