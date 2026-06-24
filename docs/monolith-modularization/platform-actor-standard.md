# Platform actor standard

**Purpose:** Standard for Akka.NET workflow orchestration in Platform 2.0 — the **composition runtime** for client-specific integrations, tenant customization, and legacy strangler bridges.

**Audience:** Engineers and AI agents scaffolding modules, integration packs, or workflow hosts.

**Related:**

| Document | Role |
|----------|------|
| `platform-correlation-standard.md` | `CorrelationId` / `UseCase` across HTTP → actors → events → SignalR |
| `platform-logging-standard.md` | Serilog + `ILoggingAdapter` in actors |
| `../floor2plan-v2-connector-architecture.md` | Integration pack dependency rules |
| `ApiImportActorPoc/docs/platform-rebuild-proposal-summary.md` | Strategic context (§11) |
| `ApiImportActorPoc/docs/deployment-profile-sketch.md` | Legacy-hosted vs native tenant routing |
| `module-composition-di.md` | `IServiceCollection` registration; host is composition root |

**Reference POCs:** `ApiImportActorPoc/`, `AkkaSignalRVuePoc/`, `F2pPlatform/`

---

## Core principle

> **Core stays canonical and tenant-agnostic.** All client variance is **composed** at the host: integration packs supply vendor mapping; customization packs supply tenant rules; actors supply orchestration; strangler adapters supply legacy delegation.

Actors are not a replacement for a disciplined domain model. They are the **explicit workflow boundary** where:

- Long-running import/export and integration pipelines run
- Tenant-specific steps plug in without `if (tenant == Acme)` in core
- Legacy behaviour delegates behind `[StranglerAdapter]` until cutover
- Supervision, retries, and correlation replace SaveChanges handler chains and ad-hoc Hangfire logic

---

## Non-negotiable rules

| Rule | Detail |
|------|--------|
| **Orchestration at boundaries** | Actors coordinate workflows; domain rules stay in plain, tested modules (Application/Domain) |
| **One EF boundary per workflow** | Only the designated **persist actor** (or Infrastructure port it calls) touches `DbContext` for that flow |
| **No `Ask` inside actors** | `Tell` / `Forward` internally; `Ask` only at HTTP/facade boundaries (see `.cursor/rules/actor-system-contracts.mdc`) |
| **Packs, not forks** | Client integrations and customizations ship as **versioned packs** enabled per tenant — not submodules or `ClientXService : BaseService` |
| **No SaveChanges orchestration** | `SaveChanges` persists; business process runs in explicit actor pipelines |
| **Correlation required** | Workflow commands use `AskCorrelated` / `TellCorrelated`; actors use `ReceiveCorrelated` (see correlation standard) |
| **Legacy is explicit** | `[StranglerAdapter]` actors or ports delegate to legacy; document removal ticket and sunset date |
| **Messages in Contracts** | Commands, queries, and events live in `*.Contracts`; actor implementations in `*.Core` |

---

## When to use actors

| Use actors for | Do **not** use actors for |
|----------------|---------------------------|
| Import/export pipelines | Simple CRUD with no workflow |
| Integration fetch → map → persist → notify | Sprinkling `DbContext` in every actor |
| Long-running recalculations (planning rollups) | Replacing a well-factored domain model |
| Tenant-specific pipeline stages (pack-registered) | Synchronous request/response with no concurrency need |
| Legacy strangler handoff (native → legacy runtime) | Business rules that belong in Application services |

---

## Workflow model

```text
HTTP / queue / file watcher / scheduler tick
    → Facade (AskCorrelated at system boundary)
        → IntegrationRouterActor (tenant profile selects pack pipeline)
            → [VendorFetchActor]     ← integration pack
            → [MapToCanonicalActor]  ← integration pack
            → [TenantRulesActor]     ← customization pack (optional)
            → OrchestratorActor      ← context manager (ImportManager, DataManager, …)
                → PersistActor       ← sole EF boundary for this workflow
                → [LegacyStranglerActor]  ← when deploymentProfile.mode = legacy_hosted
            → Notification / rollup / outbound actors
        → Domain events (past tense, with correlation)
        → SignalR / outbox (optional)
```

**Command flow example:**

```text
ImportProject
    → ImportManagerActor
        → ImportSessionActor (per session)
            → DataManagerActor
                → ProjectImportDataActor (persist)
```

Legacy replacement mapping (from rebuild proposal §11):

| Legacy | Actor approach |
|--------|----------------|
| `AcmeImportService : ImportService` | `ImportPipeline → [MapActor, AcmeRulesActor, PersistActor]` |
| Client submodule | Tenant profile registers actor pack |
| EF change handler chain | Command → orchestrator → persist actor |
| Hangfire job with business logic | Supervised workflow actor; Hangfire optional as clock during migration |

---

## Tenant packs and host composition

Integration and customization variance **never** branches in core code.

### Pack types

| Pack | Responsibility | References |
|------|----------------|------------|
| **Integration pack** | Vendor fetch, file read, map to canonical DTO/JSON | `F2P.Integration.Abstractions` + vendor SDKs only |
| **Customization pack** | Tenant-specific validation, mapping tweaks, extra pipeline stages | Abstractions + own rules; no core Domain |
| **Strangler adapter** | Delegate to legacy service/API/DB when native path not ready | Infrastructure; `[StranglerAdapter]` + removal ticket |

