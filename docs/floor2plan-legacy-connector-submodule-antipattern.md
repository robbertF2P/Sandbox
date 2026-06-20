# Why legacy Floor2Plan connectors should not reference core via git submodules

**Audience:** Developers, integrators, and tech leads working on SAP, Kronos, PLM, HR, file, and client-specific **connectors** in the legacy Floor2Plan application.

**Purpose:** Explain — in plain terms — why the current pattern (connector folder as git submodule that **references the core application**) creates mixed dependencies, and what Platform 2.0 replaces it with.

**This is not** a blame document. Submodule-based connectors solved real problems years ago. The product outgrew that model.

---

## 1. Vocabulary

| Term (legacy) | Meaning |
|---------------|---------|
| **Connector** | Code that talks to an external system (SAP, Kronos, PLM, eShare, HR, files) — own **git repository** |
| **Core** | Main Floor2Plan application — domain, services, EF, UI — also its **own git repository** |
| **Customized repository** | A **composite clone** used to compile a specific delivery: `core/` submodule + connector folders + **client-derived core services**, built as one solution |
| **Derived core services** | Subclasses of core application/domain services (`ClientXImportService : ImportService`) with **override methods** for client data handling and sync behaviour |
| **Sync processes** | Scheduled or triggered jobs that move data between F2P and external systems — often split across connector, derived service, and Hangfire/handler code |
| **Connector repo (standalone)** | Often **cannot compile** in isolation — it needs core types and sometimes client service overrides at build time |

In Platform 2.0, connectors become **versioned integration packs** referenced only through contracts — not folders in a customized mega-repo.

---

## 2. The legacy pattern (how it actually works)

Each connector lives in **its own repo**, but connectors typically **do not compile alone**. Teams assemble a **customized repository** for a client or integration profile:

```text
customized-repo/                    ← clone this to build (client / PS delivery)
├── core/                           ← git submodule → Floor2Plan main application
├── connectors/
│   ├── plm-planning/               ← git submodule → PLM connector repo
│   ├── sap-wbs/
│   ├── eshare/
│   └── …                           ← one folder per enabled connector
├── client/                         ← client-specific layer (names vary)
│   ├── services/                   ← derived core services (override methods)
│   │   ├── AcmeImportService.cs    ← : ImportService
│   │   ├── AcmeSyncService.cs      ← sync process hooks
│   │   └── AcmeWbsService.cs
│   ├── sync/                       ← sync job definitions / schedulers
│   └── …                           ← may itself be a submodule
└── Floor2Plan.Customized.sln       ← compiles core + connectors + client overrides
```

```mermaid
flowchart TB
  subgraph CustomizedRepo["Customized repository (what you clone & build)"]
    CoreFolder["core/ submodule"]
    ClientLayer["client/ derived services + sync"]
    ConnA["connectors/plm-planning/"]
    ConnB["connectors/eshare/"]
    SLN[".sln — compiles all layers"]
  end

  subgraph RemoteRepos["Separate git repos"]
    CoreRepo[(Core repo)]
    ClientRepo[(Client customizations repo)]
    PlmRepo[(PLM connector repo)]
    EshareRepo[(eShare connector repo)]
  end

  CoreRepo -.->|submodule| CoreFolder
  ClientRepo -.->|submodule or folder| ClientLayer
  PlmRepo -.->|submodule| ConnA
  EshareRepo -.->|submodule| ConnB

  ClientLayer -->|inherits / overrides| CoreFolder
  ConnA -->|project reference| CoreFolder
  ConnA -->|calls overridden services| ClientLayer
  ConnB -->|project reference| CoreFolder
  SLN --> CoreFolder
  SLN --> ClientLayer
  SLN --> ConnA
  SLN --> ConnB
```

**What happens at build time:**

1. Clone the **customized repo** (not “the product” — a **composition** of repos).
2. `git submodule update --init --recursive` — pin `core`, **client layer**, and each connector to specific commits.
3. Open the customized solution — connectors and **derived services** **project-reference** into `core/`.
4. DI registration (in customized repo) binds **client subclasses** instead of base core services where required.
5. Compile one binary/deployment artefact for that **client + integration profile**.

So integration and client variance live in separate git repos for **history isolation**, but **not** separate at compile or runtime. The customized repo is a **client-specific monolith assembler**.

### Derived core services and sync processes

Between raw connector code and core base classes sits a third layer teams rely on daily:

