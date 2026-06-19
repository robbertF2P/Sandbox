# External System Integrations — Claude Deep-Dive Instructions

**Purpose:** Guide Claude through a systematic analysis of third-party external system integrations across Floor2Plan (F2P) bounded contexts. Correct poorly expressed domain requirements around *who leads* (system of record) vs *who follows* (consumer/replica), identify every integration point in the legacy monolith and target modules, and produce actionable design guidance aligned with the modularization program.

**Audience:** Engineers and domain experts revisiting integration boundaries during module extraction (post–Phase 0).

**Prerequisite:** Phase 0 inventory complete (`docs/modularization/00-inventory.md`). Bounded context map (`02-bounded-context-map.md`) strongly recommended.

**Outcome:** Per-context integration catalogs, a **reverse-engineered story map** (user stories + acceptance criteria + epics) for domain expert validation, lead/follow classification, and target-state best practices for integration packs and the intermediate exchange format.

---

## Important: analyze the external application repository

Claude must run Phases A–G against the **external Floor2Plan monolith repository** — the legacy production application — **not** this SandBox workspace.

This SandBox repo holds instruction packs, POCs (`ApiImportActorPoc`), and modularization templates only. The integration tests, submodules, Hangfire jobs, and vendor connectors live in the external repo.

```yaml
analysis_target:
  repo: "<path or url to external F2P monolith>"
  workspace_note: "Open the external repo as the Cursor workspace root before running any @workspace phase."
  sandbox_repo_role: "Copy finished artifacts back here under docs/modularization/integrations/ if desired."
```

If the external repo is unavailable, stop and list what is missing. Do not infer integration behaviour from SandBox POCs except as **target-state reference** (clearly label `reference_only: true`).

---

## How to use this document

