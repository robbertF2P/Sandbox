# Tenant workflow extension fields — deep-dive instructions

**Purpose:** Guide AI coding assistants (GitHub Copilot, Claude Code, and similar) through systematic analysis of legacy **generic extension fields** (`Text1`, `Text2`, `Text3`, …, `Bool1`, `Bool2`, …) used for tenant-specific workflows — filtering, list columns, detail screens, imports, reports, and business rules — and produce a **judgment + V2 migration proposal** aligned with Platform 2.0 customization packs and read-model standards.

**Audience:** Engineers and domain experts during strangler migration (post–Phase 0 inventory).

**Prerequisite:** Phase 0 inventory complete (`docs/modularization/00-inventory.md`). Bounded context map (`02-bounded-context-map.md`) strongly recommended.

**Outcome:** Per-entity field catalogs, per-tenant usage matrix, filter/UI traceability, workflow-rule classification, and a **V2 target design** (core vs extension vs computed; pack vs promote) with characterization test gaps.

---

## Important: analyze the external application repository

Run all phases against the **external Floor2Plan monolith repository** — the legacy production application — **not** this SandBox workspace.

SandBox holds instruction packs, POCs, and V2 standards (`platform-ui-customization-standard.md`, Hour Approvals Acme pack). Legacy `Text*` / `Bool*` columns, Razor/Vue screens, and tenant config live in the external repo.

```yaml
analysis_target:
  repo: "<path or url to external F2P monolith>"
  workspace_note: "Open the external repo as the workspace root before running any analysis phase."
  sandbox_repo_role: "Copy finished artifacts back here under docs/modularization/tenant-workflow-fields/ if desired."
```

If the external repo is unavailable, stop and list what is missing. Do not infer legacy behaviour from SandBox POCs except as **target-state reference** (label `reference_only: true`).

---

## How to use this document

