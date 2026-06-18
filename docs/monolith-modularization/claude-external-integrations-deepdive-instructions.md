# External System Integrations — Claude Deep-Dive Instructions

**Purpose:** Guide Claude through a systematic analysis of third-party external system integrations across Floor2Plan (F2P) bounded contexts. Correct poorly expressed domain requirements around *who leads* (system of record) vs *who follows* (consumer/replica), identify every integration point in the legacy monolith and target modules, and produce actionable design guidance aligned with the modularization program.

**Audience:** Engineers and domain experts revisiting integration boundaries during module extraction (post–Phase 0).

**Prerequisite:** Phase 0 inventory complete (`docs/modularization/00-inventory.md`). Bounded context map (`02-bounded-context-map.md`) strongly recommended.

**Outcome:** Per-context integration catalogs, a cross-context integration map, lead/follow classification, and target-state best practices for integration packs and the intermediate exchange format.

---

## How to use this document

1. Open the **monolith repository** (or workspace containing legacy + target modules) in Cursor with Claude.
2. Run **one phase at a time**. Do not skip Phase A (discovery sweep) or Phase C (lead/follow classification).
3. Store outputs under `docs/modularization/integrations/` (create if missing).
4. Human review is required after Phases B, C, and E before changing domain models or extracting integration code into packs.
5. Cross-reference `docs/monolith-modularization/copilot-analysis-instructions.md` for entry-point IDs (EP-###) and bounded context names.

### Relationship to modularization phases

| Modularization phase | This deep-dive phase | Notes |
|---------------------|----------------------|-------|
| Phase 0 — Inventory | Phase A | Reuse `00-inventory.md`; extend with integration-specific search |
| Phase 2 — Context map | Phase B | Attach integrations to each context's `external_dependencies` |
| Phase 3 — Use cases | Phase D | Integration flows become use cases or steps within use cases |
| Phase 5 — Module cuts | Phase E | Integration code placement drives pack vs core decisions |

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

## Phase C — Lead/follow classification workshop

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
@workspace EXTERNAL INTEGRATIONS — PHASE C ONLY: Lead/follow classification.

Inputs:
- docs/modularization/integrations/contexts/*/integrations.yaml
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
- docs/modularization/integrations/01-lead-follow-matrix.md
- docs/modularization/integrations/01-lead-follow-matrix.yaml
- docs/modularization/integrations/01-lead-follow.mermaid

Mark file with "DRAFT — REQUIRES DOMAIN VALIDATION" at top.
List top 10 [UNDOCUMENTED] cells as questions for domain experts.
```

### Human validation checklist

- [ ] Domain expert confirms system of record per entity for pilot tenants.
- [ ] Bidirectional flows have documented conflict resolution.
- [ ] No silent assumption that one vendor is globally "lead".
- [ ] Matrix covers hours/time (Kronos) and structure (PLM/SAP) separately.

---

## Phase D — Domain language and use-case linkage

### Goal

Express integrations in **ubiquitous language** and link to modularization use cases.

### Claude instruction

```text
@workspace EXTERNAL INTEGRATIONS — PHASE D: Domain language and use-case linkage.

Inputs:
- docs/modularization/integrations/contexts/*/integrations.yaml
- docs/modularization/integrations/01-lead-follow-matrix.md
- docs/modularization/contexts/*/use-cases.yaml (if Phase 3 exists)
- docs/monolith-modularization/templates/use-case.schema.yaml

Tasks:
1. For each integration point, write domain-friendly names:
   - Command examples: "Synchronize hours from Kronos", "Import WBS from PLM"
   - Avoid vendor jargon in core terms; vendor belongs in pack metadata.
2. Define ubiquitous language terms introduced by integrations:
   - External ID, System of Record, Integration Pack, Intermediate Format, Converter, Replay
3. Map integration points to use cases:
   - Existing UC-### (update cross_context_interactions / add steps)
   - New UC-### for integration-only flows (tier P0/P1 if revenue/critical)
4. Identify missing domain concepts currently buried in infrastructure:
   - e.g. idempotent upsert, dry-run import, validation report, sync cursor/watermark
5. Propose context-owned ports (interfaces) — not implementations:
   - IStructureImportSink, IHourSyncSource, IOutboundActualsPublisher, IExternalIdRegistry

Output:
- docs/modularization/integrations/02-domain-language.md
- docs/modularization/integrations/02-use-case-linkage.yaml

Do not implement ports yet. Cite code for every proposed port boundary.
```

---

## Phase E — Target architecture and best practices

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
@workspace EXTERNAL INTEGRATIONS — PHASE E: Target architecture and recommendations.

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
- docs/modularization/integrations/03-target-architecture.md
- docs/modularization/integrations/03-pack-roadmap.md
- docs/modularization/integrations/03-integration-context-map.mermaid

Include "Best practices checklist" section engineers can adopt per new integration.
```

---

## Phase F — Gap analysis and test plan

### Claude instruction

```text
@workspace EXTERNAL INTEGRATIONS — PHASE F: Gaps and test plan.

Inputs:
- docs/modularization/integrations/contexts/*/integrations.yaml
- Existing test projects from 00-inventory.md

Tasks:
1. For each P0/P1 integration point, list test GAPs.
2. Propose golden files per converter (anonymized samples if needed).
3. Rank top 10 tests to implement before extracting Import/Hours integration code.
4. Identify untestable areas (hardcoded paths, DateTime.Now, static HTTP, missing fixtures).

Output:
- docs/modularization/integrations/04-test-plan.md
- docs/modularization/integrations/04-test-cases.yaml

Align naming with docs/monolith-modularization/copilot-analysis-instructions.md Phase 4.
```

---

## Output artifact tree

```text
docs/modularization/integrations/
├── 00-raw-integration-candidates.md
├── 01-lead-follow-matrix.md
├── 01-lead-follow-matrix.yaml
├── 01-lead-follow.mermaid
├── 02-domain-language.md
├── 02-use-case-linkage.yaml
├── 03-target-architecture.md
├── 03-pack-roadmap.md
├── 03-integration-context-map.mermaid
├── 04-test-plan.md
├── 04-test-cases.yaml
└── contexts/
    ├── import/
    │   ├── integrations.md
    │   └── integrations.yaml
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
| IG2 | C | Lead/follow matrix validated by domain expert |
| IG3 | D | Ubiquitous language aligned with context map |
| IG4 | E | Pack placement agreed before moving integration code |
| IG5 | F | Golden-file strategy agreed for P0 integrations |

---

## Prompts for common follow-ups

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

Minimum viable deep-dive (after Phase 0):

1. **Phase A** — full repo sweep (half day + review)
2. **Phase B** — pilot contexts: **Import**, **Hours**, **WBS** (highest integration density)
3. **Phase C** — lead/follow workshop with domain expert
4. **Phase E** — pack roadmap for SAP, Kronos, PLM only
5. **Phase F** — 5 golden-file tests for Import pipeline

Expand to remaining contexts after pilot proves the artifact format.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-18 | Initial deep-dive instruction pack |