1. Open the **external monolith repository** in Cursor with Claude.
2. Run **one phase at a time**. Do not skip Phase A (discovery sweep) or Phase C (story map from integration tests).
3. Store outputs under `docs/modularization/integrations/` in the **external repo** (create if missing).
4. Human review is required after Phases B, **C**, and F before changing domain models or extracting integration code into packs.
5. Cross-reference `docs/monolith-modularization/copilot-analysis-instructions.md` for entry-point IDs (EP-###) and bounded context names.

### Relationship to modularization phases

| Modularization phase | This deep-dive phase | Notes |
|---------------------|----------------------|-------|
| Phase 0 — Inventory | Phase A | Reuse `00-inventory.md`; extend with integration-specific search |
| Phase 2 — Context map | Phase B | Attach integrations to each context's `external_dependencies` |
| Phase 3 — Use cases | Phase C + E | Stories from tests (C); formal use-case linkage (E) |
| Domain validation | **Phase C** | Reverse-engineered story map for expert workshop |
| Phase 5 — Module cuts | Phase F | Integration code placement drives pack vs core decisions |

### Required inputs (fill before Phase A)

```yaml
program:
  name: "F2P Platform 2.0 — External Integrations Deep-Dive"
  monolith_repo: "<url or local path>"
  product_name: Floor2Plan
  product_alias: F2P

known_external_systems:          # seed list; Claude validates and expands
  - name: SAP
    domains: [projects, WBS, actuals, finance]
  - name: Kronos
    domains: [time, resources, hours]
  - name: PLM
    domains: [structure, components, activities, external_ids]
  - name: HR
    domains: [employees, org structure, calendars]
  - name: File
    domains: [csv, xml, excel imports/exports]

bounded_contexts:                # from 02-bounded-context-map.md or seed
  - Import
  - WBS
  - Planning
  - Hours
  - Resources
  - Reporting
  - Identity
  - Billing                    # if in scope

target_principles:
  - "Integration variance ships as versioned integration packs per tenant, not core forks"
  - "Inbound project/structure data targets one intermediate exchange format → one import pipeline"
  - "F2P core owns canonical domain, invariants, APIs, persistence boundaries, external ID registry"
  - "Packs own system-specific mapping, validation, enrichment, connectors"
  - "Lead vs follow must be explicit per entity type, not assumed globally"

reference_implementations:
  - path: ApiImportActorPoc/
    notes: "Canonical import with external IDs, idempotent upsert, dry-run"
  - doc: ApiImportActorPoc/docs/platform-rebuild-proposal-summary.md
    section: "Project portability & onboarding hub"
```

---

## Core concepts (instruct Claude to apply consistently)

### Lead vs follow

| Pattern | Meaning | Typical signals in code |
|---------|---------|-------------------------|
| **F2P leads** | F2P is system of record; external system receives updates | Outbound API clients, export jobs, webhooks, "push to SAP" handlers |
| **F2P follows** | External system is system of record; F2P imports/replicates | Import pipelines, submodules, file drops, scheduled sync *from* external |
| **Bidirectional** | Both systems mutate the same logical entity | Conflict resolution logic, "last write wins", manual merge UI, dual-write |
| **Read-only mirror** | F2P displays external data; no write-back | Cached reads, reporting joins, lookup tables refreshed on schedule |

**Critical rule:** Lead/follow is **per entity type** (e.g. PLM may own *structure* while F2P owns *planned dates*). Do not classify an entire vendor as always lead or always follow.

### Integration placement (target state)

```text
┌─────────────────────────────────────────────────────────────────┐
│  External systems (SAP, Kronos, PLM, HR, files, webhooks)        │
└────────────┬───────────────────────────────┬────────────────────┘
             │ converters / connectors      │ outbound adapters
             ▼ (integration pack)           ▼ (integration pack)
┌────────────────────────┐       ┌──────────────────────────────┐
│ Intermediate format    │       │ Outbound port implementations   │
│ (versioned JSON schema)│       │ (per enabled pack)              │
└────────────┬───────────┘       └──────────────┬───────────────┘
             │                                   │
             ▼                                   ▼
┌─────────────────────────────────────────────────────────────────┐
│  F2P Core — bounded context modules (Import, WBS, Planning, …)   │
│  · External ID registry · Domain invariants · Import API/actors  │
└─────────────────────────────────────────────────────────────────┘
```

### Anti-patterns to flag

| Anti-pattern | Why harmful |
|--------------|-------------|
| Git submodule per client/integration | Version matrix explosion; blocks module extraction |
| Integration logic inside SaveChanges handlers | Hidden orchestration; untestable; wrong bounded context |
| Cross-context DB writes during import | Violates context ownership; blocks DbContext split |
| Vendor types in core domain entities | Core fork per integration |
| Implicit lead/follow (undocumented) | Wrong conflict resolution; duplicate or lost data |
| One-off file formats per client | Cannot reuse import pipeline or golden-file tests |

---

## Phase A — Integration discovery sweep

### Claude instruction (copy/paste)

```text
@workspace EXTERNAL INTEGRATIONS — PHASE A ONLY: Discovery sweep.
Do not propose target architecture yet.

Prerequisites: docs/modularization/00-inventory.md (Phase 0 complete).

Tasks:
1. Search the repository for integration evidence:
   - HTTP clients, RestSharp, HttpClient wrappers named *Client, *Connector, *Integration
   - Import/export: *Import*, *Export*, file parsers (CSV, XML, IDoc, Excel)
   - Scheduled jobs: Hangfire, Quartz, IHostedService with sync/import/export in name or body
   - Message handlers consuming external payloads
   - Git submodules and client-specific integration folders
   - Configuration keys: SAP*, Kronos*, PLM*, HR*, External*, Integration*
   - External ID fields: ExternalId, EntityExternalId, SourceSystem, *PLM*, *SAP*
   - Comments/tickets referencing vendor names or "interface", "koppeling", "integratie"
2. For each hit, record a raw integration candidate (not yet classified).
3. Map each candidate to:
   - suspected bounded context
   - suspected external system
   - entry type (HTTP in/out, job, file, message, UI manual)
   - file path and symbol
4. Deduplicate: same integration may appear in handler + job + submodule.

Output: docs/modularization/integrations/00-raw-integration-candidates.md

Format: table with columns:
| id | external_system_guess | context_guess | trigger | location | evidence_snippet | confidence |

Mark low-confidence items [NEEDS REVIEW].
Cite file paths for every row. Stop when sweep is complete.
```

### Acceptance criteria

- All known seed systems (SAP, Kronos, PLM, HR, file) appear or are explicitly marked "not found in repo".
- Every submodule with integration purpose is listed.
- Hangfire/scheduled jobs with external I/O are included.
- No lead/follow or architecture recommendations in this phase.

---

## Phase B — Integration point catalog (per bounded context)

### Goal

Turn raw candidates into structured **integration points** owned by a bounded context.

### Claude instruction (run once per context)

```text
@workspace EXTERNAL INTEGRATIONS — PHASE B: Integration catalog for bounded context "<CONTEXT_NAME>".

Inputs:
- docs/modularization/integrations/00-raw-integration-candidates.md
- docs/modularization/02-bounded-context-map.md
- docs/monolith-modularization/templates/integration-point.schema.yaml

Tasks:
1. Filter raw candidates relevant to this context (owner or primary consumer).
2. For each, produce a full integration-point record per schema.
3. Trace code path: trigger → mapping → validation → persistence → side effects (events, rollups, notifications).
4. Document entity types exchanged (Project, WBS node, Activity, Hour, Resource, etc.).
5. Document external ID namespace if present (e.g. PLM, SAP).
6. Link to entry points (EP-###) from 01-entry-points.md where possible.
7. Link to existing tests or mark GAP.
8. Note tenant/client variance (per-client submodule, config flag, hardcoded client id).

Outputs:
- docs/modularization/integrations/contexts/<context-slug>/integrations.md
- docs/modularization/integrations/contexts/<context-slug>/integrations.yaml

Rules:
- Do not invent behaviour; mark [UNDOCUMENTED] if unclear.
- One integration point = one coherent flow with one external system and one primary direction.
- Split inbound and outbound into separate records if they differ in trigger, contract, or ownership.
- Max 20 records per context in first pass.

Stop after this context. Wait for human review before next context.
```

---

## Phase C — Domain expert validation: reverse-engineered story map

### Goal

Reverse-engineer a **user story mapping session** from existing **integration tests** in the external application. Produce epics, user stories, and acceptance criteria (ACs) rich enough for a domain expert workshop — without asking the expert to read code or test classes.

This phase is the primary **domain expert validation** gate. Experts confirm, dispute, or defer each AC and answer embedded lead/follow questions. Confirmed stories become the behavioural contract for modularization.

### Why integration tests first

In the external F2P application, years of integration behaviour are encoded in tests — often more reliably than comments or docs. Test names, arrange/act/assert blocks, fixtures, golden files, and mocked HTTP responses are the **primary source of truth** for this phase. Production code from Phase B corroborates; it does not override test-proven behaviour unless tests are clearly obsolete.

### Reverse-engineering heuristics (tell Claude to apply)

1. **Find integration test projects** — `*IntegrationTest*`, `*DataIntegrationTest*`, `*Integration.Tests*`, client-specific test folders, submodule test suites.
2. **Filter to integration-related tests** — names or bodies referencing SAP, Kronos, PLM, HR, Import, Export, Sync, ExternalId, converter, IDoc, file paths under `TestData/` or `Fixtures/`.
3. **Parse test structure:**
   - `MethodName_Scenario_ExpectedOutcome` → story + AC
   - `[Given]` / `[When]` / `[Then]` attributes (Gherkin-style) → AC directly
   - Arrange → preconditions; Act → trigger; Assert → postconditions
4. **Infer actors** from test setup: `PS operator`, `Scheduler`, `External system (PLM)`, `Tenant admin`, `End user`.
5. **Group stories into epics** by bounded context + external system + user goal (not by test class).
6. **Build story map backbone** (left → right): trigger → validate → transform → persist → notify/downstream.
7. **Flag gaps:** behaviour in Phase B catalog with no test; tests with no matching integration point.
8. **Do not invent ACs** without test evidence — use `gaps` and `untested_behaviour` sections instead.

### Claude instruction (run once per bounded context)

```text
@workspace EXTERNAL INTEGRATIONS — PHASE C: Reverse-engineered story map for "<CONTEXT_NAME>".

Target: external F2P monolith repository (NOT SandBox). Cite paths from the open workspace.

Inputs:
- docs/modularization/integrations/contexts/<context-slug>/integrations.yaml (Phase B)
- docs/modularization/02-bounded-context-map.md
- docs/monolith-modularization/templates/integration-story-map.schema.yaml
- Test project list from docs/modularization/00-inventory.md

Tasks:
1. Locate all integration tests owned by or exercising this bounded context.
2. For each relevant test method, extract one or more acceptance criteria:
   - Given / When / Then (or equivalent bullet form)
   - Cite test_evidence: project, class, method, file path
3. Cluster ACs into user stories (one user goal per story):
   - Use "As a <actor> I want <goal> So that <benefit>"
   - Imperative, domain language; vendor name only when actor is the external system
4. Cluster stories into epics bundled under this bounded context:
   - Epic = outcome a user cares about (e.g. "Onboard project from PLM", "Sync approved hours from Kronos")
   - Each epic lists external_systems, integration_points (INT-###), story_map_backbone
5. Attach lead_follow_questions per story (from Phase B + test assertions about who may edit/delete).
6. Mark every AC expert_validation.status = draft.
7. Produce a human-readable workshop pack (markdown) for domain experts — no code blocks required in the narrative section.
8. Produce machine-readable YAML per schema.

Outputs:
- docs/modularization/integrations/contexts/<context-slug>/story-map.md
- docs/modularization/integrations/contexts/<context-slug>/story-map.yaml

Rules:
- Every AC must cite at least one integration test OR be listed under gaps with reason.
- Prefer multiple ACs per story over one giant story.
- Max 8 epics, 40 stories, 120 ACs per context in first pass.
- Mark inferred behaviour [INFERRED FROM TEST NAME ONLY] if body not read.
- Mark [NEEDS REVIEW] if test is ignored, skipped, or commented out.

Stop after this context. Do not run lead/follow matrix yet — that is Phase D.
```

### Workshop pack format (`story-map.md`)

The markdown output must be **printable / shareable** for domain experts. Structure:

```markdown
# Story map — <Context name> integrations

> DRAFT — reverse-engineered from integration tests. For domain expert validation workshop.

## Epic: <Epic name> (EPIC-###)

**Outcome:** <one sentence>
**External systems:** PLM, SAP
**Integration points:** INT-IMPORT-001

### Story: <Title> (US-###)

As a **<actor>**, I want **<goal>** so that **<benefit>**.

| AC | Given | When | Then | Test evidence | Expert ✓ |
|----|-------|------|------|---------------|----------|
| AC-…-01 | … | … | … | PlmImportTests.Import_… | ☐ |

**Questions for expert:**
- …

**Gaps (no test):**
- …
```

### Rollup instruction (after all contexts)

```text
@workspace EXTERNAL INTEGRATIONS — PHASE C ROLLUP: Cross-context story map index.

Inputs: docs/modularization/integrations/contexts/*/story-map.yaml

Tasks:
1. Merge into docs/modularization/integrations/02-story-map-index.md
2. Summary table: context | epics | stories | ACs | gaps | disputed
3. List cross-context epics (e.g. "End-to-end onboarding: SAP → Import → Planning")
4. Mermaid story-map diagram: epics as swimlanes by bounded context

Output: docs/modularization/integrations/02-story-map-index.md
```

### Domain expert workshop agenda (facilitator)

| Step | Duration | Activity |
|------|----------|----------|
| 1 | 10 min | Walk through epic list per bounded context |
| 2 | 30 min | Per epic: confirm stories still reflect how PS/clients work today |
| 3 | 45 min | Per story: validate ACs — confirm / dispute / defer; capture notes in YAML |
| 4 | 20 min | Answer lead/follow questions; flag bidirectional flows |
| 5 | 15 min | Capture gaps: behaviour experts expect but no test exists |
| 6 | 10 min | Prioritize disputed items for follow-up test or code trace |

### Expert validation checklist

- [ ] Every P0 epic has at least one **confirmed** story.
- [ ] Disputed ACs have owner and follow-up action (fix test, fix code, or fix doc).
- [ ] Lead/follow questions answered for all P0/P1 stories touching external systems.
- [ ] Gaps logged for untested but business-critical flows.
- [ ] `story-map.yaml` metadata.status = `validated` only after workshop.

### Acceptance criteria (phase complete)

- Each pilot context (Import, Hours, WBS) has `story-map.md` + `story-map.yaml`.
- Every AC cites test evidence or appears in `gaps`.
- Rollup index exists with epic counts per bounded context.
- No lead/follow matrix in this phase (deferred to Phase D).

---

## Phase D — Lead/follow classification workshop

### Goal

Make **system of record** explicit per entity type — the requirement gap this deep-dive addresses.

### Classification heuristics (tell Claude to apply)

1. **Who creates the entity first?** Creator's system is usually lead for identity.
2. **Who can delete or rename?** Lead system typically owns lifecycle.
3. **Import-only code path** → F2P follows for that entity.
4. **Export/push/webhook code path** → F2P leads or bidirectional.
5. **Conflict/merge UI** → bidirectional or disputed; document resolution rule.
6. **Tests and golden files** often encode de facto lead/follow — search test fixtures for vendor prefixes.
7. **PS documentation / runbooks** in repo — search `docs/`, `README`, wiki exports.

### Claude instruction

```text
@workspace EXTERNAL INTEGRATIONS — PHASE D ONLY: Lead/follow classification.

Inputs:
- docs/modularization/integrations/contexts/*/integrations.yaml
- docs/modularization/integrations/contexts/*/story-map.yaml (Phase C — expert-validated where available)
- docs/modularization/02-bounded-context-map.md

Tasks:
1. Build a matrix:
   Rows: entity types (Project, Component, Activity, Assignment, Hour, Resource, Calendar, Actual, Invoice, …)
   Columns: external systems + F2P
   Cells: lead | follow | bidirectional | read_only | not_applicable | unknown
2. For each non-trivial cell, cite evidence (code, test, config) or mark [UNDOCUMENTED].
3. Flag contradictions: two leads for same entity+tenant, or code implying follow but UI allowing edit.
4. For each bounded context, summarize:
   - entities where F2P must never write back
   - entities where F2P is authoritative
   - entities requiring bidirectional sync (highest risk)
5. Produce Mermaid diagram: systems as nodes, directed edges labeled with entity types and lead/follow.

Outputs:
- docs/modularization/integrations/03-lead-follow-matrix.md
- docs/modularization/integrations/03-lead-follow-matrix.yaml
- docs/modularization/integrations/03-lead-follow.mermaid

Mark file with "DRAFT — REQUIRES DOMAIN VALIDATION" at top unless Phase C workshop marked stories validated.
Cross-reference disputed ACs from story-map.yaml.
List top 10 [UNDOCUMENTED] cells as questions for domain experts.
```

### Human validation checklist

- [ ] Domain expert confirms system of record per entity for pilot tenants.
- [ ] Lead/follow aligns with **confirmed** user stories from Phase C (flag mismatches).
- [ ] Bidirectional flows have documented conflict resolution.
- [ ] No silent assumption that one vendor is globally "lead".
- [ ] Matrix covers hours/time (Kronos) and structure (PLM/SAP) separately.

---

## Phase E — Domain language and use-case linkage

### Goal

Express integrations in **ubiquitous language** and link to modularization use cases.

### Claude instruction

```text
@workspace EXTERNAL INTEGRATIONS — PHASE E: Domain language and use-case linkage.

Inputs:
- docs/modularization/integrations/contexts/*/integrations.yaml
- docs/modularization/integrations/contexts/*/story-map.yaml
- docs/modularization/integrations/03-lead-follow-matrix.md
- docs/modularization/contexts/*/use-cases.yaml (if Phase 3 exists)
- docs/monolith-modularization/templates/use-case.schema.yaml

Tasks:
1. For each integration point, write domain-friendly names:
   - Command examples: "Synchronize hours from Kronos", "Import WBS from PLM"
   - Avoid vendor jargon in core terms; vendor belongs in pack metadata.
2. Define ubiquitous language terms introduced by integrations:
   - External ID, System of Record, Integration Pack, Intermediate Format, Converter, Replay
3. Map integration points to use cases:
   - Promote confirmed user stories (US-###) to UC-### where appropriate
   - Existing UC-### (update cross_context_interactions / add steps)
   - New UC-### for integration-only flows (tier P0/P1 if revenue/critical)
4. Identify missing domain concepts currently buried in infrastructure:
   - e.g. idempotent upsert, dry-run import, validation report, sync cursor/watermark
5. Propose context-owned ports (interfaces) — not implementations:
   - IStructureImportSink, IHourSyncSource, IOutboundActualsPublisher, IExternalIdRegistry

Output:
- docs/modularization/integrations/04-domain-language.md
- docs/modularization/integrations/04-use-case-linkage.yaml

Do not implement ports yet. Cite code for every proposed port boundary.
```

---

## Phase F — Target architecture and best practices

### Goal

Recommend where each integration lives in the **module + pack** target state.

### Best practices (instruct Claude to evaluate and apply)

#### 1. Bounded context owns the *port*; pack owns the *adapter*

- Core module defines `IHourSyncSource` in Hours.Application.
- `KronosIntegrationPack` implements it; enabled per tenant via pack registry.
- No `Kronos` types in Hours.Domain.

#### 2. Inbound structure/project data → intermediate format

- All converters (legacy export, SAP, PLM, Kronos structure) emit the **same versioned schema**.
- Import bounded context exposes one pipeline: validate → dry-run → persist.
- Reference: `ApiImportActorPoc` external ID registry and upsert semantics.

#### 3. External ID registry is core infrastructure

- `(system, external_value) → internal_entity_id` mapping lives in the owning context's persistence.
- Uniqueness rules per entity type are domain invariants (see `ExternalIdUniquenessValidator` in POC).
- Converters never write directly to WBS/Planning tables — only through import API.

#### 4. Lead/follow drives API shape

| Pattern | Preferred mechanism |
|---------|---------------------|
| F2P follows | Scheduled or triggered **import**; idempotent upsert; optional delete detection |
| F2P leads | **Outbox** + async publisher actor; retry with dead-letter; never block user transaction on external latency |
| Bidirectional | Split into two directed flows; explicit conflict policy; avoid dual-write in one transaction |
| Read-only mirror | CQRS read model refreshed by integration event or cache TTL |

#### 5. Failure isolation

- Integration failures must not roll back unrelated domain work.
- Per-entity error reporting for imports (POC pattern).
- Circuit breaker on outbound; manual replay for inbound files.

#### 6. Testing strategy

| Layer | What to test |
|-------|----------------|
| Golden file / characterization | Converter output → intermediate format; import → DB state |
| Contract | Pack adapter implements port; schema version negotiation |
| Integration | End-to-end with test container or recorded HTTP |
| Parity | Legacy behaviour vs new module for pilot tenant |

#### 7. Observability

- Correlation id: `tenant_id`, `integration_point_id`, `sync_batch_id` across logs and metrics.
- Admin surfacing: last success, last error, records processed (feeds backoffice runbook).

### Claude instruction

```text
@workspace EXTERNAL INTEGRATIONS — PHASE F: Target architecture and recommendations.

Inputs:
- All artifacts under docs/modularization/integrations/
- ApiImportActorPoc/docs/platform-rebuild-proposal-summary.md (Section 5 — intermediate format)
- docs/monolith-modularization/copilot-instructions-snippet.md (target topology)

Tasks:
1. For each integration point, recommend target placement:
   core | integration_pack | gateway_webhook | standalone_converter_tool
2. Propose integration pack groupings (e.g. sap-projects-v1, kronos-time-v1, plm-structure-v1).
3. Map legacy locations → target module + pack with strangler order (low coupling first).
4. List domain model changes needed to express lead/follow explicitly:
   - value objects, enums, entity flags, or separate sync metadata tables
5. Produce prioritized backlog:
   - P0: data integrity / compliance integrations
   - P1: frequent operational syncs
   - P2: reporting mirrors
6. Document cross-context integration events (past tense, per actor-system-contracts):
   - StructureImported, HoursSynchronized, ActualsExported, etc.
7. Flag blockers for module extraction: shared tables, handler chains, submodules.

Outputs:
- docs/modularization/integrations/05-target-architecture.md
- docs/modularization/integrations/05-pack-roadmap.md
- docs/modularization/integrations/05-integration-context-map.mermaid

Include "Best practices checklist" section engineers can adopt per new integration.
```

---

## Phase G — Gap analysis and test plan

### Claude instruction

```text
@workspace EXTERNAL INTEGRATIONS — PHASE G: Gaps and test plan.

Inputs:
- docs/modularization/integrations/contexts/*/integrations.yaml
- docs/modularization/integrations/contexts/*/story-map.yaml
- Existing test projects from 00-inventory.md

Tasks:
1. For each P0/P1 integration point, list test GAPs.
2. Cross-reference story-map gaps — prioritize tests experts flagged as missing.
3. Propose golden files per converter (anonymized samples if needed).
4. Rank top 10 tests to implement before extracting Import/Hours integration code.
5. Identify untestable areas (hardcoded paths, DateTime.Now, static HTTP, missing fixtures).

Output:
- docs/modularization/integrations/06-test-plan.md
- docs/modularization/integrations/06-test-cases.yaml

Align naming with docs/monolith-modularization/copilot-analysis-instructions.md Phase 4.
```

---

## Output artifact tree

```text
docs/modularization/integrations/
├── 00-raw-integration-candidates.md
├── 02-story-map-index.md              # Phase C rollup — epics by bounded context
├── 03-lead-follow-matrix.md
├── 03-lead-follow-matrix.yaml
├── 03-lead-follow.mermaid
├── 04-domain-language.md
├── 04-use-case-linkage.yaml
├── 05-target-architecture.md
├── 05-pack-roadmap.md
├── 05-integration-context-map.mermaid
├── 06-test-plan.md
├── 06-test-cases.yaml
└── contexts/
    ├── import/
    │   ├── integrations.md
    │   ├── integrations.yaml
    │   ├── story-map.md             # Domain expert workshop pack
    │   └── story-map.yaml
    ├── hours/
    ├── wbs/
    └── ...
```

---

## Quality gates

| Gate | After phase | Requirement |
|------|-------------|-------------|
| IG0 | A | Raw sweep reviewed; no major vendor missing |
| IG1 | B | Per-context catalogs reviewed by context owner |
| **IG2** | **C** | **Story map workshop complete; P0 ACs confirmed or disputed with owners** |
| IG3 | D | Lead/follow matrix validated; aligned with confirmed stories |
| IG4 | E | Ubiquitous language aligned with context map |
| IG5 | F | Pack placement agreed before moving integration code |
| IG6 | G | Golden-file strategy agreed for P0 integrations |

---

## Prompts for common follow-ups

### Run domain expert workshop from story map

```text
@workspace Using docs/modularization/integrations/contexts/<context>/story-map.md,
produce a facilitator script and a one-page summary per epic for a 2-hour workshop.
Include: confirmed/disputed/deferred checkboxes, lead/follow questions, gap list.
```

### Trace one vendor end-to-end

```text
@workspace Trace all code paths involving "<VENDOR>" (e.g. Kronos).
Classify each path: inbound/outbound, lead/follow, owning context.
Produce sequence diagram (Mermaid) for the most critical path.
```

### Resolve lead/follow conflict

```text
@workspace Entity type "<ENTITY>" appears led by both F2P and "<VENDOR>".
List all read/write locations, UI edit surfaces, and tests.
Recommend single system of record and migration steps.
```

### Design one integration pack

```text
@workspace Design integration pack "<pack-name>" for <VENDOR> <domain>.
Define: pack manifest, enabled ports, config schema, converter layout,
golden files, failure/replay, compatibility with intermediate format v1.
```

### Submodule elimination

```text
@workspace Submodule "<path>" implements client-specific integration.
Map behaviour to pack configuration vs code extension.
Propose deprecation path without losing tenant behaviour.
```

---

## Implementation guardrails (when moving from analysis to code)

```text
INTEGRATION IMPLEMENTATION GUARDRAILS:

1. No vendor-specific types in *.Domain projects.
2. Converters produce intermediate format; only Import context persists structure.
3. External IDs are immutable per (system, value) once assigned to an entity.
4. Lead/follow rules enforced in application layer, not only in UI.
5. Characterization tests must pass before replacing legacy import/sync path.
6. One integration pack per PR when possible; feature-flag per tenant.
7. Tag legacy adapters [StranglerAdapter] with removal ticket.
8. Ask at system boundary only; actors use Tell/Forward internally.
```

---

## First iteration starter pack

Minimum viable deep-dive (after Phase 0), in the **external monolith repo**:

1. **Phase A** — full repo sweep
2. **Phase B** — pilot contexts: **Import**, **Hours**, **WBS**
3. **Phase C** — reverse-engineer story maps from integration tests; **domain expert workshop**
4. **Phase D** — lead/follow matrix informed by validated stories
5. **Phase F** — pack roadmap for SAP, Kronos, PLM
6. **Phase G** — golden-file tests for gaps experts flagged

Expand to remaining contexts after pilot proves the artifact format.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-18 | Initial deep-dive instruction pack |
| 1.1 | 2026-06-18 | Phase C story map from integration tests; external repo scope; phases renumbered D–G |
