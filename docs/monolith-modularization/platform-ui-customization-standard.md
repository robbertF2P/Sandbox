# Platform UI customization standard

Tenant-specific display variance for V2 modules — **view schemas** and **extension projections** — without bloating core domain models or forking UI per client.

**Related:** `platform-frontend-standard.md`, `platform-actor-standard.md`, `floor2plan-v2-read-model-playbook.md`, `tenant-workflow-fields-deepdive-instructions.md` (legacy `Text*` / `Bool*` analysis), `F2pPlatform/README.md` (module ↔ frontend index).

**Reference implementation:** Hour Approvals slice — `HourApprovals.Packs.Acme`, `Platform.Shared.View`, `F2pPlatform/web/libs/hour-approvals/`.

**Pack blueprint (artifact catalog, scaffold):** `platform-pack-blueprint.md`.

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
| **View definition (pack)** | Column visibility, order, **label keys**, format hints | Business rules, persistence, or translated strings |

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
| **Core** | `progress`, `hoursToGo`, `plannedStart` | Domain + read DTO | Visibility, **labelKey**, order, required UX |
| **Extension** | SAP cost element, client WBS code | Pack-owned store → projected into `extensions` | Column definition + value mapping |
| **Computed** | Days since last submission | Query handler / list mapper (`computed` bag) | Column definition only — **not** per-row pack logic |

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

    /// Batch only — one call per list response (see Performance).
    IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> GetRowExtensions(
        IReadOnlyList<Guid> taskIds);
}
```

| Method | Purpose |
|--------|---------|
| `GetView(screenId)` | Returns `ViewDefinition` for a screen (e.g. `hour-approvals-queue`) |
| `GetRowExtensions(taskIds)` | Tenant-specific extension values keyed by entity id — **batched** |

Pack implementations live in `src/Packs/<Context>.Packs.<Client>/`. The host registers them in `Program.cs`. Default (no-op) pack ships in module Infrastructure.

**Forbidden in packs:** references to core Domain types, `DbContext`, `if (tenant == …)` in core Application code.

---

## Shared view types (`Platform.Shared.View`)

| Type | Role |
|------|------|
| `ViewDefinition` | Screen id + ordered columns |
| `ColumnDef` | `Id`, **`LabelKey`**, `Source`, `Visible`, `Order`, optional `Format` |
| `ColumnSource` | `Core`, `Extension`, `Computed` |

Column `Id` maps to:

- Core: JSON path on the read DTO (e.g. `currentValues.plannedStart`)
- Extension: key under `extensions` on each row (e.g. `sapCostElement`)
- Computed: key under `computed` on each row (e.g. `daysSinceLastSubmission`) — values produced by the **list query/mapper**, not the pack

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
      { "id": "hoursToGo", "labelKey": "hourApprovals.columns.hoursToGo", "source": "Core", "visible": true, "order": 10, "format": "decimal" },
      { "id": "plannedStart", "labelKey": "hourApprovals.columns.plannedStart", "source": "Core", "visible": true, "order": 30, "format": "date" },
      { "id": "sapCostElement", "labelKey": "packs.acme-hour-approvals-v1.columns.sapCostElement", "source": "Extension", "visible": true, "order": 50 },
      { "id": "daysSinceLastSubmission", "labelKey": "hourApprovals.columns.daysSinceLastSubmission", "source": "Computed", "visible": true, "order": 60, "format": "integer" }
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
  "extensions": { "sapCostElement": "CE-4401" },
  "computed": { "daysSinceLastSubmission": 14 }
}
```

Core DTO stays lean; `extensions` and `computed` bags are optional and typed per screen.

---

## Internationalization (i18n)

**Packs declare `labelKey`, not translated text.** Wording lives in locale bundles; formatting uses BCP 47 locale + `Intl` on the client.

### Three layers

| Layer | Owns | Example |
|-------|------|---------|
| **Pack view schema** | Stable `labelKey` per column | `hourApprovals.columns.plannedStart` |
| **Locale bundles** | Translated strings | `libs/hour-approvals/data-access/i18n/nl.json` |
| **Locale + format** | Date/number display | `Intl.DateTimeFormat`, `Intl.NumberFormat` |

### Label key conventions

| Key prefix | Used for |
|------------|----------|
| `hourApprovals.columns.*` | Core module columns (all tenants) |
| `packs.<pack-id>.columns.*` | Pack extension columns only |
| Tenant terminology override | Rare rename via control plane — still keyed, not inline strings |

### Locale resolution

```text
User profile locale (Identity)     ← target
  ↓ if unset
Tenant default locale (control plane)
  ↓ if unset
navigator.language / Accept-Language
  ↓
en (platform fallback)
```

Supported product locales (initial): **en**, **nl**, **es**, **ja**, **vi** (BCP 47: `nl-NL`, `en-GB`, etc. map to primary language).

### API data vs display

| On the wire (invariant) | In the UI (locale-aware) |
|-------------------------|---------------------------|
| `"plannedStart": "2026-06-10"` | `24/06/2026` (nl) or `6/24/2026` (en-US) |
| `"hoursToGo": 1234.5` | `1.234,5` (nl) or `1,234.5` (en-US) |
| `"progress": 75` + `format: "percent"` | `75 %` / `75%` via `Intl` |

**Never** put locale-formatted numbers or dates in core JSON. The pack `format` field is semantic (`date` | `decimal` | `percent` | `integer`) — not a Excel-style pattern per tenant.

