# Monolith Modularization — Copilot Analysis Instructions

**Purpose:** Guide GitHub Copilot (Chat, Agent, or Workspace) through systematic analysis of a legacy monolith repository to extract bounded-context use cases and define a characterization test suite that enables safe refactoring into a composed modular architecture.

**Audience:** Engineers leading modularization; Copilot as analysis assistant.

**Outcome:** Per–bounded-context artifacts that support strangler-fig migration with test-gated parity.

---

## How to use this document

1. Open the **external monolith repository** in VS Code / Cursor with Copilot enabled.
2. Add this file to the repo (or reference it from your platform repo) as `.github/copilot-instructions.md` or paste sections into Copilot Chat.
3. Run **one phase at a time**. Do not skip Phase 0 (inventory) or Phase 2 (context map validation).
4. Store outputs under `docs/modularization/` in the monolith repo (create if missing).
5. Human review is required after Phases 2, 3, and 4 before writing tests or extracting modules.
6. All implementation must follow `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md`.

### Recommended Copilot mode

| Phase | Copilot mode | Why |
|-------|--------------|-----|
| 0–2 Discovery | Chat + `@workspace` | Broad repo search |
| 3 Use cases | Agent or Chat with file refs | Deep dive per area |
| 4 Test design | Chat | Structured output |
| 5 Module cut | Agent | Multi-file reasoning |

### Required inputs (fill before Phase 0)

```yaml
program:
  name: "<e.g. Platform 2.0 Modularization>"
  monolith_repo: "<url or local path>"
  target_topology: "composed-api-gateway + nx-angular-shell"
  bounded_context_count_target: 7   # adjust if unknown

known_contexts:                   # optional seed list; Copilot validates/refines
  - name: Orders
    aliases: [Order, SalesOrder]
  - name: Billing
    aliases: [Invoice, Payment]
  # ... add known names or leave empty for discovery

constraints:
  - "No big-bang rewrite; strangler-fig only"
  - "Parity must be proven before retiring legacy paths"
  - "One DbContext per bounded context in target state"
  - "No cross-context FK in target state"

tech_stack_hints:                 # fill what you know
  backend: ".NET Framework 4.x | .NET Core | mixed"
  frontend: "Razor | Angular | mixed"
  database: "SQL Server"
  messaging: "NServiceBus | Hangfire | none"
  ioc: "Castle Windsor | built-in DI"
```

---

## Phase 0 — Repository inventory

### Copilot instruction (copy/paste)

```
@workspace You are analyzing a legacy monolith for modularization.

PHASE 0 ONLY: Repository inventory. Do not propose architecture yet.

Tasks:
1. List all solutions (.sln), web entry points, console apps, and class library projects.
2. Map folder structure to logical layers (UI, API, Domain, Data, Infrastructure).
3. Identify integration points: HTTP APIs, message handlers, Hangfire jobs, scheduled tasks, file imports/exports.
4. List all DbContext / DataContext types and which tables/entities they map.
5. List authentication/authorization mechanisms and where enforced.
6. Find existing test projects and test types (unit, integration, UI).
7. Note build tooling, target frameworks, and deprecated patterns.

Output file: docs/modularization/00-inventory.md

Format: tables + bullet lists. Cite file paths for every claim.
Flag uncertainties as [NEEDS REVIEW].
Stop when inventory is complete.
```

### Acceptance criteria

- Every `.sln` and host project is listed.
- Every `DbContext` is named with path and entity count estimate.
- Test projects are mapped to production projects.
- No use-case or refactoring recommendations in this phase.

---

## Phase 1 — Entry-point and trigger catalog

### Goal

Find **how work enters the system** — the anchors for use-case extraction.

### Copilot instruction

