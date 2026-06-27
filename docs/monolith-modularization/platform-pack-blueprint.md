# Platform pack blueprint

**Purpose:** Decision guide and scaffold for V2 **packs** — what can go in them, why, and where each artifact belongs.

**Related:** `platform-ui-customization-standard.md`, `platform-actor-standard.md`, `floor2plan-v2-connector-architecture.md`, `tenant-workflow-fields-deepdive-instructions.md`, `starter-kit/templates/customization-pack/`.

**Reference implementation:** `F2pPlatform/src/Packs/HourApprovals.Packs.Acme/` (`acme-hour-approvals-v1`).

---

## Pack types (pick one primary role per deployable unit)

| Pack type | Ships | Solves | Example id |
|-----------|-------|--------|--------------|
| **Customization pack (UI)** | View schemas, extension projections, i18n | Tenant-specific columns, labels, visibility | `acme-hour-approvals-v1` |
| **Customization pack (rules)** | Validation, workflow gates, actor pipeline stages | Tenant-specific approval/export rules | `acme-planning-rules-v1` |
| **Integration pack** | Vendor fetch/map, canonical DTO emission | SAP, Kronos, PLM, file connectors | `sap-projects-v1` |
| **Strangler adapter** *(not a pack)* | Legacy delegation behind a port | `legacy_hosted` tenants during cutover | `[StranglerAdapter]` in Infrastructure |

**Rule:** Do not mix integration vendor SDKs into a UI customization pack. Do not put domain invariants in any pack — promote to core when semantics are universal.

---

## What can go in a pack (artifact catalog)

Use this table when migrating legacy `Text*` / `Bool*` fields or designing a new tenant profile.

| Artifact | Pack type | What it does | When to use | When **not** to use |
|----------|-----------|--------------|-------------|---------------------|
| **ViewDefinition** | Customization (UI) | Column list: id, `labelKey`, source, visibility, order, format | Any screen with tenant column variance | Business rules or persistence |
| **Core column toggles** | Customization (UI) | Show/hide/reorder core fields via `ColumnSource.Core` | Tenant wants different default layout | Field is not in core read DTO — use extension |
| **Extension column defs** | Customization (UI) | `ColumnSource.Extension` + key under row `extensions` | Client-only semantics (WBS code, cost element) | Field is universal → promote to read DTO |
| **Computed column toggles** | Customization (UI) | `ColumnSource.Computed` — visibility/order only | Tenant shows/hides derived columns | Value calculation — belongs in query mapper |
| **`GetRowExtensions` (batch)** | Customization (UI) | `Dictionary<entityId, extensions>` for one list page | Extension values for visible rows | Per-row I/O; domain service calls |
| **Extension batch loader** | Customization (UI) | Infrastructure in pack: single `WHERE id IN (...)` query | DB-backed extension store | N+1 lazy loads |
| **Filter spec / filter port** | Customization (UI) | Named list filters on extension fields | Legacy `text2` used only in search | Opaque `?text1=` query params |
| **i18n label bundles** | Customization (UI) | `packs.<pack-id>.columns.*` locale JSON/TS | Every extension column + tenant terminology | Translated strings in C# |
| **Capabilities flags** | Customization (UI) | Extra permissions or feature toggles in `/capabilities` | Tenant enables actions beyond default | Workflow state machines |
| **TenantRulesActor stage** | Customization (rules) | Pipeline stage: validate, enrich, block | `Bool3` gates approval/export | Invariant all tenants need → domain policy |
| **Pack pipeline registration** | Customization (rules) | `RegisterPackPipeline("pack-id", …)` | Composed Akka workflows | Direct `DbContext` in actor |
| **Vendor fetch actor** | Integration | HTTP/file read from external system | Inbound/outbound vendor protocol | UI column layout |
| **Map-to-canonical actor** | Integration | Vendor types → canonical exchange JSON | Every connector | Core domain types |
| **Pack manifest (`PACK.md`)** | All | Id, version, entitlements, screens, dependencies | Every shipped pack | — |
| **DI extension** | All | `Add<Client><Context>Pack()` | Host composition root registration | Inside core Application |

