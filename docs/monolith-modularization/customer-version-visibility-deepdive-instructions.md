# Customer version visibility — deep-dive instructions

**Purpose:** Guide AI coding assistants (Claude Code, GitHub Copilot, and similar) through systematic analysis of **legacy customized Floor2Plan repositories** — core + client layer + connector submodules + release branches — to produce a **customer deployment registry**, **support matrix**, and **actionable visibility solution** so fixes, patching, and support know exactly what each customer runs.

**Audience:** Platform engineering, PS/delivery, support leads.

**Prerequisite:** Access to at least one **customized repository** (parent assembler with `core/`, `client/`, `connectors/`) and the **core product repository** (branch/tag topology). Optional: list of known customer repo names from ops/support.

**Outcome:** Machine-readable deployment inventory per tenant, customization depth scoring, branch/support policy recommendations, drift-detection design, and a phased implementation plan (registry + automation + runtime `/version` surface).

---

## Important: analyze customized repos, not SandBox

Run all phases against **external Floor2Plan delivery repositories** — **not** this SandBox workspace.

SandBox holds V2 standards and POCs. Customer version truth lives in:

- **Core repo** — `main`, `rc`, `release/*`, patch branches, tags
- **Customized repos** — one per customer (or per delivery profile) assembling submodules
- **Client customization repos** — derived services, sync jobs, tenant config
- **Connector repos** — integration submodules pinned per customer

```yaml
analysis_target:
  core_repo: "<url or path to Floor2Plan core>"
  customized_repos:                    # at least one; expand to full customer list
    - name: acme
      repo: "<url or path>"
    - name: contoso
      repo: "<url or path>"
  workspace_note: "Open ONE customized repo as workspace root per phase batch, OR a meta-repo that lists all customers. Core repo may be opened separately for branch topology."
  sandbox_repo_role: "Copy finished artifacts to docs/modularization/customer-versions/ in core or a dedicated ops repo."
```

If customized repos are unavailable, stop and output `docs/modularization/customer-versions/00-blockers.md` listing what is missing. Do not invent customer SHAs or versions.

**Read before Phase A (SandBox — reference only, label `reference_only: true`):**

| Document | Use for |
|----------|---------|
| `docs/floor2plan-legacy-connector-submodule-antipattern.md` | Customized-repo layout, version matrix explosion |
| `docs/monolith-modularization/platform-pack-blueprint.md` | Target: pack manifests instead of git forks |
| `docs/monolith-modularization/platform-architecture-overview.md` | Control plane + entitlements target state |
| `docs/monolith-modularization/external-integrations-deepdive-instructions.md` | Connector inventory overlap |
| `docs/monolith-modularization/tenant-workflow-fields-deepdive-instructions.md` | Customization depth (Text*/Bool*, overrides) |

---

## How to use this document

1. Fill **Required inputs** below before Phase A.
2. Run **one phase at a time**. Do not skip Phase A (topology) or Phase C (per-customer inventory).
3. Store outputs under `docs/modularization/customer-versions/` in the **core repo** or a dedicated **platform-ops** repo (create if missing).
4. Human review required after Phases **C**, **E**, and **G** before changing production registry or support policy.
5. Cross-reference `docs/modularization/analysis-instructions.md` for bounded contexts when scoring customization impact.
6. Satisfy `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md` — cite `path:line` or mark `[NEEDS REVIEW]`.

### Claude Code setup (recommended)

Copy into the customized repo (or core repo) as `CLAUDE.md` or `docs/modularization/agent-rules.md`:

```markdown
## Customer version visibility deep-dive

When asked about customer versions, support matrix, deployment registry, or patch targeting:

1. Read `docs/modularization/customer-version-visibility-deepdive-instructions.md` (copy from SandBox if absent).
2. Run phases in order; write artifacts to `docs/modularization/customer-versions/`.
3. Never guess SHAs — extract from `.gitmodules`, tags, CI variables, or deployment config only.
4. Propose solutions that separate **platform version** from **customization profile**.
```