```
@workspace PHASE 1 ONLY: Entry-point catalog.

Using docs/modularization/00-inventory.md as context, catalog every way work is triggered:

Categories to search:
- ASP.NET controllers / minimal API endpoints / Web API routes
- MVC actions returning JSON
- Message bus handlers (IHandleMessages, INotificationHandler, consumers)
- Hangfire / Quartz / IHostedService jobs
- SaveChanges interceptors / EF triggers / workflow handlers
- CLI commands and import pipelines
- SignalR hubs
- Razor page postbacks that mutate state

For each entry point record:
| Field | Description |
|-------|-------------|
| id | EP-### stable ID |
| type | HTTP | Message | Job | Interceptor | CLI | Other |
| location | file:line or class.method |
| route_or_trigger | URL, message type, cron, event |
| short_description | one line |
| mutates_state | yes/no |
| primary_tables | tables touched (best effort) |
| existing_tests | test classes covering this |

Output: docs/modularization/01-entry-points.md
Group by suspected domain area. Cite code. Mark [NEEDS REVIEW] where unclear.
```

---

## Phase 2 — Bounded context map

### Goal

Partition the monolith into contexts with explicit boundaries — the foundation for use cases and module cuts.

### Discovery heuristics (tell Copilot to apply these)

1. **Noun clusters** — entities that change together (Order + OrderLine + OrderStatus).
2. **Verb ownership** — who "owns" PlacedOrder, IssuedInvoice, AllocatedStock.
3. **DbContext boundaries** — existing contexts may hint at seams (also note leaky shared contexts).
4. **Namespace / folder prefixes** — `Module.One`, `Billing.*`, etc.
5. **Team language** — comments, UI labels, ticket references.
6. **Integration events** — message types often align with context boundaries.
7. **Transactional seams** — operations that do NOT share a single DB transaction today.

### Copilot instruction

```
@workspace PHASE 2 ONLY: Bounded context map.

Inputs:
- docs/modularization/00-inventory.md
- docs/modularization/01-entry-points.md
- known_contexts from program config (if any)

Tasks:
1. Propose bounded contexts (target ≤7 unless evidence requires more; merge or split with rationale).
2. For each context define:
   - name, description, ubiquitous language (5–15 terms)
   - owns (aggregates/entities)
   - does_not_own (explicit exclusions)
   - entry_points (EP-### IDs from Phase 1)
   - data_stores (tables, schemas, files)
   - external_dependencies (other contexts, third parties)
   - current_code_locations (projects, folders, namespaces)
3. Map cross-context relationships:
   - sync calls (A calls B service)
   - shared tables (anti-pattern; flag for removal)
   - messages/events
4. Produce a context map diagram (Mermaid).
5. List top 10 boundary violations (direct cross-context DB access, shared entities).

Output files:
- docs/modularization/02-bounded-context-map.md
- docs/modularization/02-context-map.mermaid

HUMAN GATE: Mark file with "DRAFT — REQUIRES HUMAN VALIDATION" at top.
Do not extract use cases until validated.
```

### Human validation checklist

- [ ] Each context has a clear owner verb set (commands it alone may issue).
- [ ] No two contexts own the same aggregate.
- [ ] Shared tables are documented with migration plan.
- [ ] Context count is manageable (≤7 or justified split).
- [ ] Business stakeholders confirm ubiquitous language.

---

## Phase 3 — Essential use case extraction (per bounded context)

### Goal

Extract **essential scenarios** — the minimum behavioural contract required before refactoring.

### Use case tiers

| Tier | Name | Purpose | Test priority |
|------|------|---------|---------------|
| P0 | Critical path | Revenue, compliance, data integrity | Characterization + integration first |
| P1 | Core domain | Frequent business operations | Integration tests |
| P2 | Supporting | Admin, reports, edge workflows | Selective coverage |
| P3 | Legacy/low use | Deprecated or rare | Document only; defer tests |

### Copilot instruction (run once per context)