1. Open the **external monolith repository** as the workspace root.
2. Run **one phase at a time**. Do not skip Phase A (discovery) or Phase C (tenant matrix).
3. Store outputs under `docs/modularization/tenant-workflow-fields/` in the **external repo** (create if missing).
4. Human review is required after Phases B, **C**, and **F** before changing domain models or shipping V2 packs.
5. Cross-reference `docs/modularization/analysis-instructions.md` for entry-point IDs (EP-###) and bounded context names.
6. Before any implementation PR, satisfy `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md`.

### Relationship to modularization phases

| Modularization phase | This deep-dive phase | Notes |
|---------------------|----------------------|-------|
| Phase 0 — Inventory | Phase A | Reuse `00-inventory.md`; extend with extension-field search |
| Phase 2 — Context map | Phase B | Attach fields to owning bounded context / aggregate |
| Phase 3 — Use cases | Phase D + E | Filters and workflow rules become UC-/AC- linkage |
| Domain validation | **Phase C** | Tenant usage matrix for expert workshop |
| Phase 5 — Module cuts | Phase F | Promotion vs pack vs strangler adapter |

### V2 target references (SandBox — read before Phase F)

| Document | Use for |
|----------|---------|
| `platform-ui-customization-standard.md` | View schemas, `extensions` / `computed` bags, customization packs |
| `platform-pack-blueprint.md` | Pack types, artifact catalog, `PACK.md` manifest, scaffold script |
| `docs/floor2plan-v2-read-model-playbook.md` | Specification-backed list filters, Nx schema-driven columns |
| `platform-actor-standard.md` | Tenant rules in actor pipelines (when fields drive orchestration) |
| `specification-pattern` skill | Encapsulate filter rules; no `IQueryable` leakage |

---

## Core concepts (instruct the agent to apply consistently)

### What legacy extension fields usually are

| Pattern | Typical legacy implementation | Risk if copied to V2 |
|---------|------------------------------|----------------------|
| **Sparse columns** | `Text1`…`TextN`, `Bool1`…`BoolN` on core tables (`Activity`, `Project`, `Hour`, `Resource`, …) | Polluted aggregates; meaningless names in ubiquitous language |
| **Per-tenant semantics** | Same column means different things per client (WBS code vs cost centre vs shift label) | Cannot promote to core without breaking other tenants |
| **Screen variance** | Field shown on planning list for Tenant A, hidden on hours screen for Tenant B | Requires view schema, not `if (tenant)` in Angular |
| **Filter variance** | API/query accepts `text1`, `bool2` as opaque filters | Needs named specifications or extension filter port |
| **Workflow hooks** | `Bool3 == true` gates approval, export, or status transition | May belong in customization pack rules or domain — classify carefully |

**Critical rule:** Semantic meaning is **per tenant (or per tenant pack)**, not per column name globally. `Text1` on `Activity` for Tenant A is not the same concept as `Text1` for Tenant B.

### Field role taxonomy (classify every usage)

| Role | Question | V2 default placement |
|------|----------|----------------------|
| **Display only** | Shown in grid/detail; no invariant | Pack view schema + `extensions` projection |
| **Filter only** | Used in list/search; not shown | Pack filter spec or extension query port |
| **Display + filter** | Both | Pack column + batched extension load + filter spec |
| **Workflow input** | User edits; drives state transition | Pack UI + Application port; promote only if universal invariant |
| **Workflow guard** | Read in `if` / rule engine / validation | Pack rule actor or domain policy — **document owner** |
| **Integration mapping** | Filled by import/export | Integration pack or strangler adapter — not customization pack alone |
| **Report dimension** | Grouping/slicing in SSRS/Excel | Read model / reporting projection |

### V2 placement decision tree (Phase F)

```text
Is the field part of ubiquitous language for ALL tenants?
├─ YES → Promote to core read DTO (and domain if invariant)
└─ NO
   ├─ Used only for display/filter on lists?
   │  └─ Customization pack: ViewDefinition + extensions (+ filter spec)
   ├─ Drives tenant-specific workflow branch?
   │  └─ Customization pack port + optional TenantRulesActor stage
   ├─ Set/consumed by external system?
   │  └─ Integration pack (+ external ID registry if needed)
   └─ Legacy-only during strangler?
      └─ [StranglerAdapter] maps legacy column ↔ V2 extension bag
```

**Forbidden in V2 core:** `if (tenantSlug == "acme")` for column visibility; optional `Text17` on domain entities; per-row pack I/O in list mappers.

---

## Required inputs (fill before Phase A)

```yaml
program:
  name: "F2P Platform 2.0 — Tenant Workflow Extension Fields Deep-Dive"
  monolith_repo: "<url or local path>"
  product_name: Floor2Plan
  product_alias: F2P

known_field_patterns:              # agent validates and expands
  text_columns: [Text1, Text2, Text3, Text4, Text5, Text6, Text7, Text8]
  bool_columns: [Bool1, Bool2, Bool3, Bool4, Bool5]
  aliases: [CustomText1, UserText1, ExtraField1, Flag1, UdfText1]

known_entities:                  # seed; agent discovers all carriers
  - Activity
  - Project
  - WbsNode
  - Hour / TimeEntry
  - Resource
  - Assignment

bounded_contexts:                # from 02-bounded-context-map.md or seed
  - Planning
  - Hours
  - WBS
  - Resources
  - Import
  - Reporting

target_principles:
  - "Client-only semantics ship as customization packs, not core optional columns"
  - "Filters use named specifications or pack filter ports — not opaque text1 query params"
  - "UI variance via view schema (labelKey + visibility), not tenant branches in SPA"
  - "Promote to core only when semantics are universal and stable"
```

---

## Phase A — Extension field discovery sweep

### Agent prompt (copy/paste)

```text
TENANT WORKFLOW FIELDS — PHASE A ONLY: Discovery sweep.
Do not propose V2 architecture yet.

Prerequisites: docs/modularization/00-inventory.md (Phase 0 complete).

Tasks:
1. Search the repository for extension field evidence:
   - Entity properties: Text1, Text2, …, Bool1, Bool2, … (PascalCase and camelCase)
   - EF mappings, migrations, Fluent API, database column names
   - DTOs, view models, API request/response models carrying text*/bool*
   - SQL views, stored procedures, report datasets referencing these columns
   - Razor/Vue/Angular templates binding text1, bool2, etc.
   - Query/filter code: Where(a => a.Text1), dynamic LINQ, OData, grid filter models
   - Tenant config: JSON/XML/settings tables mapping labels or visibility per client
   - Import/export mappers setting Text* / Bool* from external files
   - Comments referencing "UDF", "user field", "custom field", "tenant field", "workflow field"
2. For each hit, record a raw field candidate (entity + column + location).
3. Map each candidate to:
   - suspected bounded context / aggregate
   - suspected role (display, filter, workflow, integration, report) — guess only
   - file path and symbol
4. Deduplicate: same column may appear in entity + DTO + migration + UI.

Output: docs/modularization/tenant-workflow-fields/00-raw-field-candidates.md

Format: table with columns:
| id | entity | column | context_guess | role_guess | location | evidence_snippet | confidence |

Mark low-confidence items [NEEDS REVIEW].
Cite file paths for every row. Stop when sweep is complete.
```

### Acceptance criteria

- All seed patterns (`Text1`…`Bool*`) searched; document if zero hits on an entity type.
- Every entity type carrying extension columns is listed.
- UI and API filter surfaces are included, not only EF entities.
- No V2 recommendations in this phase.

---

## Phase B — Field catalog (per bounded context)

### Goal

Turn raw candidates into a structured **field catalog** owned by a bounded context.

### Agent prompt (run once per context)

```text
TENANT WORKFLOW FIELDS — PHASE B: Field catalog for bounded context "<CONTEXT_NAME>".

Inputs:
- docs/modularization/tenant-workflow-fields/00-raw-field-candidates.md
- docs/modularization/02-bounded-context-map.md

Tasks:
1. Filter raw candidates owned by this context.
2. Group by aggregate root / table (e.g. Activity, Project).
3. For each column (Text1, Bool2, …) on that aggregate, document:
   - DB type, nullability, max length (from migration/fluent config)
   - All read surfaces (API DTOs, reports, exports)
   - All write surfaces (forms, imports, bulk update jobs)
   - All filter surfaces (list endpoints, repository methods, grid query builders)
   - Any validation or business rules referencing the column
   - Link to entry points (EP-###) from 01-entry-points.md where possible
4. Note whether column name is stable across tenants or relabeled in UI only.

Outputs:
- docs/modularization/tenant-workflow-fields/contexts/<context-slug>/field-catalog.md

Per-column subsection template:
### <Entity>.<Column>
- **Type:** …
- **Roles:** display | filter | workflow | integration | report
- **Read paths:** …
- **Write paths:** …
- **Filter paths:** …
- **Rules:** …
- **Tests:** linked | GAP

Rules:
- Do not invent semantics; mark meaning [UNDOCUMENTED] if only column name exists.
- Cite file:line for every claim.
- Max 30 columns per aggregate in first pass; defer rare columns to appendix.

Stop after this context. Wait for human review before next context.
```

---

## Phase C — Tenant usage matrix (domain expert validation)

### Goal

Reverse-engineer **who uses what** — the same `Text1` column often means different things per tenant.

### Agent prompt

```text
TENANT WORKFLOW FIELDS — PHASE C: Tenant usage matrix.

Inputs:
- docs/modularization/tenant-workflow-fields/contexts/*/field-catalog.md
- Tenant configuration sources (DB tables, appsettings per client, submodule folders, feature flags)

Tasks:
1. Identify how tenants are distinguished in code (tenant id, client code, subdomain, config profile).
2. For each tenant (or tenant group) that uses extension fields, build rows:
   | tenant | entity | column | business_label | screens | filterable | editable | workflow_rule | notes |
3. Find evidence for labels (resx, vue i18n, hardcoded strings, admin config UI).
4. Find evidence for visibility (hidden columns, `v-if`, CSS, server-side omit).
5. Flag columns that are **unused** (always null) vs **load-bearing** (workflow depends on them).
6. Produce workshop questions for domain experts on [UNDOCUMENTED] semantics.

Outputs:
- docs/modularization/tenant-workflow-fields/03-tenant-usage-matrix.md
- docs/modularization/tenant-workflow-fields/03-tenant-usage-matrix.yaml

YAML schema (minimum):
tenant_usage:
  - tenant_id: "<slug or id>"
    fields:
      - entity: Activity
        column: Text2
        label: "Cost element"          # or [UNDOCUMENTED]
        screens: [planning-activity-list, hour-approval-detail]
        roles: [display, filter]
        load_bearing: true
        evidence: ["path:line", ...]

Stop when all known tenant packs / client configs are processed or marked GAP.
```

### Acceptance criteria

- At least one row per **load-bearing** field per active tenant workflow.
- Expert workshop questions listed for undocumented semantics.
- No V2 design yet — facts and evidence only.

---

## Phase D — Filter and query trace

### Goal

Document how filtering on extension fields works today — critical for V2 specifications and SQL performance.

### Agent prompt

```text
TENANT WORKFLOW FIELDS — PHASE D: Filter and query trace.

Inputs:
- field catalogs + tenant usage matrix
- docs/floor2plan-v2-read-model-playbook.md (reference_only for target patterns)

Tasks:
1. For each filter surface involving Text*/Bool*:
   - HTTP parameter names and OpenAPI/Swagger if present
   - Server-side handler → repository/service call chain
   - Expression shape (indexed equality, Contains, dynamic predicate, client-side filter)
   - DB indexes on extension columns (migrations, DBA scripts)
   - Pagination: server-side vs load-all-then-filter in browser
2. Classify filter complexity:
   - **Simple:** equality / null check on one column
   - **Composite:** multiple bool flags ANDed
   - **Cross-field:** Text1 combined with core fields (dates, status)
   - **Unsafe:** string Contains without bounds; unparameterized SQL
3. Link to characterization test gaps.

Output: docs/modularization/tenant-workflow-fields/04-filter-trace.md

Include per-filter table:
| filter_id | entity | columns | entry_point | query_location | index? | tenant_scope | test |

Cite file:line. Mark [NEEDS REVIEW] for dynamic LINQ.
```

---

## Phase E — Workflow and rule dependencies

### Goal

Find business logic that **depends** on extension fields — not just display.

### Agent prompt

```text
TENANT WORKFLOW FIELDS — PHASE E: Workflow and rule dependencies.

Tasks:
1. Search for control flow on Bool* / Text*:
   - if (activity.Bool1), switch (text2), validation attributes
   - State machines, approval gates, export eligibility
   - Hangfire jobs, actors, handlers conditioned on extension fields
   - SaveChanges interceptors, domain services, ABP application services
2. For each rule, document:
   - trigger (user action, import, schedule)
   - condition (exact expression)
   - effect (status change, notification, block save)
   - tenant scope (all tenants vs specific client config)
3. Classify: **display concern** vs **true invariant** vs **tenant policy**.

Output: docs/modularization/tenant-workflow-fields/05-workflow-rules.md

Link rules to UC-### where they exist; otherwise propose UC-### candidates.
Flag rules that should NEVER move to core domain without expert sign-off.
```

---

## Phase F — V2 judgment and migration proposal

### Goal

Synthesize Phases A–E into an actionable **target design** using Platform 2.0 standards.

### Agent prompt

```text
TENANT WORKFLOW FIELDS — PHASE F: V2 judgment and migration proposal.

Inputs:
- All artifacts under docs/modularization/tenant-workflow-fields/
- SandBox standards (reference_only): platform-ui-customization-standard.md,
  floor2plan-v2-read-model-playbook.md, platform-actor-standard.md

Tasks:
1. For each (tenant, entity, column) in the usage matrix, recommend one of:
   - **KEEP_LEGACY** — strangler only; no V2 surface yet
   - **PACK_EXTENSION** — customization pack: extensions bag + view column + labelKey
   - **PACK_FILTER** — pack-owned filter spec / query port (named filter, not text1 param)
   - **PACK_RULE** — tenant rule in customization pack or TenantRulesActor
   - **PROMOTE_READ** — add to core read DTO (universal semantics)
   - **PROMOTE_DOMAIN** — domain value object / invariant (rare; justify)
   - **INTEGRATION_PACK** — owned by import/export pack
   - **DROP** — unused; safe to omit in V2 with parity test proof
2. Propose pack boundaries:
   - One customization pack per tenant workflow profile vs shared pack with config
   - Pack id naming: `<client>-<context>-v1`
3. For each V2 list screen affected:
   - ViewDefinition columns (core / extension / computed)
   - Filter API shape (typed filter DTO + specifications)
   - Batch extension loading strategy (see platform-ui-customization-standard performance rules)
4. Strangler plan:
   - [StranglerAdapter] read/write mapping legacy columns ↔ extensions JSON
   - Characterization tests required before cutover
5. Explicit **anti-patterns rejected** for this program.

Outputs:
- docs/modularization/tenant-workflow-fields/06-v2-migration-proposal.md
- docs/modularization/tenant-workflow-fields/06-v2-migration-proposal.yaml

YAML minimum per field:
field_migration:
  - tenant: acme
    entity: Activity
    legacy_column: Text2
    recommendation: PACK_EXTENSION
    v2_extension_key: sapCostElement
    v2_label_key: packs.acme-planning-v1.columns.sapCostElement
    filter: PACK_FILTER
    filter_spec: AcmeActivitiesByCostElementSpec
    parity_tests: [TC-###, GAP]
    risks: [...]

Human review required before implementation.
```

### Acceptance criteria

- Every **load-bearing** legacy field has a recommendation with evidence link.
- Filter migration does not propose opaque `?text1=` on V2 APIs without pack scoping.
- Performance section addresses batch extension load (no per-row N+1).
- Promotion to domain is rare and justified per field.

---

## Phase G — Characterization test plan

### Agent prompt

```text
TENANT WORKFLOW FIELDS — PHASE G: Characterization test plan.

Using docs/monolith-modularization/templates/test-case.schema.yaml:

1. For each PACK_* and PROMOTE_* item in 06-v2-migration-proposal.yaml, define:
   - Legacy baseline test (tenant + screen + filter + expected rows)
   - V2 target test (same AC, pack enabled)
   - Strangler adapter test (dual-write/read parity if applicable)
2. Prioritize load-bearing workflow rules and high-traffic list filters.
3. Mark tests that need anonymized production snapshots [NEEDS DATA].

Output: docs/modularization/tenant-workflow-fields/07-test-plan.md
```

---

## Quick prompts (ad-hoc)

### Map one screen

```text
Screen "<screen-name>" in legacy monolith: list every Text*/Bool* shown or filtered.
Trace to entity columns and tenant config. Output table: column | label | editable | filter param | file:line.
```

### Judge one column

```text
Entity "<Entity>.<Column>" — full legacy usage trace. Recommend V2 placement using
platform-ui-customization-standard decision tree. List risks if promoted to core.
```

### Design one customization pack

```text
Design customization pack "<pack-id>" for context "<Context>" tenant "<tenant>".
Define: I<Context>CustomizationPack methods, ViewDefinition for screens [...],
extension keys, filter specifications, labelKeys, strangler adapter touchpoints.
Reference Hour Approvals Acme pack as reference_only.
```

### Eliminate opaque grid filters

```text
List endpoint "<route>" accepts generic text1/bool2 filters.
Propose typed V2 ListFilter DTO + named Ardalis specifications per tenant pack.
Show index strategy for extension-backed filters.
```

---

## Implementation guardrails (analysis → code)

```text
TENANT WORKFLOW FIELD IMPLEMENTATION GUARDRAILS:

1. No Text1/Bool2 on core domain entities in new modules.
2. Customization pack owns extension semantics, view schema, and tenant filter specs.
3. List endpoints: batch GetRowExtensions(ids) — never per row in a loop.
4. Filters: named specifications in Infrastructure; IQueryable never leaves Infrastructure.
5. Angular: schema-driven columns from capabilities/view — no if (tenant === ...).
6. labelKey in packs; translations in locale JSON — not English strings in C#.
7. Characterization tests pass before retiring legacy column reads.
8. [StranglerAdapter] for dual-read period; tag with removal ticket.
9. Workflow rules in pack/actor before promoting to domain — expert sign-off required.
10. Integration-filled fields belong to integration packs, not UI-only customization.
```

---

## First iteration starter pack

Minimum viable deep-dive (after Phase 0), in the **external monolith repo**:

1. **Phase A** — full repo sweep for Text*/Bool*
2. **Phase B** — pilot contexts: **Planning**, **Hours** (highest screen + filter variance)
3. **Phase C** — tenant matrix for top 3–5 active client workflows
4. **Phase D** — filter trace for pilot list screens
5. **Phase E** — workflow rules on Bool* flags used in approvals/exports
6. **Phase F** — V2 proposal aligned to `platform-ui-customization-standard.md`
7. **Phase G** — characterization tests for top 10 load-bearing fields

Expand to WBS, Resources, Reporting after pilot artifact format is validated.

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-27 | Initial deep-dive for legacy Text*/Bool* tenant workflow fields → V2 packs |
