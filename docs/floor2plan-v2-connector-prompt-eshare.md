# Ready prompt — eShare connector → V2 pack

**Run in:** external **customized repository** (`core/` + `connectors/eshare/` or similar). Connector repo alone typically does not compile.

**Attach or paste:** `docs/floor2plan-legacy-connector-submodule-antipattern.md`, `docs/floor2plan-v2-connector-architecture.md`.

**Output:** `docs/modularization/integrations/v2-proposals/eshare-<domain>-v1.md` (Claude picks domain after Phase 1)

---

## Copy from here

```text
@workspace LEGACY CONNECTOR → V2 PACK — concrete migration proposal

You are analyzing the legacy **eShare connector** and producing a concrete Platform 2.0
integration-pack design. Avoid abstract platform lectures; every section must cite
evidence from this repo or mark [NEEDS REVIEW].

## Target architecture (mandatory)

1. Integration PACK references only Integration.Abstractions + vendor SDKs — NOT core Domain,
   Application services, DbContext, or EF entities.
2. Core modules reference Integration.Abstractions only — NOT pack projects.
3. Host/gateway composes packs + core.
4. Inbound: pack maps eShare → canonical format → appropriate core port (usually Import).
5. Outbound: core command/event → outbound port → pack → eShare (if legacy pushes).
6. eShare-specific types never leave the pack Connector/Mapping projects.
7. External IDs namespace "EShare" (or legacy namespace found in code) on linked entities.
8. No SaveChanges handlers for integration orchestration.

## Scope for this run

connector_scope:
  name: "eShare connector"
  vendor: "EShare"
  legacy_hints:
    - "core/"
    - "connectors/"
    - ".gitmodules"
    - "eshare"
    - "EShare"
    - "E-Share"
    - "e-share"
    - "Eshare"
    - "submodules"                    # .gitmodules search
    - "*Share*Connector*"
    - "*Share*Integration*"
    - "Transmittal"
    - "DocumentLink"
    - "EDMS"
  bounded_context: "[DISCOVER]"       # Import | Planning | Resources | WBS — determine in Phase 1
  direction: "[DISCOVER]"             # inbound | outbound | bidirectional
  open_questions:
    - "What does eShare integrate — documents, drawings, transmittals, activity attachments?"
    - "Is eShare system of record for documents while F2P owns activities?"
    - "Is this a submodule referencing core like other legacy connectors?"

---

## PHASE 0 — Customized repository layout

| Path | Submodule URL | Role |
|------|---------------|------|
| core/ | | |
| connectors/<eshare-folder>/ | | eShare connector |

If workspace is connector-only and does not build, STOP.

---

## PHASE 1 — Forensic: find the legacy eShare connector

### 1.1 Discovery search (exhaustive)

Search case-insensitive:
- eshare, e-share, EShare, Eshare
- .gitmodules + submodule folder names
- csproj names and ProjectReference to core
- *Eshare* *EShare* in class, job, controller, service names
- Configuration keys: EShare*, eshare*, Document*, Transmittal*
- Integration tests: *Eshare*Test*, *Share*Integration*
- HTTP clients, WCF, file paths pointing to eShare endpoints
- UI strings: "eShare", "transmittal", document upload from eShare

**First output:** one paragraph stating what eShare does in THIS codebase with ≥3 file citations,
or stop with "not found" and list searches tried.

### 1.2 Legacy dependency map

| From | To | Dependency type | Evidence file:line |
|------|-----|-----------------|-------------------|

Highlight submodule → core project references.

### 1.3 Legacy runtime flow

Trigger → classes → external call → core persistence → side effects.

### 1.4 Submodule coupling

Answer explicitly:
- Is eShare a git submodule? Path?
- Which core types does it import/use directly?
- Duplicate logic in jobs vs submodule?

---

## PHASE 2 — Classify semantics

After discovery, fill:

| Entity / concept | Lead system | Follow system | Evidence |
|------------------|-------------|---------------|----------|

Examples to consider (mark N/A if not in code):
- Document / drawing metadata
- Link to Activity / Component
- Transmittal workflow state
- File binary vs metadata only

---

## PHASE 3 — Concrete V2 proposal

Use discovered behaviour to name the pack. Example IDs (adjust after Phase 1):

```yaml
pack:
  id: "eshare-document-links-v1"          # or eshare-transmittal-inbound-v1
  display_name: "eShare document integration"
  contract_version: "f2p-intermediate-<TBD>-v1"
  direction: <inbound|outbound|both>
  enabled_by: "tenant.integration.packs.eshare.enabled"
```

### 3.2 Solution tree

```text
F2P.Integration.Packs.EShare.<Domain>/
├── EShare.<Domain>.Connector/
├── EShare.<Domain>.Mapping/
├── EShare.<Domain>.Pack/
└── EShare.<Domain>.Tests/
    └── Golden/
```

### 3.3 Ports

Define ports based on actual legacy behaviour — do not assume PLM-style structure import.

| Port | Implemented by |
|------|----------------|
| | |

### 3.4 Canonical payload

One example JSON for the handoff pack → core, using field names from legacy code/tests.

### 3.5 Pack classes ↔ legacy classes mapping table

| V2 class | Replaces legacy |
|----------|-----------------|

### 3.6 Core changes (minimal)

Which bounded context module owns persistence? No eShare types in domain.

### 3.7–3.10

Host registration, tests (name legacy test fixtures), strangler steps, mermaid diagram.

---

## Output

Write: docs/modularization/integrations/v2-proposals/eshare-<domain>-v1.md

Rules:
- If eShare is only config or dead code, say so and propose decommission vs pack.
- Do not copy PLM structure import design unless legacy actually imports WBS from eShare.
- Cite file:line for every claim.
- Do not propose pack → Domain references.
```