### Frontend (Hour Approvals POC)

- `hour-approvals.i18n.ts` — core + Acme pack bundles for en/nl/es/ja/vi
- `translateHourApprovalsLabel(labelKey, locale)` — resolves key at render time
- `formatHourApprovalsValue(value, format, locale)` — `Intl` formatters

Production target: `@angular/localize` or shared `@floorganise/i18n` package; packs ship their own JSON fragments merged at build time.

---

## Performance (pack columns, computed, extensions)

**Short answer:** pack-specific columns should not cause a measurable drop when implemented correctly. The POC initially called `GetRowExtensions` per row (N+1); that is fixed — **one batch call per list response**.

### Cost by column type

| Type | Where values are computed | Typical cost |
|------|---------------------------|--------------|
| **Core** | SQL projection / spec (same as without packs) | Baseline list query |
| **Computed** | List mapper or SQL projection from data already loaded | O(1) per row in memory — **no extra DB round-trip** |
| **Extension (in-memory)** | Pack batch map over ids (POC: derived from task id) | O(n) in memory |
| **Extension (stored)** | Pack batch load — **one query** with `WHERE id IN (...)` | Single extra query per list page |

### Rules (non-negotiable)

1. **`GetView` is in-memory** — pack returns a static/cached schema; no I/O per request unless you explicitly cache a remote manifest (discouraged on hot paths).
2. **No per-row pack I/O** — `GetRowExtensions(taskIds)` receives **all** visible ids; never call inside `rows.Select(...)`.
3. **Computed columns belong in the query layer** — calculate in the Ardalis spec projection, EF `Select`, or list mapper from fields already fetched. The pack only toggles visibility via `ColumnSource.Computed`.
4. **Heavy extension data → SQL join** — when extensions live in DB, the pack's Infrastructure adapter exposes a **batch port** implemented with a compiled spec / `EF.CompileQuery` / single `IN` query — not N lazy loads.
5. **Paginate** — extension batch size equals page size (e.g. 50–200 rows), not whole tenant.

### When compiled LINQ / specs help

Use when extension columns require **database-backed** values:

```csharp
// Pack Infrastructure — illustrative
public sealed class AcmeSapCostElementBatchLoader
{
    // EF.CompileQuery or Ardalis spec evaluated once per list request
    public Task<IReadOnlyDictionary<Guid, string>> LoadAsync(
        IReadOnlyList<Guid> taskIds, CancellationToken ct);
}
```

The list handler:

```text
1. Run core list spec        → rows (paginated)
2. Collect task ids          → one batch
3. pack.GetRowExtensions(ids) → one DB query or in-memory map
4. Map computed fields       → from row data already in memory
5. Merge into response DTO
```

Compiled queries help when the same batch shape runs on every list request — they reduce expression-tree compile overhead; the bigger win is **one query instead of N**.

### What to avoid

| Anti-pattern | Effect |
|--------------|--------|
| `GetRowExtensions` per row in a loop | N+1 queries / calls |
| Pack computes from Domain services per row | Unbounded latency |
| Computed column triggers extra HTTP per row | Unacceptable on Gantt/lists |
| Extension join without pagination bounds | Memory blow-up on 50k rows |

### POC status

| Item | Status |
|------|--------|
| Batch `GetRowExtensions(taskIds)` | Implemented |
| `computed` bag from list mapper | Implemented (`daysSinceLastSubmission`) |
| DB-backed extension batch loader | Documented — add when a real pack needs SQL |

---

## Frontend conventions

1. **Load view schema once** — `GET /capabilities` (or screen-specific endpoint) before rendering.
2. **Schema-driven columns** — iterate `queueView.columns`; resolve `labelKey` via i18n bundle; bind core / `extensions` / `computed` paths.
3. **Format with locale** — `Intl.DateTimeFormat` / `Intl.NumberFormat` from invariant API values.
4. **No tenant branches in Angular** — `if (tenant === 'acme')` is forbidden; use pack-delivered schema.
5. **Presentational widgets** in `libs/<context>/ui/`; schema iteration stays in `feature-*`.

TypeScript mirrors `Platform.Shared.View`:

```typescript
export interface ViewDefinitionDto {
  screenId: string;
  columns: ColumnDefDto[];
}

export interface ColumnDefDto {
  id: string;
  labelKey: string;
  source: ColumnSourceDto;
  visible: boolean;
  order: number;
  format?: string | null;
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
- [ ] Computed values in list mapper / spec — not in pack per-row loops
- [ ] `GetRowExtensions` is batched — one call per list response
- [ ] No tenant branches in Domain/Application
- [ ] Characterization test: default pack vs client pack column sets

**Frontend**

- [ ] `data-access` DTOs include `view`, `extensions`, `computed`
- [ ] `labelKey` resolved via locale bundles (en/nl/es/ja/vi minimum)
- [ ] Invariant API values formatted with `Intl` — no locale strings on wire
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
| English `label` strings in pack C# | `labelKey` + locale JSON |
| Per-row `GetRowExtensions` in list map | Batch `GetRowExtensions(taskIds)` |
| Computed values in pack per row | Query handler / list mapper `computed` bag |
| Locale-specific dates in API JSON | ISO dates + `Intl` in UI |

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.1 | 2026-06-27 | `labelKey` + i18n; batch extensions; computed bag; performance section |
| 1.0 | 2026-06-27 | Initial standard; `Platform.Shared.View`; Hour Approvals Acme pack POC |