---

## Placement decision tree

```text
Tenant-specific behavior needed?
├─ NO → Core module only (default pack may hide optional columns)
└─ YES
   ├─ External system is source/sink?
   │  └─ Integration pack (vendor SDK allowed)
   ├─ List/detail column or filter variance only?
   │  └─ Customization pack (UI): ViewDefinition + extensions
   ├─ Workflow branch / validation gate?
   │  └─ Customization pack (rules): TenantRulesActor or Application port
   ├─ Legacy column during strangler?
   │  └─ Strangler adapter maps legacy ↔ extensions bag
   └─ Used by ALL tenants with same meaning?
      └─ Promote to core read DTO / domain — pack controls display only
```

---

## Customization pack anatomy (UI)

```text
<Context>.Packs.<Client>/
├── PACK.md                              manifest (required)
├── <Context>.Packs.<Client>.csproj      refs Application + Shared.View only
├── <Client><Context>Pack.cs             implements I<Context>CustomizationPack
├── DependencyInjection.cs               Add<Client><Context>Pack()
└── Infrastructure/                      optional — DB-backed extension loaders
    └── <Extension>BatchLoader.cs

web/libs/<context>/data-access/
└── src/lib/<context>.i18n.ts            packs.<pack-id>.* keys (or separate npm fragment)

tests/Modules/<Context>/
└── <Context>.Unit.Tests/
    └── <Client><Context>PackShould.cs   default vs client column assertions
```

### Port contract (per bounded context)

Each module owns its port in Application — not a shared mega-interface:

```csharp
public interface I<Context>CustomizationPack
{
    string PackId { get; }
    ViewDefinition GetView(string screenId);
    IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> GetRowExtensions(
        IReadOnlyList<Guid> entityIds);
}
```

Extend the port only when the module needs more than UI (e.g. `GetFilterSpecs()`, `ApplyApprovalRules()`). Prefer separate ports over one god-interface.

### Field source reference

| `ColumnSource` | Value location | Who computes the value |
|----------------|----------------|------------------------|
| `Core` | Read DTO path (e.g. `currentValues.plannedStart`) | List query / spec |
| `Extension` | Row `extensions` bag | Pack `GetRowExtensions` (batched) |
| `Computed` | Row `computed` bag | List mapper / SQL projection — pack sets visibility only |

---

## Customization pack anatomy (rules)

When legacy code used `Bool*` for approval gates or export eligibility:

```text
<Context>.Packs.<Client>.Rules/          or same pack project if small
├── <Client><Context>RulesActor.cs       Akka stage
└── <Client><Context>RulePort.cs         optional Application port for non-actor callers

Host Program.cs:
  registry.RegisterPackPipeline("<pack-id>", b => b.AddStage<<Client>RulesActor>());
```

| Legacy pattern | V2 artifact |
|----------------|-------------|
| `if (activity.Bool3) block export` | Rules pack stage before outbound actor |
| `AcmePlanningService : PlanningService` | Core service + rules pack pipeline |
| Tenant `if` in Application | Pack port implementation |

---

## Integration pack anatomy (summary)

Full detail: `floor2plan-v2-connector-architecture.md`.

```text
F2P.Integration.Packs.<Vendor>.<Domain>/
├── PACK.md
├── FetchActor / FileWatcher
├── MapToCanonicalActor
└── DependencyInjection.cs               Add<Vendor><Domain>Pack()
```

**Compile-time rule:** pack → `Integration.Abstractions` + vendor SDKs only. Never `*.Domain` or `DbContext`.

---

## Pack manifest (`PACK.md`)

Every shipped pack includes a manifest at the project root. Template: `starter-kit/templates/customization-pack/PACK.md`.

Minimum fields:

| Field | Example | Purpose |
|-------|---------|---------|
| `packId` | `acme-hour-approvals-v1` | Control plane entitlement string |
| `packType` | `customization-ui` | `customization-ui` \| `customization-rules` \| `integration` |
| `context` | `HourApprovals` | Bounded context module |
| `client` | `Acme` | Owning tenant/client (or `platform` for shared) |
| `version` | `1.0.0` | Semver of this artifact |
| `screens` | `hour-approvals-queue` | Screen ids served by `GetView` |
| `modulePort` | `IHourApprovalsCustomizationPack` | Application port name |
| `hostRegistration` | `AddAcmeHourApprovalsPack()` | DI method host must call |
| `i18nPrefix` | `packs.acme-hour-approvals-v1` | Frontend label key prefix |
| `dependencies` | `HourApprovals.Application` | Project/package refs |

Bump `packId` suffix (`v1` → `v2`) when breaking extension keys or column ids.

---

## Runtime wiring

```text
Control plane                    Host (composition root)              Module
─────────────────────────────────────────────────────────────────────────────
tenant.packEntitlements          AddAcmeHourApprovalsPack()           AddHourApprovalsModule()
  .customizationPacks:           registers IHourApprovals           default pack if none registered
  ["acme-hour-approvals-v1"]       CustomizationPack → Acme impl
        │                                    │
        └──────── packId echoed in ──────────┴── GET /capabilities
                                                 customizationPackId + view schema
```

| Deployment mode | Pack enforcement |
|-----------------|------------------|
| `native` | Host loads entitled packs; API returns active `packId` |
| `legacy_hosted` | Pack ids are metadata until cutover; legacy build profile still owns code |

---

## Distribution

| Audience | Mechanism |
|----------|-----------|
| SandBox / dev | Project reference in `F2pPlatform.Host` |
| Native multi-tenant | NuGet to internal feed; host references entitled packages |
| Client-owned variance | Client feed or git submodule of pack repo only (not core) |
| Frontend i18n | Merged in module lib (POC) or `@client/<context>-pack-i18n` npm package |

---

## Scaffold a new customization pack

```bash
./scripts/scaffold-customization-pack.sh HourApprovals Acme acme-hour-approvals-v1
```

Then:

1. Fill `PACK.md` field inventory and extension column list.
2. Implement `GetView` per screen; add `GetRowExtensions` if using `ColumnSource.Extension`.
3. Add `packs.<pack-id>.*` keys to frontend i18n.
4. Register `Add<Client><Context>Pack()` in host `Program.cs`.
5. Add characterization tests (default vs client column sets).
6. Entitle tenant in control plane: `customizationPacks: ["<pack-id>"]`.

---

## Checklist (new pack)

### Design

- [ ] Classified each tenant field: display / filter / workflow / integration (`tenant-workflow-fields-deepdive-instructions.md`)
- [ ] Chosen pack type; no vendor SDK in UI pack
- [ ] `packId` is stable, versioned, registered in control plane

### Backend

- [ ] Port in `<Context>.Application`; default pack in Infrastructure
- [ ] Pack project references Application only (optional Infrastructure folder inside pack for batch loaders)
- [ ] `GetRowExtensions` is batched — one call per list response
- [ ] Computed values in list mapper, not pack per-row
- [ ] `PACK.md` complete
- [ ] Unit tests: label keys, visibility, extension shape

### Frontend

- [ ] Schema-driven columns from `/capabilities` — no `if (tenant)` branches
- [ ] i18n for every `packs.<pack-id>.*` label key

### Ops

- [ ] Host documents pack id → `Add*Pack()` mapping
- [ ] NuGet publish path defined (if not monorepo project ref)

---

## Anti-patterns

| Anti-pattern | Use instead |
|--------------|-------------|
| `Text17` on domain entity | Extension bag + pack projection |
| English labels in pack C# | `labelKey` + locale bundle |
| `GetRowExtensions` inside `rows.Select` | Single batch call with all page ids |
| SAP SDK in customization pack | Integration pack |
| One pack for UI + Kronos + rules | Split by pack type |
| `if (tenantSlug == "acme")` in core | Pack port or pipeline registration |

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-27 | Initial blueprint; customization pack templates; scaffold script |