---

## Required inputs (fill before Phase A)

```yaml
program:
  name: "F2P — Customer Version Visibility"
  core_repo: "<url or path>"
  ops_contact: "<team or person for deployment truth>"

branch_topology_expected:          # agent validates against core repo
  integration_branch: main
  stabilization_branch: rc
  release_branch_pattern: "release/*"    # e.g. release/5.4
  patch_branch_pattern: "*-patch*"       # document actual convention
  customer_branches: false               # agent confirms — flag if true

known_customers:                   # seed list; agent expands
  - id: acme
    customized_repo: "<url>"
    environment: production
  - id: contoso
    customized_repo: "<url>"
    environment: production

support_policy_seed:               # agent validates with humans
  lts_releases: ["5.2", "5.4"]
  current_release: "5.4"
  eol_releases: ["5.1", "5.3"]

deployment_surfaces:               # where live version may be recorded
  - azure_devops_pipelines
  - octopus / helm / ansible
  - customer_vm_iis
  - internal_spreadsheet
  - none_known

target_principles:
  - "Branches track the product line; registry tracks tenants"
  - "Platform version is semver/tag on core — not a customer branch name"
  - "Customization is a manifest (submodules + overrides + config) — scored, not guessed"
  - "Patch fixes target release/X.Y; cherry-pick to main — no orphan patch branches"
  - "V2 path: pack entitlements on control plane; legacy path: registry + /version API"
```

---

## Core concepts (apply consistently)

### What a “customer version” actually is (legacy)

A running customer is **not** `release/5.4-acme`. It is a **composition**:

```text
deployment(acme) =
  core_ref          @ v5.4.2 | branch release/5.4 | sha abc123
+ client_ref        @ tag acme-2025.06 | branch main | sha def456
+ connectors[]      @ { plm-planning: sha..., sap-wbs: sha... }
+ build_profile     @ Release-Acme | customized solution name
+ config            @ appsettings.{Environment}.json, web.config transforms
```

Support questions map to this tuple — not to a single branch.

### Two-axis model (registry fields)

| Axis | Meaning | Example |
|------|---------|---------|
| **Platform version** | Supported core product line | `5.4.2` (tag on core) |
| **Customization profile** | Client + connectors + overrides | `acme-prod-2025-06` |
| **Deployment record** | Platform + profile + environment + deploy date | row in registry |

### Customization depth tiers (score every customer)

| Tier | Signals | Support implication |
|------|---------|---------------------|
| **L0 — config only** | appsettings, feature flags, no `client/` overrides | Patch core; low risk |
| **L1 — light client** | 1–3 derived service subclasses, standard connectors | Patch core + retest client layer |
| **L2 — heavy client** | 10+ overrides, custom sync jobs, Text*/Bool* workflow gates | Dedicated regression; slow patches |
| **L3 — forked core** | Changes inside `core/` submodule or long-lived customer branch | Escalation; migration candidate |

### Branch hygiene rules (recommend in Phase E)

| Keep | Remove / avoid |
|------|----------------|
| `main`, `rc`, `release/X.Y` (2–3 LTS max) | Per-customer long-lived core branches |
| Short-lived `fix/*` merged to release | `release/X.Y-patch-acme` without merge-back |
| Tags `vX.Y.Z` on release branches | Using branch name as “the version” in support docs |

---

## Phase A — Repository & branch topology

### Agent prompt

