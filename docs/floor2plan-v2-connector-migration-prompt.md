# Claude prompt — legacy connector → concrete V2 pack proposal

Copy the **prompt block** below into Claude (Cursor) with the **external Floor2Plan monolith** open as workspace root.

**Goal:** Find one legacy connector built the old way (**customized repo** with `core/` submodule + sibling connector folders) and produce a **concrete** V2 integration-pack design.

**Legacy layout (confirm in Phase 1):**

```text
customized-repo/
├── core/                 ← submodule → Floor2Plan main repo
├── connectors/<name>/    ← submodule → connector repo (cannot compile alone)
└── *.sln                 ← builds core + connectors together
```

Open the **customized repository** clone if the connector repo alone does not build.

**Prerequisite docs** (in SandBox repo — paste paths or attach files to Claude if the monolith repo does not contain them):

- `docs/floor2plan-legacy-connector-submodule-antipattern.md`
- `docs/floor2plan-v2-connector-architecture.md`
- `ApiImportActorPoc/README.md` (external IDs, import boundary)

---

## Before you run

Fill in the YAML block inside the prompt:

```yaml
connector_scope:
  name: "<e.g. SAP WBS import, Kronos hours sync, PLM structure>"
  vendor: "<SAP | Kronos | PLM | HR | File | Other>"
  legacy_hints:                    # optional search seeds
    - "<submodule path or folder name>"
    - "<class name prefix, e.g. Sap*, Kronos*>"
  bounded_context: "<Import | WBS | Hours | Planning | Resources>"
  direction: "<inbound | outbound | bidirectional>"   # best guess; Claude validates
```

Open the **customized repository** (or core monolith if that is the workspace). If only a connector repo is open and it does not compile, stop and request the parent customized repo path.

---

## Prompt (copy from here)

```text
@workspace LEGACY CONNECTOR → V2 PACK — concrete migration proposal

You are analyzing ONE legacy Floor2Plan connector and producing a concrete Platform 2.0
integration-pack design. Avoid abstract platform lectures; every section must cite evidence
from this repo or mark [NEEDS REVIEW].

## Target architecture (mandatory — do not deviate)

Follow Floor2Plan V2 connector rules:

1. Integration PACK references only Integration.Abstractions + vendor SDKs.
   NO references to core Domain, Application services, DbContext, or EF entities.
2. Core modules reference Integration.Abstractions only — NOT pack projects.
3. Host/gateway composes packs + core (composition root).
4. Inbound: pack maps vendor → canonical/intermediate format → Import module port.
5. Outbound: core raises command/event → outbound port → pack → vendor.
6. Vendor-specific types never leave the pack's Connector/Mapping projects.
7. External IDs on every imported entity; idempotent upsert in core (not pack).
8. No new SaveChanges handlers for integration; explicit workflow/job/actor at boundary.

Reference: SandBox docs floor2plan-v2-connector-architecture.md and
floor2plan-legacy-connector-submodule-antipattern.md if available in context.

## Scope for this run

connector_scope:
  name: "<FILL>"
  vendor: "<FILL>"
  legacy_hints:
    - "<FILL>"
  bounded_context: "<FILL>"
  direction: "<FILL>"

---

## PHASE 0 — Identify repository layout (required first)

| Layout | How to recognize | Action |
|--------|------------------|--------|
| Customized repo | core/ + connectors/ + umbrella .sln | Document .gitmodules |
| Core only | Single monolith | Find embedded integration |
| Connector only | Broken refs to core | STOP — need customized repo |

| Path | Submodule URL | Role |
|------|---------------|------|
| core/ | | Floor2Plan application |
| client/ (or similar) | | Derived services, sync jobs, DI overrides |
| connectors/<name>/ | | Vendor connector |

---

## PHASE 1 — Forensic: find the legacy connector (evidence only)

Search the workspace and document HOW this connector is built today.

### 1.1 Discovery search

Search for:
- **Customized repo layout:** `core/`, `connectors/`, `.gitmodules`, umbrella solution file
- Git submodule paths and .gitmodules entries matching legacy_hints
- Project references FROM `connectors/<vendor>/` TO `core/` (csproj Reference Include)
- Prove connector repo **fails to build** without core (list missing references if visible)
- Classes named *Connector*, *Integration*, *Import*, *Sync*, *Export* for this vendor
- Hangfire / IHostedService / scheduled jobs for this vendor
- SaveChanges handlers, workflow handlers touching this vendor
- Direct use of core entities (e.g. Activity, Project, WbsNode) inside connector code
- Configuration keys (SAP*, Kronos*, PLM*, External*)
- Integration tests naming this vendor

### 1.2 Legacy dependency map

Produce a table:

| From (legacy) | To (legacy) | Dependency type | File:line evidence |
|---------------|-------------|-----------------|-------------------|
| e.g. SapSubmodule | Core.Domain | project reference | path |
| e.g. SapImportJob | ImportService | method call | path |

### 1.3 Legacy runtime flow

Sequence (bullet steps with file citations):
- Trigger (schedule, UI, file drop, API)
- Classes executed in order
- Where EF / SaveChanges is called
- Where core domain services are invoked
- External system protocol (RFC, REST, file, etc.)

### 1.4 Legacy pain points (this connector only)

List concrete issues visible in code:
- Compile coupling to core
- Duplicated mapping (job vs service vs submodule)
- Hidden handler side effects
- Client-specific branches
- Untestable without full monolith

Stop Phase 1 when the connector boundary is identified. If multiple connectors match,
pick the smallest well-bounded one and note others as out of scope.

---

## PHASE 2 — Classify integration semantics

For THIS connector, per entity type touched:

| Entity type | Lead/follow | Evidence | Notes |
|-------------|-------------|----------|-------|
| e.g. WBS node | F2P follows PLM | ... | |

Flag bidirectional or undocumented conflicts.

---

## PHASE 3 — Concrete V2 proposal (the deliverable)

Design a V2 integration pack for THIS connector only. Be specific enough that a
developer could create the solution folders in a sprint.

### 3.1 Pack identity

```yaml
pack:
  id: "<e.g. sap-wbs-inbound-v1>"
  display_name: "<human name>"
  contract_version: "<e.g. f2p-intermediate-structure-v1>"
  direction: inbound | outbound | both
  enabled_by: "<tenant profile flag name>"