```
@workspace PHASE 3: Use case extraction for bounded context "<CONTEXT_NAME>".

Inputs:
- docs/modularization/02-bounded-context-map.md
- docs/modularization/01-entry-points.md
- templates/use-case.schema.yaml

Tasks:
1. List all entry points owned by this context.
2. For each P0/P1 entry point, derive one or more use cases.
3. For each use case fill the full schema (see template).
4. Trace code path: entry → application service → domain → persistence → side effects.
5. Document business rules inline (if/else, validations, state transitions).
6. Document data variations (status enums, edge inputs, tenant overrides).
7. Link to existing tests or mark GAP.
8. Identify characterization test candidates (behaviour with no clear spec).

Output:
- docs/modularization/contexts/<context-slug>/use-cases.md
- docs/modularization/contexts/<context-slug>/use-cases.yaml

Rules:
- Use imperative names: "Place order", not "Order placement functionality".
- One use case = one user/system goal.
- Commands and queries separate (CQRS-friendly).
- Cite source files for every rule and state transition.
- Do not invent behaviour; mark [UNDOCUMENTED] if unclear.
- Max 15 P0/P1 use cases per context in first pass (expand later).

Stop after this context. Wait for human review before next context.
```

### Use case schema

See `templates/use-case.schema.yaml` in this folder. Each use case must include:

- **ID** — `UC-<CONTEXT>-###`
- **Trigger** — user action, message, schedule
- **Preconditions**
- **Steps** (domain language, not code)
- **Postconditions** (state + side effects)
- **Alternative flows / failures**
- **Owned data**
- **Cross-context calls** (must be explicit)
- **Test mapping** — existing / gap / characterization candidate

---

## Phase 4 — Test suite design (characterization-first)

### Goal

Turn use cases into a **refactoring safety net** before moving code.

### Test layers

```text
┌─────────────────────────────────────────┐
│  Contract tests (context public API)    │  ← future module boundary
├─────────────────────────────────────────┤
│  Integration tests (DB + messaging)     │  ← P0/P1 use cases
├─────────────────────────────────────────┤
│  Characterization tests (golden master) │  ← legacy behaviour lock
├─────────────────────────────────────────┤
│  Unit tests (domain rules)              │  ← pure logic
└─────────────────────────────────────────┘
```

### Characterization test

A test that captures **current** behaviour, not ideal behaviour:

- Given inputs / fixture
- Execute through **existing** entry point (HTTP, handler, service)
- Assert outputs, DB state, messages emitted
- When legacy is wrong but relied upon, document `known_quirk: true`

### Copilot instruction

```
@workspace PHASE 4: Test suite plan for bounded context "<CONTEXT_NAME>".

Inputs:
- docs/modularization/contexts/<context-slug>/use-cases.yaml
- Existing test projects from 00-inventory.md

Tasks:
1. For each P0 use case, specify:
   - test_type: characterization | integration | unit | contract
   - test_name convention
   - entry_point_under_test
   - arrange (fixtures, test data, mocks allowed only at external boundaries)
   - act
   - assert (response, DB tables, outbox/messages, side effects)
   - parallel_safe: yes/no
2. Identify shared test infrastructure needed (WebApplicationFactory, test DB, message sink).
3. List GAP tests to implement first (max 10 for sprint 1).
4. Propose test project layout aligned with target modules:
   - <Context>.Characterization.Tests
   - <Context>.Integration.Tests
5. Flag flaky or untestable areas (static singletons, DateTime.Now, hardcoded config).

Output:
- docs/modularization/contexts/<context-slug>/test-plan.md
- docs/modularization/contexts/<context-slug>/test-cases.yaml

Do not write production code changes in this phase unless asked.
Prefer testing through public entry points over testing private methods.
```

### Test naming convention

```text
<Context>_<UseCase>_<Scenario>_<ExpectedOutcome>

Examples:
Orders_PlaceOrder_ValidCart_CreatesOrderInSubmittedState
Billing_IssueInvoice_OrderShipped_CreatesInvoiceAndPostsEvent
```

---

## Phase 5 — Modularization slice plan

### Goal

Define **strangler slices** — ordered, test-gated extraction from monolith to module.

### Copilot instruction

