# Ready prompt — PLM planning connector → V2 pack

**Recommended first example** — aligns with `ApiImportActorPoc` external IDs and integration templates in this repo.

**Run in:** external **customized Floor2Plan repository** (clone with `core/` + `connectors/`). If only the PLM connector repo is available, Phase 0 must fail fast and request the parent customized repo.

**Attach or paste:** `docs/floor2plan-legacy-connector-submodule-antipattern.md` (customized-repo layout), `docs/floor2plan-v2-connector-architecture.md`, `ApiImportActorPoc/README.md` (from SandBox).

**Output:** `docs/modularization/integrations/v2-proposals/plm-structure-inbound-v1.md`

---

## Copy from here

```text
@workspace LEGACY CONNECTOR → V2 PACK — concrete migration proposal

You are analyzing the legacy **PLM planning / structure connector** and producing a
concrete Platform 2.0 integration-pack design. Avoid abstract platform lectures;
every section must cite evidence from this repo or mark [NEEDS REVIEW].

## Target architecture (mandatory)

1. Integration PACK references only Integration.Abstractions + vendor SDKs — NOT core Domain,
   Application services, DbContext, or EF entities.
2. Core modules reference Integration.Abstractions only — NOT pack projects.
3. Host/gateway composes packs + core.
4. Inbound: pack maps PLM → canonical/intermediate format → Import module port.
5. Outbound (if any): core command/event → outbound port → pack → PLM.
6. PLM-specific types never leave the pack Connector/Mapping projects.
7. External IDs namespace "PLM" on every imported entity; idempotent upsert in core.
8. No SaveChanges handlers for integration orchestration.

## Scope for this run

connector_scope:
  name: "PLM project structure import (WBS / components / activities for planning)"
  vendor: "PLM"
  legacy_hints:
    - "core/"                         # submodule root in customized repo
    - "connectors/"                   # sibling connector folders
    - ".gitmodules"
    - "PlmImport"
    - "PlmStructure"
    - "PlmPlanning"
    - "PLM"
    - "Integration/Plm"
    - "DataIntegrationTests"
    - "PlmImportTests"
    - "EntityExternalId"
    - "SourceSystem"
    - "PLM:"
  bounded_context: "Import"           # primary; also trace touchpoints in WBS + Planning
  direction: "inbound"                # validate; flag any outbound planning sync [NEEDS REVIEW]
  domains:
    - structure                        # components, activities, relations
    - external_ids                   # PLM namespace
    - planning_enrichment            # what F2P adds after import (dates, assignments) — core only

hypothesis_from_templates:
  lead_follow:
    - entity: "WBS / component / activity structure"
      follow: "PLM leads — F2P imports"
    - entity: "planned dates / durations / milestones"
      lead: "F2P leads — not written back to PLM in most tenants"
  test_entry_points:
    - "tests/Legacy.DataIntegrationTests/PlmImportTests.cs"
    - "tests/**/Plm*Test*"
    - "TestData/**/Plm*"
    - "TestData/**/PLM*"

---

## PHASE 0 — Customized repository layout

Document the clone structure:

| Path | Submodule? | Role |
|------|------------|------|
| core/ | yes/no | Main Floor2Plan |
| client/ (or actual folder name) | yes/no | Derived services, sync, DI overrides |
| connectors/plm-planning/ (or actual name) | yes/no | PLM connector |

List `.gitmodules` entries. Document:
- csproj references from connector → core
- csproj references from client services → core
- DI registrations swapping base services for client subclasses
- override methods involved in import/sync path for PLM

---

## PHASE 1 — Forensic: find the legacy PLM connector

### 1.1 Discovery search

Execute searches for legacy_hints. Also:
- .gitmodules entries containing Plm, PLM, plm
- csproj ProjectReference from any Plm* project TO core Domain/Application/Data
- Hangfire jobs: *Plm*, *PLM*, *Structure*Import*
- SaveChanges / workflow handlers mentioning PLM
- Configuration: Plm*, PLM*, ExternalSystem
- UI or API entry points: "import from PLM", structure sync

### 1.2 Legacy dependency map (required table)

| From | To | Dependency type | Evidence file:line |
|------|-----|-----------------|-------------------|

Flag every compile-time reference from connector/submodule → core as VIOLATION of V2 rules.

### 1.3 Legacy runtime flow

Numbered steps with citations: file drop / API / job trigger → classes → EF SaveChanges →
side effects in Planning module.

### 1.4 Legacy pain points (this connector only)

---

## PHASE 2 — Lead/follow per entity

| Entity type | Lead | Follow | Evidence |
|-------------|------|--------|----------|
| Component / block | | | |
| Activity | | | |
| Activity relation | | | |
| Planned dates | | | |
| External ID (PLM) | | | |

Answer explicitly: "Does any tenant write planning data back to PLM?" — cite test or [NEEDS REVIEW].

---

## PHASE 3 — Concrete V2 proposal

### 3.1 Pack identity

```yaml
pack:
  id: "plm-structure-inbound-v1"
  display_name: "PLM structure import"
  contract_version: "f2p-intermediate-structure-v1"   # align with ApiImportActorPoc import payload if possible
  direction: inbound
  enabled_by: "tenant.integration.packs.plm-structure.enabled"
```

### 3.2 Solution tree (exact project names)

Propose:

```text
F2P.Integration.Packs.Plm.Structure/
├── Plm.Structure.Connector/       # file watcher, API client, or PLM SDK
├── Plm.Structure.Mapping/           # PLM XML/JSON → canonical
├── Plm.Structure.Pack/
└── Plm.Structure.Tests/
    └── Golden/                      # real fixture from PlmImportTests TestData
```

List allowed csproj references per project.

### 3.3 Ports

| Port | Implemented by | Notes |
|------|----------------|-------|
| IPlmStructureFetchPort | Pack | read from PLM |
| IInboundStructureImportPort | Import module | canonical batch submit |

Provide C# interface stubs.

### 3.4 Canonical payload example

One JSON document matching a **real** golden file from legacy tests, including:

```json
"externalIds": { "PLM": "<sample>" }
```

Map fields to: Project, Component, Activity, ActivityRelation from ApiImportActorPoc import model
where applicable.

### 3.5 Named pack classes

| Class | Project | Replaces legacy class |
|-------|---------|----------------------|
| | | |

### 3.6 Core changes (minimal)

- Import command / actor handler name
- What Planning module receives **after** import (event? read model?) — no PLM types

### 3.7 Host registration

### 3.8 Tests

| Test | Fixture path in legacy repo |
|------|----------------------------|
| Golden map | |
| Idempotent re-import | Import_SamePlmFileTwice_UpdatesByExternalId (if exists) |

### 3.9 Strangler migration (max 10 steps)

Include running legacy PlmImportTests as characterization baseline.

### 3.10 Mermaid — this pack only

---

## Output

Write: docs/modularization/integrations/v2-proposals/plm-structure-inbound-v1.md

Rules:
- Cite file:line for legacy claims.
- Do not propose pack → Domain references.
- Prefer structure-only slice; planning recalc stays in Planning module after import event.
- If PlmImportTests or submodule not found, stop and list missing paths.
```
