# Platform UI customization standard

Tenant-specific display variance for V2 modules — **view schemas** and **extension projections** — without bloating core domain models or forking UI per client.

**Related:** `platform-frontend-standard.md`, `platform-actor-standard.md`, `floor2plan-v2-read-model-playbook.md`, `F2pPlatform/README.md` (module ↔ frontend index).

**Reference implementation:** Hour Approvals slice — `HourApprovals.Packs.Acme`, `Platform.Shared.View`, `F2pPlatform/web/libs/hour-approvals/`.

---

## Problem

Legacy per-client builds often solve display variance by:

- Adding optional fields to core entities and hiding them in Razor/Vue
- Subclassing services (`AcmePlanningService : PlanningService`)
- Forking UI bundles per tenant

V2 replaces that with **versioned customization packs** enabled per tenant via the control plane (`packEntitlements.customizationPacks`).

---

## Three artifacts (keep separate)

| Artifact | Owns | Must not |
|----------|------|----------|
| **Core domain** | Ubiquitous language, invariants, workflow state | Tenant-specific optional columns |
| **Read projection (DTO)** | Screen-shaped API payload from domain + specs | Arbitrary client-only fields in aggregates |
| **View definition (pack)** | Column visibility, order, labels, format hints | Business rules or persistence |

```text
Domain aggregate          Query handler / composer       Customization pack
(canonical fields)   →    (read DTO, core columns)  +   (view schema + extensions)
                                    │
                                    ▼
                          API: { view, items[{ …, extensions }] }
                                    │
                                    ▼
                          Schema-driven Angular feature
```

---

## Field categories

| Category | Example | Stored in | Pack controls |
|----------|---------|-----------|---------------|
| **Core** | `progress`, `hoursToGo`, `plannedStart` | Domain + read DTO | Visibility, label, order, required UX |
| **Extension** | SAP cost element, client WBS code | Pack-owned store → projected into `extensions` | Column definition + value mapping |
| **Computed** | Hours since last submission | Query handler | Column definition only |

**Rule:** Client-only fields never enter core aggregates. When a field becomes universal, promote it from `extensions` to the read DTO in a **versioned API change** — do not pre-emptively add it to the domain.

---

## Pack contract (per bounded context)

Each module defines an Application **port** — not a shared mega-interface:

```csharp
// HourApprovals.Application/Ports/IHourApprovalsCustomizationPack.cs
public interface IHourApprovalsCustomizationPack
{
    string PackId { get; }

    ViewDefinition GetView(string screenId);

    IReadOnlyDictionary<string, object?> GetRowExtensions(Guid taskId);
}
```

| Method | Purpose |
|--------|---------|
| `GetView(screenId)` | Returns `ViewDefinition` for a screen (e.g. `hour-approvals-queue`) |
| `GetRowExtensions(entityId)` | Tenant-specific values keyed by extension column id |

Pack implementations live in `src/Packs/<Context>.Packs.<Client>/`. The host registers them in `Program.cs`. Default (no-op) pack ships in module Infrastructure.

**Forbidden in packs:** references to core Domain types, `DbContext`, `if (tenant == …)` in core Application code.

---

## Shared view types (`Platform.Shared.View`)

| Type | Role |
|------|------|
| `ViewDefinition` | Screen id + ordered columns |
| `ColumnDef` | `Id`, `Label`, `Source`, `Visible`, `Order`, optional `Format` |
| `ColumnSource` | `Core`, `Extension`, `Computed` |

Column `Id` maps to:

- Core/computed: JSON path on the read DTO (e.g. `currentValues.plannedStart`)
- Extension: key under `extensions` on each row (e.g. `sapCostElement`)

---

## API shape

### Capabilities (per screen or module)

```http
GET /api/hour-approvals/capabilities
```