```
@workspace PHASE 5: Modularization slice plan.

Inputs:
- All docs/modularization/contexts/*/use-cases.yaml
- docs/modularization/02-bounded-context-map.md

Tasks:
1. For each bounded context, propose extraction slices ordered by:
   - low coupling first
   - P0 tests already green
   - minimal cross-context shared tables
2. Per slice define:
   - slice_id
   - scope (namespaces, projects, routes moved)
   - prerequisite_tests (UC- IDs)
   - adapter_strategy (facade, strangler route, feature flag)
   - rollback_plan
   - done_criteria
3. Propose target project structure matching composed host pattern:
   - <Context>.Domain
   - <Context>.Application
   - <Context>.Infrastructure
   - <Context>.Api (`Add<Context>Module`, `Map<Context>Endpoints` — **not** ABP `AbpModule`)
4. List blockers (shared kernel, god services, shared DbContext).

Output: docs/modularization/03-modularization-roadmap.md

Order contexts in recommended migration sequence with rationale.
```

---

## Phase 6 — Copilot code generation guardrails

When Copilot moves from analysis to implementation, enforce:

```
IMPLEMENTATION GUARDRAILS (always apply):

1. No behaviour changes unless a test proves intentional fix.
2. Extract module without changing public API of the slice in the same PR.
3. One slice per PR; characterization tests must pass before and after.
4. New module may call legacy via adapter; legacy must not import new module.
5. Do not split DbContext until tables are assigned to a single context.
6. Cross-context calls become contracts + integration events, not project references.
7. Every extraction PR links to UC- IDs and test cases it satisfies.
8. Mark adapters with [StranglerAdapter] and ticket to remove.
9. New modules use IServiceCollection / WebApplication extension methods only — no Volo.Abp or AbpModule (see module-composition-di.md).
```

---

## Output artifact tree

```text
docs/modularization/
├── 00-inventory.md
├── 01-entry-points.md
├── 02-bounded-context-map.md
├── 02-context-map.mermaid
├── 03-modularization-roadmap.md
└── contexts/
    ├── orders/
    │   ├── use-cases.md
    │   ├── use-cases.yaml
    │   ├── test-plan.md
    │   └── test-cases.yaml
    ├── billing/
    └── ...
```

---

## Quality gates (do not proceed without)

| Gate | After phase | Requirement |
|------|-------------|-------------|
| G0 | 0 | Inventory reviewed by tech lead |
| G1 | 2 | Context map validated by domain + engineering |
| G2 | 3 | P0 use cases reviewed per context |
| G3 | 4 | Test plan approved; fixtures strategy agreed |
| G4 | 5 | First slice chosen; ≤10 GAP tests scheduled |
| G5 | Extract | Characterization tests green on main |

---

## Prompts for common follow-ups

### Find hidden coupling

```
@workspace Find all references from <ContextA> code to <ContextB> types, tables, or namespaces.
Classify: direct DB | service call | message | UI only.
Suggest decoupling approach per reference.
```

### Resolve shared table

```
@workspace Table <TableName> is used by contexts A and B.
Trace all reads/writes. Propose owner context and read model or event for the other.
```

### Prioritize characterization tests

```
@workspace From use-cases.yaml, rank P0 gaps by: change frequency, bug history, coupling, refactor blocker.
Return top 10 tests to implement this sprint with estimated complexity S/M/L.
```

### Angular UI alignment

```
@workspace For context <CONTEXT_NAME>, list UI routes, components, and API calls.
Map each to UC- IDs. Propose Nx library split matching backend context.
```

---

## Anti-patterns (instruct Copilot to avoid)

| Anti-pattern | Why harmful |
|--------------|-------------|
| Extracting by technical layer only | Preserves coupling; breaks by domain |
| Writing idealized tests | Miss legacy quirks; false confidence |
| Big-bang DbContext split | High risk; data corruption |
| Skipping characterization | Regressions during move |
| One giant BFF upfront | Hides context APIs; blocks parallel work |
| 100% coverage before first slice | Analysis paralysis |

---

## First sprint starter pack

Minimum viable first iteration:

1. Phase 0 + 1 (1–2 days human review)
2. Phase 2 draft + validation workshop
3. Phase 3 for **one pilot context** (smallest, well-tested)
4. Phase 4 — implement **5–10 characterization tests** for pilot P0 cases
5. Phase 5 — define **slice 1** only for pilot context

Prove the loop on one context before running Phase 3 across all seven.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-17 | Initial instruction pack |