```text
CUSTOMER VERSION VISIBILITY — PHASE A: Repository & branch topology.

Workspace: core Floor2Plan repository (or meta-repo listing all customer repos).

Tasks:
1. Map all long-lived branches: main, rc, release/*, *patch*, per-customer branches (if any).
2. List tags matching v* semver pattern; note which release branch each tag points from.
3. Document branch flow: where fixes land first, cherry-pick rules (from README, wiki, or commit messages).
4. Find CI/CD pipelines that build core-only vs customized builds; note artifact naming (does it include version?).
5. Search for version strings: AssemblyInfo, Directory.Build.props, csproj Version, build scripts.
6. If a VERSION, CHANGELOG, or release notes file exists, summarize last 5 releases.

Outputs:
- docs/modularization/customer-versions/01-branch-topology.md
- docs/modularization/customer-versions/01-branch-topology.yaml

YAML minimum:
branch_topology:
  integration: main
  stabilization: rc
  releases:
    - version: "5.4"
      branch: release/5.4
      latest_tag: v5.4.2
      status: current | lts | eol
  patch_policy: "<extracted or NEEDS REVIEW>"
  customer_specific_branches: []   # list with evidence
version_sources:
  - file: path
    mechanism: "<how version is set at build>"
```

### Acceptance criteria

- Every `release/*` branch has a latest tag or `[NEEDS REVIEW]`.
- Patch workflow is documented or explicitly unknown.
- No architecture recommendations in this phase — inventory only.

---

## Phase B — Customized repo anatomy (template)

### Agent prompt

```text
CUSTOMER VERSION VISIBILITY — PHASE B: Customized repo anatomy.

Workspace: ONE representative customized repository (parent assembler).

Tasks:
1. Parse .gitmodules — list every submodule: path, url, branch pin (if any).
2. Map folder layout: core/, client/, connectors/*, solution file(s), build scripts.
3. Identify how submodules are pinned in practice:
   - committed SHA in parent repo
   - branch tracking in .gitmodules
   - manual checkout scripts
4. Find build entry point: which .sln, which configuration (Debug/Release/ClientX).
5. Locate deployment config: appsettings*, web.config, environment transforms, install scripts.
6. Search for hardcoded client/tenant identifiers (company name, tenant slug, "Acme").
7. Document derived service pattern: grep for ": ImportService", ": PlanningService", "override " in client/.
8. List connector submodules and whether each is optional per build profile.

Outputs:
- docs/modularization/customer-versions/02-customized-repo-anatomy.md
- docs/modularization/customer-versions/02-customized-repo-template.yaml

template:
  customized_repo_layout:
    submodules:
      - path: core
        remote: "<url>"
        pin_style: sha | branch | floating
      - path: connectors/plm-planning
        remote: "<url>"
        pin_style: sha
    client_layer:
      derived_services_count: N
      sync_jobs_paths: [...]
    solutions: [...]
    build_configurations: [...]
```

### Acceptance criteria

- Submodule pin mechanism is explicit (SHA vs branch).
- Template is reusable for Phase C batch runs across customers.

---

## Phase C — Per-customer deployment inventory

### Agent prompt

```text
CUSTOMER VERSION VISIBILITY — PHASE C: Per-customer deployment inventory.

For EACH known customer customized repo (from required inputs):

Tasks:
1. Clone or open repo; record current branch and parent repo HEAD sha.
2. For each submodule in .gitmodules, record:
   - path, remote url, checked-out sha, branch/tag if detached, commit date, one-line subject
3. Derive platform version:
   - prefer core submodule tag if pointing to tagged commit
   - else map core sha to nearest release/* tag
   - else mark platform_version: UNKNOWN with sha
4. Record client/ submodule sha and connector shas as customization_profile refs.
5. Score customization tier L0–L3 (see core concepts) with evidence counts.
6. Cross-check against deployment_surfaces from inputs:
   - pipeline variables, release notes, Octopus/Helm values, appsettings Production
7. Flag drift: submodule sha differs from last documented deploy [if prior doc exists].

Outputs:
- docs/modularization/customer-versions/03-customer-inventory.md   (human table)
- docs/modularization/customer-versions/03-customer-inventory.yaml   (machine source)

YAML per customer:
customers:
  - tenant_id: acme
    customized_repo: "<url>"
    customized_repo_ref: "<branch or sha>"
    platform:
      version: "5.4.2"          # semver tag or NEEDS REVIEW
      core_sha: "<sha>"
      core_branch: release/5.4  # if detectable
    customization:
      profile_id: acme-prod
      client_sha: "<sha>"
      connectors:
        - id: plm-planning
          sha: "<sha>"
          version: "<tag if any>"
      tier: L2
      tier_evidence:
        derived_service_count: 12
        core_fork_detected: false
    environments:
      - name: production
        last_deployed_at: "<from CI or NEEDS REVIEW>"
        deployed_by: "<pipeline name>"
    support:
      tier: lts | current | eol | unknown
    drift_flags: []
```

