# Platform 2.0 architecture overview

**Purpose:** Single entry point for **stakeholder presentations**, onboarding, and engineering orientation. Explains how the V2 platform is structured, how **modules** and **packs** relate, and where to find deep standards.

**Audience:** Tech leads, product, engineers, AI agents.

**Runnable reference:** `F2pPlatform/` in SandBox.

**Deep standards (detail):** linked in [Further reading](#further-reading).

---

## Presentation outline (slide deck skeleton)

Copy section headings into slides; diagrams below are presentation-ready.

| # | Slide title | Key message |
|---|-------------|-------------|
| 1 | **Why V2** | One product core; client variance via packs — not git submodules or service inheritance |
| 2 | **Platform layers** | Control plane → host → modules + packs → SPA |
| 3 | **What is a module?** | Tenant-agnostic bounded context (API, domain, default UI) |
| 4 | **What is a pack?** | Versioned tenant/vendor plugin (columns, rules, connectors) |
| 5 | **Modules vs packs** | Modules are required; packs are optional; packs never replace modules |
| 6 | **Hour Approvals example** | One module, one shared UI, Acme pack adds SAP column |
| 7 | **Legacy vs native** | Control plane routes; strangler until cutover |
| 8 | **Orchestration** | Akka actor pipelines compose pack stages at the host |
| 9 | **Tenant SPA — one application** | `f2p-shell` is the only tenant product SPA; bounded contexts are lazy-loaded libs |
| 10 | **How modules plug into the shell** | `loadChildren` routes, `@f2p/*` path aliases, feature guards, home tiles |
| 11 | **Shared plumbing** | One auth session, HTTP interceptors, `@floorganise/css` + `@floorganise/ui` chrome |
| 12 | **Same-origin deployment** | Dev proxy + nginx `try_files` — `/api` and `/hubs` on one origin with the SPA |
| 13 | **Roadmap** | Foundation → pilots → scale (`foundation-and-pilot-plan.md`) |

**Speaker notes (slide 1):** Legacy Floor2Plan often ships client variance as optional `Text*` columns, `AcmeService : BaseService`, and connector submodules in a customized mega-repo. V2 keeps **one canonical module per capability** and plugs in **versioned packs** per tenant.

**Speaker notes (slide 9):** Legacy Floor2Plan often feels like separate apps per module (different Razor areas, full page reloads, inconsistent chrome). V2 ships **one Angular SPA** (`f2p-shell`) with lazy-loaded context libs. Users navigate via the router — not separate deployments. The operator admin console (`f2p-admin-shell`) is intentionally separate.

**Printable SPA supplement:** [`platform-v2-tenant-spa.html`](platform-v2-tenant-spa.html) · diagram: [`diagrams/platform-v2-tenant-spa.svg`](diagrams/platform-v2-tenant-spa.svg)

---

## Platform at a glance

```mermaid
flowchart TB
  subgraph CP["Control plane (always on)"]
    TEN["Tenant registry"]
    ENT["Pack entitlements"]
    ROUTE["Routing / SSO binding"]
  end

  subgraph RUNTIME["Tenant runtime (per deployment profile)"]
    HOST["Composition host\nProgram.cs"]
    MOD["Modules\n(tenant-agnostic)"]
    PACK["Packs\n(tenant / vendor)"]
    SPA["Tenant SPA\nf2p-shell + libs"]
  end

  subgraph LEGACY["Legacy-hosted (during migration)"]
    LEG["Legacy app + DB"]
    STR["Strangler adapters"]
  end

  CP --> ROUTE
  ROUTE --> HOST
  ROUTE --> LEG
  HOST --> MOD
  HOST --> PACK
  PACK -.->|implements ports| MOD
  MOD --> SPA
  STR -.-> LEG
  HOST --> STR
```

| Layer | Responsibility | Examples |
|-------|----------------|----------|
| **Control plane** | Who the tenant is, which packs they own, legacy vs native routing | `ControlPlane` module, Admin backoffice |
| **Host** | Composition root — wires modules and entitled packs | `F2pPlatform.Host` |
| **Module** | Product capability — domain, API, persistence, default behavior | `HourApprovals`, `Import`, `Planning` |
| **Pack** | Variance — UI layout, tenant rules, vendor protocols | `acme-hour-approvals-v1`, `sap-projects-v1` |
| **SPA** | Schema-driven UI paired 1:1 with modules | `web/libs/hour-approvals/` |
| **Actors** | Long-running workflow orchestration; pack stages attach here | Import pipeline, integration routers |

---

## Module vs pack

### One-line definitions

| Term | Definition |
|------|------------|
| **Module** | A **bounded context** delivered as Domain → Application → Infrastructure → Api (+ paired frontend libs). **Same for every tenant.** |
| **Pack** | A **small, versioned plugin** that implements a port defined by a module (or integration abstractions). **Different per tenant or vendor.** |

### What each owns

| Owns | Module | Pack |
|------|--------|------|
| Ubiquitous language & invariants | ✓ | ✗ |
| HTTP API & read DTOs | ✓ | ✗ |
| Persistence (`DbContext`) | ✓ | ✗ *(optional pack-local store for extensions only)* |
| Default screen behavior | ✓ (default pack in Infrastructure) | ✗ |
| Tenant column layout / `extensions` | port only | ✓ |
| Tenant workflow gates | port / actor hook | ✓ (rules pack) |
| Vendor fetch & map | port only | ✓ (integration pack) |
| i18n for core columns | ✓ (module bundles) | pack keys only (`packs.<pack-id>.*`) |
| Angular feature components | ✓ (shared per context) | ✗ |

### Dependency direction

```text
Pack  ──implements──►  Module Application port
Module  ──never references──►  Pack implementation
Host  ──registers both──►  AddHourApprovalsModule() + AddAcmeHourApprovalsPack()
```

**Rule:** Compile-time arrows point **from pack to module abstractions**, never into module Domain.

### Module application boundaries (Ports vs Persistence)

Not every interface in Application is a **pack** port. Split abstractions by role:

```text
<Context>.Application/
  Ports/                    ← inbound API + tenant extension points
  Persistence/              ← outbound storage (Infrastructure implements)
```

| Interface | Folder | Implemented by | Per-tenant? |
|-----------|--------|----------------|-------------|
| `IHourApprovalsService` | `Ports/` | Application | No |
| `IHourApprovalsCustomizationPack` | `Ports/` | Pack or default | Yes |
| `IHourApprovalsRepository` | `Persistence/` | `EfHourApprovalsRepository` | No |

**Who calls the repository?** Only `HourApprovalsService` — not HTTP endpoints or packs.

```text
HTTP  →  IHourApprovalsService  →  IHourApprovalsRepository  →  EF
              ↑
              └── IHourApprovalsCustomizationPack (view schema only)
```

Reference: `F2pPlatform/src/Modules/HourApprovals/`.

### Cardinality

```text
1 bounded context  →  1 module  (required for that capability)
1 module           →  0..n packs  (optional)
1 tenant × context →  typically 1 active customization pack (or default)
```

### Can you have packs without modules?

**No.** Packs are plugins; modules are the application surface.

| Scenario | Valid? |
|----------|--------|
| Host with modules, no packs | ✓ — default packs suffice (`Reference`, `Identity`) |
| Host with packs, no modules | ✗ — nothing to plug into |
| Ship only an integration pack NuGet | ✓ as a **deliverable**, but runtime still needs `Import` / `WBS` modules to consume canonical data |
| Legacy tenant with pack ids in control plane only | ✓ metadata until native cutover — code still in legacy build profile |

### Legacy → V2 mapping

| Legacy pattern | V2 replacement |
|----------------|----------------|
| `Text3` / `Bool2` on core tables | Module read DTO + pack `extensions` / rules |
| `AcmePlanningService : PlanningService` | `Planning` module + `acme-planning-rules-v1` pack |
| Connector git submodule | `sap-projects-v1` integration pack |
| Per-client Razor/Vue fork | Shared Angular feature + pack view schema |
| `if (tenant == "acme")` in C# | Forbidden in modules; pack port or actor stage |

---

## End-to-end example: Hour Approvals

```mermaid
sequenceDiagram
  participant UI as hour-approvals SPA
  participant API as HourApprovals module
  participant Pack as Acme customization pack
  participant CP as Control plane

  CP-->>API: tenant entitled acme-hour-approvals-v1
  UI->>API: GET /api/hour-approvals/capabilities
  API->>Pack: GetView(queue)
  Pack-->>API: ViewDefinition + packId
  API-->>UI: columns, labelKeys, permissions
  UI->>API: GET /api/hour-approvals/queue
  API->>Pack: GetRowExtensions(taskIds) batch
  Pack-->>API: extensions per row
  API-->>UI: items + extensions + computed
  Note over UI: Renders schema — no if(tenant===acme)
```

| Artifact | Location |
|----------|----------|
| Module | `F2pPlatform/src/Modules/HourApprovals/` |
| Acme pack | `F2pPlatform/src/Packs/HourApprovals.Packs.Acme/` |
| Frontend | `F2pPlatform/web/libs/hour-approvals/` |
| Pack manifest | `HourApprovals.Packs.Acme/PACK.md` |

---

## Module rules (engineering)

1. **Tenant-agnostic** — no tenant slug branches in Domain/Application.
2. **Define extension ports** — e.g. `IHourApprovalsCustomizationPack`; ship `DefaultHourApprovalsPack` in Infrastructure.
3. **Explicit DI** — `Add<Context>Module()` / `Map<Context>Endpoints()`; no ABP in new code.
4. **Paired frontend** — `web/libs/<context>/` per `MODULE.md`.
5. **Characterization tests** — P0 behavior locked before strangler moves.

Detail: `module-composition-di.md`.

---

## Pack rules (engineering)

1. **One primary role per pack** — UI customization, rules, or integration (do not mix SAP SDK with view schema).
2. **Versioned id** — `acme-hour-approvals-v1`; bump suffix on breaking changes.
3. **Entitled per tenant** — `packEntitlements.customizationPacks` / `integrationPacks` in control plane.
4. **Host registration only** — `AddAcmeHourApprovalsPack()` in `Program.cs`.
5. **No domain invariants** — promote to module when all tenants need the rule.

Detail: `platform-pack-blueprint.md`.

---

## Pack types (quick reference)

| Type | packId example | Solves |
|------|----------------|--------|
| **Customization (UI)** | `acme-hour-approvals-v1` | Columns, visibility, extension fields, labels |
| **Customization (rules)** | `acme-planning-rules-v1` | Approval gates, validation stages |
| **Integration** | `sap-projects-v1` | Vendor protocol → canonical exchange format |
| **Strangler adapter** *(not a pack)* | — | Delegate to legacy API/DB during migration |

---

## Deployment profiles

```text
                    Control plane
                          │
           ┌──────────────┴──────────────┐
           ▼                             ▼
   legacy_hosted                    native
   · full legacy app                · v2 host + modules
   · submodule build profile        · entitled packs loaded
   · pack ids = metadata            · canonical DB per tenant
           │                             │
           └──────── cutover ────────────┘
```

| Mode | Modules | Packs |
|------|---------|-------|
| **legacy_hosted** | Legacy code owns behavior | Pack ids documented; enforced after cutover |
| **native** | V2 modules in host | Packs registered and enforced |

Detail: `ApiImportActorPoc/docs/deployment-profile-sketch.md` (pattern applies platform-wide).

---

## Actor orchestration (workflows)

For imports, integrations, and multi-step tenant workflows:

```text
HTTP / queue / scheduler
  → Facade (AskCorrelated)
    → IntegrationRouterActor
        → [VendorFetchActor]      ← integration pack
        → [MapToCanonicalActor]   ← integration pack
        → [TenantRulesActor]      ← customization rules pack
        → OrchestratorActor
            → PersistActor        ← sole DbContext boundary
```

**Principle:** Modules own domain rules; actors **compose** pack stages at the host. Detail: `platform-actor-standard.md`.

---

## Tenant SPA — single application

The tenant product frontend is **one Angular SPA**, not a micro-frontend federation of separate apps. Bounded contexts ship as **Nx libs** (or path-alias libs in SandBox) that plug into a shared shell via lazy routes.

**Runnable reference:** `F2pPlatform/web/` · **Standards:** `platform-frontend-standard.md`, `floor2plan-v2-read-model-playbook.md` §3.

### Presentation diagram

![Tenant SPA — single application](diagrams/platform-v2-tenant-spa.svg)

### At a glance

```mermaid
flowchart TB
  subgraph SPA["Single SPA — f2p-shell"]
    BOOT["main.ts → AppComponent\n&lt;router-outlet /&gt;"]
    ROUTER["app.routes.ts"]
    AUTH["authGuard + IdentityAuthService"]
    HTTP["correlation + auth interceptors"]
    CSS["@floorganise/css + @floorganise/ui"]
    BOOT --> ROUTER
    ROUTER --> AUTH
    ROUTER --> LAZY["lazy context routes"]
    HTTP --> API["/api/* same origin"]
    CSS --> CHROME["shared navbar, tiles, tokens"]
  end

  LAZY --> HA["hour-approvals"]
  LAZY --> REF["reference"]
  LAZY --> PE["platform-events"]
  LAZY --> MORE["planning, hours, …"]

  API --> HOST["F2pPlatform.Host\ncomposed modules"]
  HOST --> HUB["/hubs/platform-events"]
```

### What makes it feel like one app

| Mechanism | What it unifies | Key location (SandBox) |
|-----------|-----------------|------------------------|
| **Shell host** | One bootstrap, one router outlet | `web/apps/f2p-shell/` |
| **Lazy routes** | In-app navigation between contexts | `apps/f2p-shell/src/app/app.routes.ts` |
| **Monorepo libs** | One build artifact, many contexts | `web/tsconfig.json` paths, `libs/<context>/` |
| **Root auth** | One login/session for all modules | `libs/identity/data-access/` |
| **HTTP interceptors** | Bearer token + correlation on every `/api/` call | `app.config.ts`, `libs/shared/api-core/` |
| **Design system** | Shared chrome and tokens | `@floorganise/css`, `@floorganise/ui` |
| **Same-origin API** | Browser sees one backend | `proxy.conf.json`, `nginx.docker.conf` |
| **SPA fallback** | Deep links work without per-route apps | nginx `try_files $uri /index.html` |
| **Composed API host** | All module endpoints on one host | `host/F2pPlatform.Host/Program.cs` |

### Shell composition root

The shell is intentionally thin — bootstrap, router, global styles, and cross-cutting providers only.

```typescript
// apps/f2p-shell/src/app/app.component.ts
@Component({
  selector: 'f2p-root',
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
export class AppComponent {}
```

```typescript
// apps/f2p-shell/src/app/app.config.ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([correlationInterceptor, identityAuthInterceptor])),
    { provide: F2P_API_BASE_URL, useValue: '' }, // same origin
  ],
};
```

Global styles import the design system once:

```css
/* apps/f2p-shell/src/styles.css */
@import '@floorganise/css';
```

### Lazy routes — modules plug into one router

Each bounded context exports a `Routes` array from a `feature-*` lib. The shell lazy-loads them — no full page reload, no separate SPA per module.

```typescript
// apps/f2p-shell/src/app/app.routes.ts (excerpt)
{
  path: 'hour-approvals',
  canActivate: [authGuard],
  loadChildren: () =>
    import('@f2p/hour-approvals/feature-tasks').then(m => m.hourApprovalsRoutes),
},
```

Context feature libs own their child routes and optional capability guards:

```typescript
// libs/hour-approvals/feature-tasks/src/lib/hour-approvals.routes.ts
export const hourApprovalsRoutes: Routes = [
  {
    path: '',
    canActivate: [hourApprovalsFeatureGuard],
    loadComponent: () =>
      import('./hour-approvals-page.component').then(m => m.HourApprovalsPageComponent),
  },
];
```

**Add a new context:** `./scripts/scaffold-frontend-module.sh <Context>` → register lazy route → add home tile. See `F2pPlatform/web/README.md`.

### Home tiles — module launcher inside the SPA

The home page is the in-app module grid. Tiles navigate via Angular `Router`, not external URLs.

```typescript
// apps/f2p-shell/src/app/pages/home-page.component.ts (pattern)
onTileSelect(tile: HomeTile): void {
  void this.router.navigateByUrl(tile.route);
}
```

Shared chrome (`F2pAppNavbarComponent`, `F2pHomeTilesComponent`) comes from `@floorganise/ui` — feature pages reuse the same navbar so every module looks like part of one product.

### Shared auth and HTTP — one session, all modules

| Concern | Pattern | Location |
|---------|---------|----------|
| Login session | `IdentityAuthService` (`providedIn: 'root'`) | `libs/identity/data-access/` |
| Route protection | `authGuard` / `guestGuard` on shell routes | `auth.guard.ts` |
| API identity | `identityAuthInterceptor` adds `Authorization` to `/api/` | `identity-auth.interceptor.ts` |
| Tracing | `correlationInterceptor` adds `X-Correlation-Id`, `X-Use-Case` | `libs/shared/api-core/` |
| Feature gating | Context guards (e.g. `hourApprovalsFeatureGuard`) | `libs/<context>/data-access/` |

### Monorepo layout — one compilation unit

SandBox uses TypeScript path aliases; the monolith uses Nx with the same layering:

```text
web/
├── apps/f2p-shell/              # composition root (bootstrap, routes, global styles)
├── apps/f2p-admin-shell/        # separate operator console — NOT the tenant product
└── libs/
    ├── shared/
    │   ├── api-core/            # correlation interceptor, API base URL token
    │   ├── platform-events/     # root SignalR hub client
    │   └── ui/                  # re-export @floorganise/ui
    ├── identity/data-access/    # auth service, guards, interceptor
    └── <context>/
        ├── data-access/         # API clients, DTOs, feature guards
        └── feature-*/           # routes, smart pages
```

`tsconfig.app.json` includes **all libs** in one build:

```json
"include": ["apps/f2p-shell/src/**/*.d.ts", "libs/**/*.ts"]
```

Nx boundary tags (`scope:planning`, `type:ui`, etc.) prevent cross-context imports while keeping one deployable SPA. Detail: `floor2plan-v2-read-model-playbook.md` §3.

### Same-origin API and realtime

Empty `F2P_API_BASE_URL` means relative `/api/*` and `/hubs/*` — the browser treats backend and frontend as one origin.

**Development** — Angular dev server proxies to the composed host:

```json
// proxy.conf.json
{ "/api": { "target": "http://localhost:5080" },
  "/hubs": { "target": "http://localhost:5080", "ws": true } }
```

**Production** — nginx serves the SPA and proxies API/SignalR to the host:

```nginx
location /api/  { proxy_pass http://f2p-platform-api:5080/api/; }
location /hubs/ { proxy_pass http://f2p-platform-api:5080/hubs/; ... }
location /      { try_files $uri $uri/ /index.html; }
```

`try_files … /index.html` is what makes deep links like `/hour-approvals` work without a separate app per route.

Cross-cutting realtime uses a root-scoped `PlatformEventsService` (`providedIn: 'root'`) connecting to `/hubs/platform-events` on the same host.

### Backend mirror — one composed API

The frontend's single-origin model matches the backend composition root:

```csharp
// host/F2pPlatform.Host/Program.cs (pattern)
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddHourApprovalsModule(builder.Configuration);
// …
app.MapIdentityModule();
app.MapHourApprovalsModule();
app.MapHub<PlatformEventsHub>("/hubs/platform-events");
```

One host, many modules — the SPA talks to one API surface.

### What is NOT the tenant product SPA

| Artefact | Role |
|----------|------|
| `f2p-shell` | **Tenant product SPA** — planning, hours, hour approvals, etc. |
| `f2p-admin-shell` | **Platform operator console** — tenant provisioning, control plane |
| Legacy Razor/Vue | Untouched during strangler; replaced screen-by-screen into `f2p-shell` libs |

### Checklist — adding a context to the single app

1. Scaffold: `./scripts/scaffold-frontend-module.sh <Context>`
2. Backend module: `./scripts/scaffold-module.sh <Context>` + register in `Program.cs`
3. Lazy route in `apps/f2p-shell/src/app/app.routes.ts`
4. Home tile in `home-page.component.ts`
5. Feature page uses `@floorganise/ui` shell chrome — no local copies of navbar/tiles
6. `data-access` lib for API; `feature-*` for routes — no cross-context `data-access` imports

---

## Physical layout (SandBox)

```text
F2pPlatform/
  host/F2pPlatform.Host/           composition root
  src/
    Modules/<Context>/               bounded contexts
    Packs/<Context>.Packs.<Client>/  customization packs
    Shared/                          Platform.Shared.View, etc.
  web/
    apps/f2p-shell/                  tenant SPA host
    libs/<context>/                  paired frontend modules
```

Scaffold:

```bash
./scripts/scaffold-module.sh Planning
./scripts/scaffold-frontend-module.sh Planning
./scripts/scaffold-customization-pack.sh Planning Acme acme-planning-v1
```

---

## Glossary

| Term | Meaning |
|------|---------|
| **Bounded context** | Business area (Planning, Hours, Import) — usually one module |
| **Module** | Code delivery of a context: layers + API + UI lib |
| **Pack** | Versioned tenant or vendor variance plugin |
| **Port** | Application interface a pack implements |
| **Default pack** | Baseline implementation inside module Infrastructure |
| **Entitlement** | Control-plane list of enabled pack ids for a tenant |
| **Composition root** | Host `Program.cs` — only place that wires modules + packs |
| **f2p-shell** | Tenant product SPA host — bootstrap, router, auth, lazy routes, global styles |
| **Lazy route** | `loadChildren` / `loadComponent` — context lib loaded on demand inside one SPA |
| **Persistence port** | `I<Context>Repository` in `Application/Persistence/` — EF adapter in Infrastructure |
| **Promotion** | Move field from pack `extensions` into module when universal |
| **Strangler** | Adapter that delegates to legacy until native parity |

---

## FAQ

**Does every module need a pack?**  
No. Many modules run on the default pack only.

**Does every tenant need the same packs?**  
No. Entitlements differ per tenant.

**Where does client-specific English/Dutch labels for SAP columns go?**  
Pack i18n keys (`packs.<pack-id>.columns.*`), not in C#.

**Can a pack open EF Core on the main domain DbContext?**  
Integration packs: no. UI packs: only via a dedicated batch loader in pack Infrastructure — never in Application Domain.

**Is Control Plane a module?**  
Yes — platform capability, not a tenant product feature. It typically has no customization packs.

---

## Further reading

| Topic | Document |
|-------|----------|
| Modularization roadmap & pilots | `foundation-and-pilot-plan.md` |
| Module DI (no ABP) | `module-composition-di.md` |
| Pack artifact catalog & scaffold | `platform-pack-blueprint.md` |
| UI view schemas & extensions | `platform-ui-customization-standard.md` |
| Actor pipelines | `platform-actor-standard.md` |
| Integration pack dependencies | `../floor2plan-v2-connector-architecture.md` |
| Legacy Text*/Bool* migration | `tenant-workflow-fields-deepdive-instructions.md` |
| Frontend (@floorganise/css) | `platform-frontend-standard.md` |
| Tenant SPA (single app) | This document § [Tenant SPA](#tenant-spa--single-application) · [`platform-v2-tenant-spa.html`](platform-v2-tenant-spa.html) |
| Auth & SSO | `platform-authentication-standard.md` |
| Legacy connector anti-pattern | `../floor2plan-legacy-connector-submodule-antipattern.md` |
| Runnable POC index | `F2pPlatform/README.md` |

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.1 | 2026-06-28 | Tenant SPA — single application section; slides 9–12; diagram + printable HTML |
| 1.0 | 2026-06-27 | Initial overview; module vs pack; presentation outline |
