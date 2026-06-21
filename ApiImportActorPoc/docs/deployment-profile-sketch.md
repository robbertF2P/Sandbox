# Minimum deployment profile sketch

v2 is a **control plane**. A tenant's **deployment profile** decides whether users hit the **full legacy product** or the **native v2 runtime**. Legacy mode is intentional technical debt in a box — not a failed migration.

**Authentication (design):** SSO binding, OIDC flows, cloud multi-tenant vs on-prem install config — see `docs/monolith-modularization/platform-authentication-standard.md`. **PDF:** `docs/monolith-modularization/platform-v2-authentication-flow.pdf`.

## Responsibilities

```
                         ┌─────────────────────────────────────┐
                         │  v2 control plane (always)          │
                         │  · tenant registry                  │
                         │  · deployment profile               │
                         │  · pack entitlements (metadata)     │
                         │  · auth / SSO binding               │
                         │  · edge routing                     │
                         │  · provisioning jobs                │
                         │  · billing stub (tier, seats)       │
                         └──────────────┬──────────────────────┘
                                        │
              ┌─────────────────────────┴─────────────────────────┐
              ▼                                                   ▼
   ┌──────────────────────────┐                    ┌──────────────────────────┐
   │  Legacy-hosted tenant    │                    │  Native tenant           │
   │  · full legacy app       │                    │  · v2 API + SPA          │
   │  · legacy DB             │                    │  · canonical schema      │
   │  · legacy submodules     │                    │  · enabled packs         │
   │  · all current pages     │                    │  · import pipeline       │
   └──────────────────────────┘                    └──────────────────────────┘
```

| Concern | v2 control plane | Legacy-hosted runtime | Native runtime |
|---------|------------------|----------------------|----------------|
| Product UI | Routes only | Owns 100% | Owns 100% |
| Domain logic | None | Legacy services/handlers | v2 core + packs |
| Database | Connection metadata | Legacy DB (per tenant) | Canonical schema in **tenant's own DB** |
| Customization | Pack IDs + flags | Submodule/build profile | Versioned packs |
| Auth | IdP, tenant binding | Legacy session or shared SSO | v2 auth |
| Provisioning | Creates profile + runtime | Deploy legacy instance/DB | Create tenant row + DB |
| Migration | Orchestrates cutover | Source of truth until cutover | Target after cutover |

## Minimum tenant record

One row in the control-plane DB. Keep it small.

```json
{
  "tenantId": "3f2e9b1a-8c4d-4e5f-9a0b-1c2d3e4f5a6b",
  "slug": "acme-shipyard",
  "displayName": "Acme Shipyard",
  "status": "active",
  "deploymentProfile": {
    "mode": "legacy_hosted",
    "dataTier": "dedicated_sql_server",
    "region": "eu-west",
    "legacy": {
      "buildProfileId": "acme-onprem-2024",
      "runtimeUrl": "https://acme-shipyard.legacy.internal",
      "databaseConnectionRef": "vault:tenants/acme-shipyard/legacy-db"
    },
    "native": null
  },
  "packEntitlements": {
    "integrationPacks": ["sap-v1"],
    "customizationPacks": ["acme-rules-v1"]
  },
  "migration": {
    "phase": "none",
    "targetMode": null,
    "lastExportAt": null,
    "cutoverAt": null
  },
  "billing": {
    "tier": "enterprise",
    "seatLimit": 50
  }
}
```

Native tenant — same shape, `mode: "native"`, `legacy: null`, `native` populated:

```json
"deploymentProfile": {
  "mode": "native",
  "dataTier": "shared_sql_server",
  "region": "eu-west",
  "legacy": null,
  "native": {
    "databaseConnectionRef": "vault:tenants/acme-shipyard/native-db",
    "apiBaseUrl": "https://api.platform.example/v1"
  }
}
```

## Deployment profile fields (minimum)

### Required for every tenant

| Field | Type | Purpose |
|-------|------|---------|
| `mode` | `legacy_hosted` \| `native` | Where user traffic goes |
| `dataTier` | `shared_sql_server` \| `dedicated_sql_server` | SQL Server instance tier — **one database per tenant always**; shared tier = multiple tenant DBs on one server |
| `region` | string | Data residency / latency |

### Legacy-hosted only

| Field | Type | Purpose |
|-------|------|---------|
| `buildProfileId` | string | Which legacy variant (submodule set, config bundle) |
| `runtimeUrl` | string | Edge proxy target for this tenant |
| `databaseConnectionRef` | secret ref | Legacy SQL Server — never inline secrets |

### Native only

| Field | Type | Purpose |
|-------|------|---------|
| `databaseConnectionRef` | secret ref | This tenant's native database (dedicated DB; server may be shared or dedicated) |
| `apiBaseUrl` | string | Versioned platform API for this tenant |