### Acceptance criteria

- Every known customer has a row — no silent omissions.
- `UNKNOWN` and `[NEEDS REVIEW]` used instead of guessing.
- Customization tier has numeric evidence, not adjectives.

**Human review checkpoint:** validate inventory with PS/support before Phase D.

---

## Phase D — Runtime & operational version surfaces

### Agent prompt

```text
CUSTOMER VERSION VISIBILITY — PHASE D: Runtime & operational version surfaces.

Repos: core + one customized repo per tier (L0, L2, L3 if available).

Tasks:
1. Search for existing version reporting:
   - /health, /api/version, About page, footer in SPA, assembly metadata endpoint
   - log lines at startup containing version, git sha, build id
2. If none exist, identify lowest-friction injection point (ASP.NET middleware, assembly attribute, build target).
3. Map CI/CD: which pipeline deploys which customer; are submodule shas recorded as build artifacts?
4. Find support runbooks or wiki mentioning "which version" — extract gaps.
5. Propose minimum JSON shape for runtime version (do not implement yet):

{
  "platformVersion": "5.4.2",
  "coreSha": "...",
  "customizationProfileId": "acme-prod",
  "clientSha": "...",
  "connectors": { "plm-planning": "..." },
  "buildId": "...",
  "buildTime": "..."
}

Outputs:
- docs/modularization/customer-versions/04-runtime-surfaces.md
- docs/modularization/customer-versions/04-version-endpoint-proposal.json
```

### Acceptance criteria

- Existing surfaces catalogued with file:line.
- Gap between git inventory (Phase C) and production truth is explicit.

---

## Phase E — Support matrix & patch targeting

### Agent prompt

```text
CUSTOMER VERSION VISIBILITY — PHASE E: Support matrix & patch targeting.

Inputs: 01-branch-topology.yaml, 03-customer-inventory.yaml

Tasks:
1. Build support matrix:
   | Release | Branch | Latest tag | Status (current/lts/eol) | Customers count | Patch until |
2. For each customer, assign recommended patch target branch (release/X.Y).
3. Identify customers blocking patch rollout:
   - on eol release
   - L3 forked core
   - floating submodule branches
   - unknown platform version
4. Document fix workflow AS-IS (from team docs or interviews in repo issues/wiki):
   - bug filed → which branch → how backported → how deployed per customer
5. Propose TO-BE workflow (max 1 page):
   - fix on release/X.Y → tag vX.Y.Z → deploy customers on matrix → cherry-pick main
6. List orphan patch branches that should be merged or deleted.

Outputs:
- docs/modularization/customer-versions/05-support-matrix.md
- docs/modularization/customer-versions/05-patch-targeting.yaml
```

### Acceptance criteria

- Every customer maps to exactly one platform release line or `NEEDS UPGRADE`.
- AS-IS vs TO-BE patch flow is actionable for support.

**Human review checkpoint:** support lead signs off matrix.

---

## Phase F — Visibility solution design

### Agent prompt