### Host registration (composition root only)

```csharp
// Host Program.cs — illustrative
services.AddImportModule(configuration);
services.AddAkkaActors(configuration, registry =>
{
    registry.RegisterPackPipeline("sap-v1", builder => builder
        .AddStage<SapFetchActor>()
        .AddStage<MapSapToCanonicalActor>());
    registry.RegisterPackPipeline("acme-rules-v1", builder => builder
        .AddStage<AcmeValidationActor>());
});
```

Tenant `packEntitlements` (control plane) determines which pipelines attach — see `deployment-profile-sketch.md`.

### Pack rules (from connector architecture)

| Allowed | Forbidden |
|---------|-----------|
| Call external APIs; map to canonical format | Call `WbsService`, `ImportService`, or any core application service |
| Validate vendor-specific rules | `if (tenant == Acme)` in core |
| Implement outbound port for push | Register SaveChanges handlers in core |
| Emit telemetry with correlation ID | Construct or mutate EF entities outside persist boundary |

---

## Legacy support

When a tenant remains on **legacy-hosted** mode (`deploymentProfile.mode = legacy_hosted`):

1. Control plane routes user traffic to legacy runtime; v2 owns registry, packs metadata, and cutover orchestration.
2. New workflows may still run in v2 but **persist or query via strangler adapter** until parity is proven.
3. Same `CorrelationId` / `UseCase` crosses native and legacy sides for observability.
4. Every strangler path has a **documented sunset criterion** — avoid "two systems forever."

```text
Native orchestrator
    → LegacyStranglerAdapter (Infrastructure)
        → legacy HTTP / legacy service interface / legacy DB connection ref
```

Adapters live in **Infrastructure**, registered explicitly in `Add<Context>Infrastructure` — not in Domain or Application.

---

## Project layout

Aligned with module template and POCs:

```text
<Context>.Contracts/
  Messages/          ← IActorSystemMessage commands and queries
  Events/            ← IActorSystemEvent / IDataEvent (past tense records)

<Context>.Core/      ← or <Context>.Application.Actors if team prefers
  Actors/
    RootActor.cs
    <Workflow>ManagerActor.cs
    <Workflow>SessionActor.cs

<Context>.Infrastructure/
  Actors/            ← optional: persist actors with EF
  Strangler/         ← [StranglerAdapter] implementations

<Context>.Api/
  Services/
    AkkaActorHostedService.cs
    ActorSystemCommandFacade.cs   ← AskCorrelated boundary for HTTP
```

`Domain` has no Akka references. Application ports define persistence and legacy interfaces; actors call those ports.

---

## Messaging conventions

| Kind | Location | Naming |
|------|----------|--------|
| Command | `*.Contracts/Messages/` | Imperative: `StartImportCommand` |
| Query | `*.Contracts/Messages/` | `GetImportModelQuery` |
| Event | `*.Contracts/Events/` | Past tense PascalCase: `ImportPersisted` |
| Facade | `*.Api/Services/` | `IActorSystemCommandFacade` — `AskCorrelated` only |

Workspace rule: `.cursor/rules/actor-system-contracts.mdc` (AkkaSignalRVuePoc patterns).

Skills: `akka-net`, `platform-correlation`, `reactive-applications-akka-net`.

---

## Hosting checklist

- [ ] `build/Platform.Logging.Akka.props` on API + Core actor projects
- [ ] `services.AddAkka<THostedService>(...)` with named top-level actors
- [ ] `IActorSystemCommandFacade` (or `IRequiredActor<T>`) for HTTP entry — no direct `ActorSystem` in controllers
- [ ] `UsePlatformCorrelationPipeline()` before actor facades
- [ ] Supervision strategy documented for integration stages (restart vs escalate)
- [ ] Persist actor is the only type that opens `DbContext` for the workflow
- [ ] Pack stages registered from tenant profile, not hard-coded in core actors
- [ ] Strangler adapters tagged `[StranglerAdapter]` with removal ticket
- [ ] Characterization tests through HTTP facade; actor unit tests via TestKit

---

## Extraction sequence (where actors land)

Actors follow module extraction — not before characterization tests exist:

```text
1. Extract module (Domain / Application / Infrastructure / Api) with strangler adapter
2. Characterize P0 behaviour through existing entry points
3. Introduce orchestrator + persist actors for one workflow slice
4. Register integration/customization pack stages per tenant
5. Retire SaveChanges handlers and Hangfire business logic for that workflow family
6. Sunset strangler adapter when deployment profile moves to native
```

See `03-modularization-roadmap.md` step 5: `ApiImportActorPoc` → production import host slice.

---

## Definition of done — actors (per workflow slice)

- [ ] Workflow drawn as message diagram (orchestrator → persist → events)
- [ ] No `DbContext` outside designated persist boundary
- [ ] Correlation on all boundary commands and published events
- [ ] Pack stages isolated; core has zero tenant/vendor branches
- [ ] Legacy path (if any) uses strangler adapter with sunset ticket
- [ ] TestKit coverage on orchestrator routing; characterization test on HTTP happy path
- [ ] Seq query by `CorrelationId` demonstrates full flow in dev

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-23 | Initial standard — actors as integration/customization/legacy composition runtime |