### Pack entitlements (metadata in v2; enforcement in runtime)

| Field | Type | Purpose |
|-------|------|---------|
| `integrationPacks` | string[] | SAP, Kronos, PLM, … |
| `customizationPacks` | string[] | Client rules (native) or documentation of legacy submodule mapping |

In **legacy_hosted** mode, packs are **documentation + provisioning hints** until native cutover. The legacy build profile still carries the real code variant.

### Migration block (optional until cutover)

| Field | Type | Purpose |
|-------|------|---------|
| `phase` | `none` \| `exporting` \| `dry_run` \| `importing` \| `cutover` \| `rolled_back` | Orchestration state |
| `targetMode` | `native` | Always native when migrating |
| `lastExportAt` | datetime | Last successful legacy export |
| `cutoverAt` | datetime | When routing flipped to native |

## Tenant status (lifecycle)

```
provisioning → active ⇄ suspended
                  │
                  └── migrating → active (native) | rolled_back (legacy_hosted)
```

| Status | Meaning |
|--------|---------|
| `provisioning` | DB/runtime being created; not routable |
| `active` | Users can work (legacy or native per `mode`) |
| `suspended` | Routed to maintenance; data retained |
| `migrating` | Cutover in progress; read-only or dual-run rules apply |
| `retired` | Offboarded; backups only |

## Edge routing (simplest rule)

```
Request: https://{slug}.app.example/...
         │
         ▼
   Resolve tenant by slug (control plane)
         │
         ├── mode = legacy_hosted  →  proxy to deploymentProfile.legacy.runtimeUrl
         │
         └── mode = native         →  v2 SPA + API (tenant context from JWT / header)
```

No mixed mode per tenant at the edge. One tenant, one runtime.

## Provisioning flows

### A. Legacy-hosted (default for unmigrated clients)

1. Create tenant row: `mode = legacy_hosted`, `status = provisioning`
2. Resolve `buildProfileId` → legacy artefact (image + submodule manifest + config)
3. Provision dedicated DB (or attach existing on-prem connector)
4. Run legacy install/migrate seed
5. Store `runtimeUrl` + `databaseConnectionRef`
6. Bind SSO in control plane
7. `status = active` — users get **full legacy product**

### B. Native (pilot / post-migration)

1. Create tenant row: `mode = native`, `status = provisioning`
2. Provision native database for the tenant (own DB on shared or dedicated SQL Server instance)
3. Apply enabled packs in control plane
4. Optional: import from intermediate format
5. `status = active` — users get **native SPA only**

### C. Legacy → native cutover (later)

1. `status = migrating`, `migration.phase = exporting`
2. Legacy export → intermediate format (all projects or agreed scope)
3. `phase = dry_run` → native import validation report
4. `phase = importing` → persist on native
5. Maintenance window: `mode = native`, update routing, `cutoverAt = now`
6. `status = active`, `migration.phase = none`, decommission legacy runtime when confident

Rollback: flip `mode` back to `legacy_hosted`, `migration.phase = rolled_back`, restore routing.

## What v2 must implement first (Sprint-sized)

| Order | Deliverable | Outcome |
|-------|-------------|---------|
| 1 | Tenant registry + deployment profile schema | Can describe a tenant |
| 2 | `POST /admin/tenants` (internal) | Provision staging tenant |
| 3 | Edge resolver: slug → `mode` + target URL | Route to legacy or native |
| 4 | Legacy-hosted provisioning job | One full legacy staging tenant |
| 5 | Native dry-run import (existing POC) | Second staging tenant on native |

Do **not** build backoffice UI, billing integration, or pack runtime in sprint 1. JSON API + scripts is enough.

## Explicit non-goals (v0 profile)

- Per-screen mode inside one tenant
- Bidirectional sync between legacy and native DBs
- Inline secrets in profile JSON
- Automatic legacy → native without export/import
- Retiring `legacy_hosted` mode (set sunset policy later; keep the option)

## Internal language (avoid confusion)

| Say | Don't say |
|-----|-----------|
| "Tenant is **legacy-hosted on v2**" | "Tenant migrated to v2" |
| "Tenant is **native on v2**" | "Fully modernized" (until adapters are gone) |
| "v2 **control plane**" | "v2 product" (when only hosting legacy) |

## Exit criteria for legacy_hosted (publish later)

Example policy to prevent "two systems forever":

- No **new** commercial tenants on `legacy_hosted` after date X
- Every `legacy_hosted` tenant has a named cutover quarter or exception recorded
- `buildProfileId` catalogue shrinks — no new client-specific legacy profiles

Legacy-hosted remains supported for existing tenants; it is not the default end state.