```json
{
  "featureEnabled": true,
  "customizationPackId": "acme-hour-approvals-v1",
  "queueView": {
    "screenId": "hour-approvals-queue",
    "columns": [
      { "id": "hoursToGo", "label": "Hours to go", "source": "Core", "visible": true, "order": 10 },
      { "id": "plannedStart", "label": "Planned start", "source": "Core", "visible": true, "order": 30 },
      { "id": "sapCostElement", "label": "SAP cost element", "source": "Extension", "visible": true, "order": 50 }
    ]
  },
  "canApprove": true,
  "permissions": ["ApproveHoursProgress"]
}
```

### List / queue rows

```json
{
  "taskId": "…",
  "currentValues": { "hoursToGo": 12.5, "plannedStart": "2026-06-10" },
  "extensions": { "sapCostElement": "CE-4401" }
}
```

Core DTO stays lean; extensions bag is optional and pack-defined.

---

## Frontend conventions

1. **Load view schema once** — `GET /capabilities` (or screen-specific endpoint) before rendering.
2. **Schema-driven columns** — feature component iterates `queueView.columns` where `visible`; bind core paths or `extensions[id]`.
3. **No tenant branches in Angular** — `if (tenant === 'acme')` is forbidden; use pack-delivered schema.
4. **Presentational widgets** in `libs/<context>/ui/`; schema iteration stays in `feature-*`.

TypeScript mirrors `Platform.Shared.View`:

```typescript
export interface ViewDefinitionDto {
  screenId: string;
  columns: ColumnDefDto[];
}
```

---

## Module ↔ frontend discoverability

Backend and SPA libs are **paired by context name** but live in parallel trees:

| Backend | Frontend | API prefix | SPA route |
|---------|----------|------------|-----------|
| `src/Modules/HourApprovals/` | `web/libs/hour-approvals/` | `/api/hour-approvals` | `/hour-approvals` |
| `src/Modules/Reference/` | `web/libs/reference/` | `/api/reference` | `/reference` |

Each backend module includes **`MODULE.md`** with the frontend path, routes, and pack port name.

Scaffold both sides together:

```bash
./scripts/scaffold-module.sh Planning
./scripts/scaffold-frontend-module.sh Planning
```

---

## Control plane wiring

Tenant profile stores enabled customization packs (see `TenantPackEntitlements.CustomizationPacks`). Host resolves the active pack implementation at startup or per-request from configuration — same pattern as integration packs.

```text
Admin backoffice → tenant.customizationPacks: ["acme-hour-approvals-v1"]
                → host registers AddAcmeHourApprovalsPack()
```

---

## Promotion path (extension → core)

1. Field used by **one** tenant → extension column + pack projection.
2. Field needed by **most** tenants → add to read DTO; pack toggles visibility.
3. Field part of **workflow/invariants** → promote to domain value object; pack controls display only.

---

## Checklist (new screen with tenant variance)

**Backend**

- [ ] Read DTO contains only core/canonical fields
- [ ] `I<Context>CustomizationPack` port in Application
- [ ] Default pack in Infrastructure; client pack in `src/Packs/`
- [ ] Capabilities endpoint returns `ViewDefinition`
- [ ] List endpoint includes `extensions` when pack defines extension columns
- [ ] No tenant branches in Domain/Application
- [ ] Characterization test: default pack vs client pack column sets

**Frontend**

- [ ] `data-access` DTOs include `view` + `extensions`
- [ ] Feature renders columns from schema (no hard-coded tenant checks)
- [ ] `MODULE.md` links backend ↔ `web/libs/<context>/`

**Control plane**

- [ ] Pack id registered in tenant provisioning UI
- [ ] Host `Program.cs` documents pack registration

---

## Anti-patterns

| Anti-pattern | Instead |
|--------------|---------|
| 50 optional fields on `Activity`, hide in UI | Core DTO + `extensions` bag |
| `if (tenantSlug == "acme")` in Angular | Pack view schema |
| Copy-paste hour-approvals page per client | Customization pack + shared feature component |
| Pack references `Planning.Domain` entity | Pack maps to DTO / extension dictionary only |
| View schema in database without version | Versioned pack id + manifest in pack assembly |

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-27 | Initial standard; `Platform.Shared.View`; Hour Approvals Acme pack POC |