```text
CUSTOMER VERSION VISIBILITY — PHASE F: Solution design.

Synthesize Phases A–E into an implementable visibility program.

Deliver:

1. **Deployment registry** — canonical store (recommend: git-backed YAML in platform-ops repo OR control plane DB table). Schema from 03-customer-inventory.yaml; include audit fields.

2. **Drift detection** — nightly job comparing:
   - registry intended state
   - live /api/version (or equivalent) per environment
   - optional: re-scan customized repo submodule SHAs

3. **Developer/support UX**
   - CLI or admin page: `f2p-customer-version acme`
   - support ticket template fields: tenant_id, platform_version, customization_profile_id
   - Azure DevOps/Jira dashboard linking customers → release line

4. **Automation hooks**
   - on deploy: pipeline writes registry row + tags artifact with full composition sha bundle
   - on submodule bump in customized repo: PR check warns if platform version unsupported

5. **V2 alignment** (reference_only from SandBox)
   - map customization_profile → future packEntitlements list
   - legacy_hosted metadata until cutover (platform-architecture-overview.md)

6. **Anti-patterns to reject** — cite evidence from this analysis

Outputs:
- docs/modularization/customer-versions/06-solution-design.md
- docs/modularization/customer-versions/06-registry-schema.yaml
- docs/modularization/customer-versions/06-implementation-backlog.md  (ordered stories)

Backlog story format:
- id, title, owner (platform|ps|support), depends_on, acceptance criteria
```

### Acceptance criteria

- Solution does not require big-bang V2 migration to deliver value in phase 1.
- Registry schema matches Phase C YAML (no parallel invented format).
- At least one "week 1" quick win identified (e.g. inventory YAML + spreadsheet replacement).

---

## Phase G — Implementation starter (optional)

Only after human approval of Phase F.

### Agent prompt

```text
CUSTOMER VERSION VISIBILITY — PHASE G: Implementation starter.

Implement the smallest slice approved in 06-implementation-backlog.md:

Typical slice (adjust to backlog):
1. Add assembly/build metadata injection in core (git sha, semver).
2. Add GET /api/platform/version (or extend health) returning 04-version-endpoint-proposal.json shape.
3. Add scripts/inventory-customized-repo.sh — parses .gitmodules + git submodule status → YAML snippet.
4. Add docs/modularization/customer-versions/README.md — how to refresh inventory.

Rules:
- No per-customer branches created.
- Script output must be deterministic and suitable for CI.
- Tests for version endpoint if host has test project.

Output: PR(s) with evidence paths; update 03-customer-inventory.yaml via script on pilot customer.
```

---

## Quick prompts (ad-hoc)

### Inventory one customer

```text
Customized repo at "<path>": extract full submodule composition (sha, date, subject).
Derive platform version from core sha. Score customization tier L0–L3 with counts.
Output YAML snippet for 03-customer-inventory.yaml.
```

### Where to patch a bug

```text
Bug affects customers [acme, contoso]. Using 03-customer-inventory.yaml and 05-support-matrix.yaml:
which release branch to fix, which tag to cut, which customers need deploy, cherry-pick to main?
```

### Compare two customers

```text
Diff acme vs contoso customized repos: core sha, client sha, connector set, derived service count.
What breaks if we bump acme core to contoso's core sha?
```

### Drift check

```text
Registry says acme production is platform 5.4.2 / core sha X.
Live /api/platform/version returns Y. List drift and recommended remediation.
```

---

## First iteration starter pack

Minimum viable deep-dive when time-boxed:

1. **Phase A** — core branch/tag topology
2. **Phase B** — one representative customized repo template
3. **Phase C** — top 5 production customers (highest support volume)
4. **Phase E** — support matrix for those 5
5. **Phase F** — registry schema + backlog (no code unless approved)

Expand to all customers and Phase D/G after format validation with support.

---

## Artifact index

| File | Phase | Purpose |
|------|-------|---------|
| `00-blockers.md` | — | Missing access / inputs |
| `01-branch-topology.md` | A | Core release branches & tags |
| `02-customized-repo-anatomy.md` | B | Submodule template |
| `03-customer-inventory.md` | C | Human-readable customer table |
| `03-customer-inventory.yaml` | C | Machine source of truth |
| `04-runtime-surfaces.md` | D | Existing version reporting |
| `05-support-matrix.md` | E | LTS/current/eol + patch targets |
| `06-solution-design.md` | F | Registry + drift + UX |
| `06-implementation-backlog.md` | F | Ordered delivery stories |

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-30 | Initial deep-dive for customer version visibility across customized repos |