```text
Connector (vendor protocol)
    → calls ClientAcmeImportService : ImportService   ← override in customized repo
        → calls base ImportService / repositories     ← core submodule
            → SaveChanges → handlers → Hangfire sync jobs
```

| Pattern | Example | Problem |
|---------|---------|---------|
| **Service override** | `protected override void AfterMap(...)` | Behaviour scattered; base class changes break clients silently |
| **Virtual hook** | `OnBeforeSync`, `CustomizeWbsNode` | Implicit contract — not documented as API |
| **Sync process** | Hangfire job in `client/sync` calling connector + service | Same sync logic duplicated in job, connector, and override |
| **DI swap** | Register `AcmeImportService` for `IImportService` | Runtime behaviour differs per customized build — not per tenant flag |

**Sync processes** (PLM structure pull, hours push, document link refresh) often span:

1. Connector — talks to external system  
2. Derived service — client-specific mapping, filtering, “what counts as changed”  
3. Core handler/job — persistence side effects nobody documented  

A developer fixing “PLM sync” must read **three layers in three repos**, none of which has a stable public contract.

### Why connectors cannot compile standalone

A typical connector project references:

- Core **entity types** and **DbContext**
- **Application/domain services** — often the **client subclass**, not the base
- **Override methods** or types defined only in the customized `client/` folder
- Shared **handler**, **sync**, and **job** registration across core + client layer

Opening only the PLM connector repo in Visual Studio → **missing references, build fails**. Opening connector + core but **without** client overrides → may compile but **wrong runtime behaviour** for that customer.

### What this is not

| Misconception | Reality |
|---------------|---------|
| “Connector is a plug-in DLL” | It is source compiled **into** the same solution as core **and** client overrides |
| “Separate repo = separate deployment unit” | Deployment is the **customized build**, not any single repo |
| “Core is the product, connectors are optional extras” | **Core + client overrides + connector SHAs** define what runs |
| “Sync is owned by the connector” | Sync logic is usually split across connector, derived service, and core jobs |

---

## 3. Why it seemed reasonable at the time

| Original benefit | What we wanted |
|------------------|----------------|
| **Isolate client/vendor code** | Acme SAP mapping should not pollute default core |
| **Per-customer delivery** | Ship connector only when customer pays for integration |
| **Parallel teams** | Integrator works in submodule without merging to main daily |
| **Client-specific data rules** | Override `MapHours`, `ShouldImportNode`, custom validation in derived service |
| **Reuse core logic** | Why rewrite import if `ImportService` already exists — subclass it |

Those goals are still valid. **Customized repos with submodules + inheritance overrides** are the wrong mechanism at platform scale.

---

## 4. What actually goes wrong

### 4.1 Dependencies point the wrong way

A connector should depend on a **small, stable contract** (ports, messages, file schema). Instead it depends on the **entire core**:

```text
  Desired (plug-in):     Connector ──► Integration contract (API / format)
                              Core implements contract

  Legacy (submodule):    Connector ──► Core entities, services, DbContext, handlers
                              Core knows nothing stable about connector
```

**Effect:** Every core refactor breaks connectors. Every connector needs a core version it was built against. Nobody can answer “which core SHA works with SAP connector 3.2?” without archaeology.

This violates **Dependency Inversion**: high-level core should not be the concrete dependency of low-level integration code.

---

### 4.2 Version matrix explosion

With customized repos you do not ship **one product** — you ship **compositions**:

```text
customized-repo-A  =  core@v2025.14  +  client-acme@main  +  plm@abc  +  eshare@def
customized-repo-B  =  core@v2025.14  +  client-contoso@x  +  sap@ghi
```

| Combination | Risk |
|-------------|------|
| Core submodule bumped, connector submodules not | Customized clone build breaks |
| Two connectors need incompatible core APIs | Cannot assemble one customized repo |
| PS maintains customized-repo fork for one client | Product repo diverges from delivery repo |
| Developer clones connector repo only | **Does not compile** — must know parent customized repo |
| QA asks “which Floor2Plan?” | Answer is **SHA tuple** across submodules, not a version number |

**Testing:** You cannot test “Floor2Plan” — you test one cell in the matrix. QA and support ask *which git SHAs* are running; business thinks they bought one product.

---

### 4.3 Mixed concerns: connector + client overrides + sync

Because connectors call **derived core services**, not stable ports:

- Vendor mapping may live in the **connector**, while “what to do with the result” lives in a **service override**
- **Sync schedules** in `client/sync` re-invoke the same paths as manual import — often with slightly different code
- The same client rule appears in **override**, **connector**, and **Hangfire job** — kept in sync manually
- Code review: “Is this PLM behaviour or client-specific handling?” — requires client layer + connector + core base

There is no line between **integration**, **customization**, and **domain** — all compiled into one binary.

**Effect:** Integration bugs look like core bugs. Fixing “sync” means tracing connector → `ClientXSyncService` override → Hangfire job → SaveChanges handlers.

---

### 4.4 Hidden orchestration

Legacy core often chains work through:

- **SaveChanges interceptors** / workflow handlers
- **Hangfire** jobs fired from services
- **Implicit** ordering (handler A before handler B)

Connectors that call `SaveChanges` or domain services **inherit that chain** without declaring it.

**Effect:**

- Import works in unit test, fails in production (different handler registration)
- “Small” connector change triggers unrelated billing side effect
- Cannot test connector in isolation — must boot half the monolith

Platform 2.0 rule: **`SaveChanges` is for persistence, not business process.** Orchestration belongs in explicit workflows (see `ApiImportActorPoc` actor pipelines).

---

### 4.5 Blocks modularization and Platform 2.0

The modularization program assumes:

- **One bounded context** owns WBS, Import, Hours, …
- **One DbContext per context** in target state
- **No cross-context writes** during import
- **Integration packs** at the edge, not inside domain entities

Submodule connectors **pin integration to core internals**:

| Goal | Submodule blocker |
|------|-------------------|
| Extract Import module | Connector still references monolith entities |
| Split DbContext | Connector uses old shared `DbContext` |
| Versioned public API | Connector bypasses API; calls services |
| Tenant packs on single core | Each tenant still implies submodule set |

You cannot strangler-migrate a domain while connectors surgically attach to the old guts.

---

### 4.6 Onboarding and operability

| Task | Customized-repo world |
|------|------------------------|
| New developer clones connector repo | Build fails — must clone **parent customized repo** and init submodules |
| New developer clones customized repo | `git submodule update --init --recursive`; easy to get wrong SHAs |
| CI build | Must reproduce exact submodule manifest per client profile |
| Security patch core | Retest **every customized composition** that pins that core commit |
| “Which SAP fields map to Activity?” | Search core + connector submodule + client folder in **that** clone |
| Enable integration for tenant | New customized repo variant or submodule pin — not a runtime flag |
| Open PLM connector in IDE alone | **Cannot compile** — architectural constraint of current model |

Platform 2.0 target: **enable integration pack in admin backoffice** — not add a git submodule.

---

### 4.7 Vendor and client types leak into core

When connectors reference core entities, pressure grows to:

- Add `SapSpecificField` to shared tables
- Branch `if (client == Acme)` in core services
- Fork core “slightly” for one connector

**Effect:** Core stops being canonical. “The domain model” is different per deployment — exactly what multi-tenant Platform 2.0 must avoid.

Target: **external ID registry** + mapping in the pack; core keeps stable invariants (`ApiImportActorPoc` external id rules are the reference).

---

## 5. Symptoms your team already recognizes

If you have said any of these, you are paying the customized-repo tax:

- “Clone the **customized** repo, not the connector repo alone.”
- “It works on my machine but not on the patch environment.”
- “We need to merge core before we can merge the connector.”
- “Which **submodule SHAs** is the client running?”
- “Our customized repo pins core to a branch the connector team doesn’t use.”
- “The connector repo doesn’t build — you need the parent repo with `core/` and `client/` checked out.”
- “Which **override** of `ImportService` is registered for this deployment?”
- “Sync failed — is it the connector, the `AcmeSyncService` override, or the Hangfire job?”

These are **structural** problems, not lack of discipline.

---

## 6. What good looks like (Platform 2.0)

```text
 External system (SAP, Kronos, PLM, …)
        │
        ▼
┌───────────────────┐
│ Integration pack  │  ← versioned NuGet/deployable; maps vendor ↔ contract
│ (connector)       │     NO reference to core EF entities
└─────────┬─────────┘
          │ intermediate format OR versioned core API / commands
          ▼
┌───────────────────┐
│ F2P Core modules  │  ← Import, WBS, Planning, Hours, …
│ · domain rules    │     owns invariants + persistence boundaries
│ · import API      │
│ · external ID map │
└───────────────────┘
```

