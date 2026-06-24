# Floor2Plan.Core — UI to Bounded Context Map (Phase 2.1)

> **DRAFT — REQUIRES HUMAN VALIDATION**
> Maps production home tiles and navigation pages to the bounded contexts defined in `02-bounded-context-map.md`.

**Generated:** 2026-06-24  
**Inputs:** `02-bounded-context-map.md`, RC smoke run against `2025-14-patch.floor2plan.com` (`testrd` service login)  
**Related:** `Floor2PlanSmokeTests/scripts/ui-context-map.js` (route → context rules used by the smoke harness)

---

## Bounded contexts (reference)

| # | Name | Label |
|---|------|-------|
| 1 | Planning & Resource Management | **PBS** |
| 2 | Workforce & Organization | **Workforce** |
| 3 | Timekeeping & Work Hours | **Timesheets** |
| 4 | Devices & Shop Floor Clocking | **Devices** |
| 5 | Workflow & Ticketing | **Tickets** |
| 6 | Synchronization & Import Pipeline | **Sync** |
| 7 | System Administration & Infrastructure | **System** |

**Reporting** is documented separately (read-only `ReportingDbContext`) and is not one of the seven data-model contexts.

---

## Home tiles (RC acceptance — `testrd`)

Nine `.f2ps-tile` entries are visible on `/` after service login:

| Tile | Abbrev | Route | Primary context | Also touches | MVC area |
|------|--------|-------|-----------------|--------------|----------|
| Projects | Pr | `/Pbs/Planning/#/Project/2` | **PBS** | Workforce | `Pbs` |
| Activities | Ac | `/Do/Reporter#/task-list` | **PBS** | Timesheets | `Do` |
| Hours and Progress | Hp | `/Do/EmployeeTimesheet#/tasks/allocated` | **Timesheets** | PBS | `Do` |
| Floorboard | Fb | `/Do/Floorboard#/` | **PBS** | Devices, Timesheets | `Do` |
| Person management | Pm | `/GeneralSettings#/organisationPersons` | **Workforce** | System | Root |
| HR management | Hr | `/Hr/ClockingTerminal#/PunchManagement` | **Devices** | Workforce | `HR` |
| Timesheets | Ts | `/Do/Weeklytimesheet#/Historical/` | **Timesheets** | Workforce | `Do` |
| Timesheets administration | Ta | `/Check/ErpWeeklytimesheet#/Historical/` | **Timesheets** | Reporting | `Check` |
| Tickets | Ti | `/Ticket/Index#/` | **Tickets** | PBS, Workforce | `Ticket` |

---

## Module dropdown pages (menu-only additions)

The upper-left module menu exposes additional pages for the same user. Items already listed as home tiles are omitted here.

| Menu item | Route | Primary context | Also touches |
|-----------|-------|-----------------|--------------|
| Budgets | `/Do/Reporter#/budget` | **PBS** | — |
| Planboard | `/Do/Planboard` | **PBS** | Workforce |
| Floorspace Planning | `/floorspace` | **PBS** | Workforce (Location) |
| Reports | `/Check/Reports` | **Reporting** | All (read projections) |
| PBS Settings | `/PBS/Settings#/` | **System** | PBS |
| Organisation selector | `/Select/Organisation?returnUrl=%2F` | **Workforce** | System |

Shell entries (`Home`, `Floor2Plan` logo link) map to **Shell** — the cross-context launcher, not a domain context.

---

## Auth and shell pages

| Page | Route | Context |
|------|-------|---------|
| Login | `/Account/Login` | **Identity** / System |
| Home dashboard | `/` | **Shell** (launcher) |

---

## Contexts not in navigation (for `testrd`)

These MVC areas exist in the monolith (see `01-entry-points.md`) but do not appear in this user's home tiles or module menu — typically permission- or license-gated:

| Context | Example routes | MVC area |
|---------|----------------|----------|
| **Sync** | `/Sync/`, `/Sync/Import*` | `Sync` |
| **System** (admin) | `/System/*` | `System` |
| **Devices** (standalone) | `/Devices/*` | `Devices` |
| **Workforce** (balances) | `/HR/PersonBalance`, `/HR/BalancePolicy` | `HR` |
| **PBS** (master-data CRUD) | `/Pbs/Project`, `/Pbs/Activity`, `/Pbs/Person` | `Pbs` |
| **Reporting/KPI** | `/Check/KPI`, `/Check/ReportDesigner` | `Check` |

---

## V2 shell tiles (`F2pPlatform` POC)

The Angular V2 shell (`F2pPlatform/web/apps/f2p-shell`) uses placeholder tiles that do not yet mirror production:

| V2 tile | Route | Intended context | Status |
|---------|-------|------------------|--------|
| Reference | `/reference` | Cross-cutting POC | Implemented |
| Planning | `/modules/planning` | **PBS** | Stub |
| Production | `/modules/production` | **PBS** + **Devices** (execution) | Stub |
| Quality | `/modules/quality` | Not in 7-context map | Stub / future |

Target V2 decomposition (from `floor2plan-v2-read-model-playbook.md`): **Import**, **WBS**, **Planning**, **Hours**, **Resources**, **Reporting**, **Identity**, **Billing**.

---

## Cross-context UI surfaces

| Surface | Why it spans contexts |
|---------|----------------------|
| **Floorboard** | Reads planning state, accepts shop-floor clocking, feeds timesheet validation |
| **Hours and Progress** | Timesheet manager view with activity assignment FKs into PBS |
| **Activities (Reporter)** | Activity reporting in `Do` area; may cross-reference booked hours |
| **Person management** | Workforce data hosted in System settings shell |
| **HR management** | Named HR but lands on Devices (clocking terminals) |

The `Do` MVC area is a **cross-context execution shell** — Planboard, Floorboard, Reporter, and timesheet pages live under `Areas/Do/` but belong to different bounded contexts.

---

## Relationship diagram

See `03-ui-to-context-map.mermaid`.

---

## Smoke harness artifact

Each Cypress smoke run writes a structured, role-scoped map to:

```text
Floor2PlanSmokeTests/artifacts/ui-context-map/<host>.json
```

Example shape: `Floor2PlanSmokeTests/scripts/ui-context-map.example.json`

Fields:

| Field | Description |
|-------|-------------|
| `schemaVersion` | Artifact format version |
| `generatedAt` | ISO timestamp |
| `host` | Target hostname |
| `pages[]` | Discovered tiles and menu pages with `primaryContext`, `secondaryContexts`, `route`, `sources` |
| `contextsNotInNavigation` | Static catalog of contexts often absent from home/menu |
| `summary` | Counts of home tiles vs menu-only pages |

Route → context rules live in `Floor2PlanSmokeTests/scripts/ui-context-map.js` and should stay aligned with this document and `02-bounded-context-map.md`.

---

## Validation notes

| Item | Status |
|------|--------|
| Tile visibility is role-scoped | Confirmed — `testrd` sees 9 tiles; admin-only areas (Sync, System) absent |
| "HR management" → Devices | Confirmed — `/Hr/ClockingTerminal` |
| "Person management" → Workforce via System shell | Confirmed — `/GeneralSettings` |
| User/Role ownership (Context 2 vs 7) | [NEEDS REVIEW] per `02-bounded-context-map.md` |
| V2 Production / Quality tile targets | [NEEDS REVIEW] — placeholders only |

---

*Phase 2.1 draft. Update when navigation changes or additional RC environments are mapped.*