```

### 3.2 Solution / project tree (exact names)

```text
F2P.Integration.Abstractions/          # shared — only add if port/DTO missing
F2P.Integration.Packs.<Vendor>.<Domain>/
├── <Vendor>.<Domain>.Connector/
├── <Vendor>.<Domain>.Mapping/
├── <Vendor>.<Domain>.Pack/            # manifest + DI extension
└── <Vendor>.<Domain>.Tests/
```

List each project’s ONLY allowed references (csproj level).

### 3.3 Ports and contracts

For each port, specify:

| Port interface | Owner | Implemented by | Method(s) |
|----------------|-------|----------------|-----------|
| e.g. IInboundStructureImportPort | Abstractions | Import module | SubmitBatchAsync |
| e.g. ISapWbsFetchPort | Abstractions | Pack | FetchAsync |

Include C# interface stubs (signatures only, no full implementation).

### 3.4 Canonical payload (concrete)

Provide ONE example JSON (or C# record) for the handoff from pack → core after mapping.
Use realistic field names from the legacy connector. Include externalIds block.

### 3.5 Pack class responsibilities (named types)

| Class | Project | Responsibility |
|-------|---------|----------------|
| e.g. SapIdocWbsFetcher | Connector | call SAP |
| e.g. SapToStructureMapper | Mapping | IDoc → canonical |
| e.g. SapWbsInboundPack | Pack | register ports, manifest |

No class may call core services — show what it calls instead.

### 3.6 Core changes (minimal)

What the Import / owning context must expose or already has:
- New or existing command/handler/actor name
- What core does NOT need to know about SAP

Keep core changes small; prefer existing ApiImportActorPoc patterns if analogous.

### 3.7 Host registration

```csharp
// Pseudocode: how gateway enables pack for tenant
services.AddSapWbsInboundPack(configuration);
```

### 3.8 Test plan (concrete)

| Test | Type | Input | Assert |
|------|------|-------|--------|
| Golden map | Pack unit | tests/Golden/sample.idoc.xml | matches canonical.json |
| Import handoff | Integration | canonical.json | rows in DB / actor message |

Name test files and fixtures.

### 3.9 Strangler migration (this connector)

Ordered steps (max 10) from legacy to V2 without big-bang:
1. ...
Include [StranglerAdapter] touch points and parity/characterization test requirement.

### 3.10 Dependency diagram (this connector only)

Mermaid diagram showing ONLY the projects/classes for this pack and its touchpoints
to Abstractions + Import. No platform-wide diagram.

---

## Output format

Write to: docs/modularization/integrations/v2-proposals/<pack-id>.md

Structure:
1. Executive summary (5 sentences)
2. Phase 1 evidence (tables + flows)
3. Lead/follow matrix
4. V2 pack design (sections 3.1–3.10)
5. Open questions [NEEDS REVIEW]
6. Out of scope

## Rules

- Cite file:line for every legacy claim.
- Do not invent business rules; extract from tests or mark [NEEDS REVIEW].
- Do not propose pack → Domain references — if tempted, stop and redesign.
- Prefer one inbound entity slice (e.g. WBS only) over whole-vendor boil-the-ocean.
- If legacy code is missing from workspace, stop and list what to clone/open.
```

---

## Example filled scope (SAP WBS inbound)

```yaml
connector_scope:
  name: "SAP WBS / project structure import"
  vendor: "SAP"
  legacy_hints:
    - "submodules/SapIntegration"
    - "SapWbsImport"
    - "SapStructure"
  bounded_context: "Import"
  direction: "inbound"
```

---

## Follow-up prompts

**Narrow to one entry point:**

```text
@workspace From docs/modularization/integrations/v2-proposals/<pack-id>.md —
implement Phase 3.4 canonical JSON only from legacy test fixture at <path>.
Cite the test that defines expected behaviour.
```

**Parity test first:**

```text
@workspace For pack <pack-id>, write a characterization test that runs the LEGACY
import path for fixture X and records DB state. Do not implement V2 yet.
Output test plan row only.
```

**Slice too large:**

```text
@workspace The V2 proposal for <pack-id> is too broad. Split into slice 1:
read-only fetch + canonical map only, no Import handoff. Update sections 3.2–3.6.
```

---

## Related artifacts

| Doc | Role |
|-----|------|
| `floor2plan-v2-connector-architecture.md` | Ground rules Claude must follow |
| `floor2plan-legacy-connector-submodule-antipattern.md` | What to find in Phase 1 |
| `external-integrations-deepdive-instructions.md` | Broader integration catalog |
| `ApiImportActorPoc/` | Reference import + external ID behaviour |