| Principle | Implementation |
|-----------|----------------|
| **Stable boundary** | Intermediate exchange format (inbound) + OpenAPI ports (outbound) |
| **Pack owns vendor** | SAP / PLM / eShare protocol → canonical format |
| **Pack or customization pack owns client variance** | Tenant profile + versioned packs — **not** `ClientXService : BaseService` in a customized clone |
| **Core owns domain** | One import pipeline; idempotent upsert; external IDs |
| **Sync is explicit** | Actor/workflow with correlation ID — not override + handler chain |
| **Tenant enables pack** | Configuration, not customized repo + DI subclass swap |
| **Testability** | Golden files: vendor sample → intermediate JSON → import result |
| **Lead vs follow explicit** | Per entity type — documented, not assumed (see integrations deep-dive) |

Reference POC: `ApiImportActorPoc/` — import actors, external IDs, single EF boundary per workflow.

Policy (approved direction): **no new git submodules for connectors or client core forks.**

---

## 7. “But we need access to core types” — alternatives

| Legacy habit | Replace with |
|--------------|--------------|
| Connector calls `WbsService` | `ImportProject` command / canonical batch / actor message |
| Connector builds `Activity` entities | Map to **DTO / intermediate format**; core materializes entities |
| Client override `AcmeImportService` | **Customization pack** hook or tenant profile rule on canonical payload |
| Sync in client folder + Hangfire | **Supervised workflow actor** per integration; observable pipeline |
| Connector shares `DbContext` | Core persistence only; connector never opens SQL |
| DI registers subclass per client | Host registers **packs per tenant** — one core binary |

**Rule of thumb:** If the connector **cannot be compiled** without the core solution, the boundary is wrong.

---

## 8. FAQ

### “Submodules keep client code out of main — isn’t that separation?”

It separates **git history**, not **runtime or compile dependencies**. The connector still **couples** to core internals. Real separation is a **published contract** (format or API) that core and pack evolve independently.

### “Integration packs sound like submodules with another name.”

Packs depend on **contracts**, not on `YourApp.Domain.Entities.Project`. You can version `sap-projects-v1` against `core-api-v2` with a compatibility matrix. Submodules version against **whatever class names exist in core today**.

### “We’ll fix discipline — stricter reviews.”

Discipline helps at the margins. The architecture **rewards** shortcuts (subclass core service, call `DbContext`). Platform 2.0 makes the right path the easy path.

### “What about on-prem clients with odd SAP configs?”

**Pack configuration** and mapping tables — not a core submodule. Same pack binary, different tenant settings.

### “Can we migrate one connector at a time?”

Yes — **strangler**: adapter calls legacy until pack produces intermediate format; core import path unchanged. See `platform-rebuild-proposal-summary.md` Section 6 (portability / intermediate format).

---

## 9. Team rules (legacy → transition)

**Stop doing**

- New git submodules that reference core for connectors or client logic
- New `SaveChanges` handlers for integration side effects
- New direct entity manipulation from connector code

**Start doing**

- Classify integration: **lead vs follow** per entity (integrations deep-dive)
- Map vendor data to **intermediate format** or **versioned API DTO**
- Characterization tests on existing connector behaviour before moving code
- Document which **bounded context** owns each integration point

**When touching legacy connector code**

- Do not widen coupling (no new core type references)
- Prefer thin adapter toward new import API
- Tag temporary bridges `[StranglerAdapter]` + ticket to remove

---

## 10. Further reading (this repo)

| Document | Topic |
|----------|--------|
| `docs/monolith-modularization/claude-external-integrations-deepdive-instructions.md` | Lead/follow, integration catalog, packs |
| `ApiImportActorPoc/docs/platform-rebuild-proposal-summary.md` | Submodule policy, portability hub, journey stages |
| `ApiImportActorPoc/README.md` | External IDs, import actor boundary |
| `docs/floor2plan-v2-read-model-playbook.md` | V2 read paths (orthogonal but same boundary thinking) |

---

## 11. One-slide summary

> **Legacy:** Each connector has its own repo but cannot compile without a **customized repository**: `core/` submodule, **client-derived service overrides**, connector siblings, and sync jobs — all built as one solution. That gives git separation without architectural separation. **V2:** One core binary; **integration packs** (vendor) and **customization packs** (tenant rules) depend on contracts; sync is explicit orchestration — not inheritance overrides in a per-client clone.
