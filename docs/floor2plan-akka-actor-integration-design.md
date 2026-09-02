# Floor2Plan - Akka Actor Integration Design

**Purpose:** Design for moving tenant customization (e.g. P6 / EPPM integration) from service inheritance on forked branches to custom actors registered against a core ActorSystem.

**Status:** Draft for discussion

---

## Context

### Today

Each Floor2Plan tenant gets a customized branch with a submodule pointing at core. Core services are inherited and overridden in the customization branch.

```mermaid
flowchart TB
  subgraph tenant["Tenant customization branch"]
    OVR["AcmeImportService overrides ImportService"]
    OVR2["AcmePlanningService overrides PlanningService"]
    SUB["git submodule to core repo"]
  end

  subgraph core["Core repo"]
    SVC["ImportService, PlanningService"]
    DB[("DB")]
  end

  SUB --> core
  OVR --> SVC
  OVR2 --> SVC
  SVC --> DB
```

**Pain:** merge drift, hidden coupling, hard to see what a tenant changed.

### Target

Core stays one codebase. Tenant customization adds actors and mapping — not service subclasses. Side effects that today live in EF `SaveChanges` change handlers move onto an Akka bus; tenant actors subscribe and react.

---

## Design principles

| Principle | Detail |
|-----------|--------|
| **Thin facade** | `IActorSystemFacade` — register actors, `Tell` only. No `Ask` (that stays at HTTP/controller boundaries if needed). |
| **Core owns data** | Import, entities, `DataActor` — canonical persistence into F2P database. |
| **Tenant owns integration** | P6 actor, EPPM mapping, P6 client configuration. |
| **Reusable P6 client** | Shared `P6.Client` NuGet; per-tenant mapping only. |
| **Events = state changed** | User interaction → save → bridge publishes onto Akka bus. |
| **Bridge, not subsystem** | Change handler bridge is a thin hook onto the bus — same trigger point as today. |

---

## View 1 — .NET components

How the solution is built and wired.

```mermaid
flowchart TB
  subgraph host["Host"]
    API["API and services"]
    SVC["Core services"]
    EF["DbContext"]
    CH["SaveChanges change handlers"]
    IMP["Import core entities"]
    HOST["Akka hosting"]
    AS["ActorSystem"]
    FACADE["IActorSystemFacade"]
    BRIDGE["Change handler bridge"]
    BUS["Akka bus"]
    DATA_ACTOR["DataActor persists updates"]
  end

  subgraph core_lib["Core libraries"]
    CONTRACTS["Contracts events and messages"]
  end

  subgraph tenant["Tenant customization"]
    P6_PROJ["Acme.P6Integration"]
    MAP["Event to EPPM mapper"]
    P6_ACTOR["P6SyncActor"]
    REG["Register at startup"]
  end

  subgraph shared["Shared"]
    P6_CLIENT["P6.Client"]
  end

  subgraph external["External"]
    DB[("F2P database")]
    P6API["P6 EPPM API"]
  end

  API --> SVC
  SVC --> IMP --> EF --> DB
  EF --> CH
  CH -.-> BRIDGE
  BRIDGE -.-> BUS
  HOST --> AS
  FACADE --> AS
  DATA_ACTOR --> EF
  REG --> FACADE
  BUS -.-> P6_ACTOR
  P6_ACTOR --> MAP --> P6_CLIENT --> P6API
  P6_ACTOR --> DATA_ACTOR
  P6_PROJ --> CONTRACTS
  P6_PROJ --> FACADE
  P6_PROJ --> P6_CLIENT
```

### Component roles

| Component | Owner | Role |
|-----------|-------|------|
| **Core services + import** | Core | User actions, import into core entities |
| **DbContext + change handlers** | Core | Persist; detect what changed |
| **Change handler bridge** | Core | Thin hook — publishes state-change events onto Akka bus |
| **ActorSystem + facade** | Core | Runs actors; tenant registers via facade |
| **DataActor** | Core | Handles update messages; imports/writes into F2P DB correctly |
| **\*.Contracts** | Core | Shared events and messages |
| **P6.Client** | Shared | Reusable P6 / EPPM HTTP client (auth, retries, wire format) |
| **Event → EPPM mapper** | Tenant | Maps bus events to EPPM messages |
| **P6SyncActor** | Tenant | Handles sync-start messages (user/system); reacts to core update events it deems relevant; calls P6; sends update messages to DataActor |
| **Registration** | Tenant | `facade.RegisterActor(...)` at startup |

---

## View 2 — Actor organization

How work flows inside the ActorSystem.

```mermaid
flowchart TB
  BTN["Raw data test button"] --> P6["P6SyncActor tenant"]
  CH["Change handler bridge"] --> BUS["Akka bus"]
  BUS --> P6
  P6 --> MAP["Map to EPPM message"]
  MAP --> P6API["P6 EPPM API"]
  P6 --> DATA["DataActor core"]
  DATA --> DB[("F2P DB")]
```

### Flows

**Sync start (user or system initiated)**  
Scheduled job or explicit sync trigger → `facade.Tell(StartP6Sync)` → P6 actor → P6 API → update messages to DataActor when persisting.

**Raw data test (connector bypass)**  
Raw data button bypasses the connector import path — it does not import into F2P. It is a quick test: call the API client read, display what comes back. Useful to verify P6 connectivity and mapping before wiring full sync.

**F2P → P6 (reactive)**  
User saves → change handler → bridge publishes core update event → P6 actor handles only events it deems relevant → maps to EPPM → P6 API.

**P6 → F2P (inbound)**  
P6 actor receives data / response → sends update message → DataActor → F2P database.

Tenant actor never touches `DbContext` directly.

---

## Reusable P6 client vs tenant mapping

```mermaid
flowchart LR
  subgraph shared["Reusable all tenants"]
    CLIENT["P6.Client"]
    DTO["EPPM DTOs"]
  end

  subgraph tenant["Per tenant"]
    MAP["IEventToEppmMapper"]
    ACTOR["P6SyncActor"]
  end

  START["RawDataTest or StartP6Sync"] --> ACTOR
  BUS["Core update events selective"] --> ACTOR
  ACTOR --> MAP --> CLIENT --> P6API["P6 API"]
```

| Layer | Reusable? | Responsibility |
|-------|-----------|----------------|
| **P6.Client** | Yes | Auth, endpoints, send/receive, errors, retries |
| **EPPM DTOs** | Yes | P6 wire format |
| **IEventToEppmMapper** | No — per tenant | Sync commands + selected bus events → EPPM message |
| **P6SyncActor** | No — per tenant | Handle sync-start messages; react to relevant core update events; call client; forward updates to DataActor |

### Migrating to another tenant

1. Reference shared **P6.Client** NuGet.
2. Implement their **IEventToEppmMapper**.
3. Register their **P6SyncActor** via facade.
4. Configure P6 URL/credentials in appsettings.

No fork of core. No fork of the client.

---

## Suggested interfaces

```csharp
// Core — facade (Tell only; no Ask inside the actor system)
public interface IActorSystemFacade
{
    void Tell(object message);
    void RegisterActor(string name, Props props);
}

// Shared — reusable
public interface IP6Client
{
    Task SendAsync(EppmMessage message, CancellationToken ct);
}

// Tenant — per client
public interface IEventToEppmMapper
{
    bool CanHandle(object message);   // sync-start commands + selected core update events
    EppmMessage Map(object message);
}
```

---

## Phased rollout

### Phase 1 — Core plumbing

- Add Akka hosting (`IHostedService`) to core.
- Implement `IActorSystemFacade`.
- Smoke test: dummy actor receives a message.

### Phase 2 — Raw data button (connector bypass, quick test)

- Wire raw data button as a connector bypass — skips import, no DB write.
- Calls API client read only and displays the response (verify connectivity and mapping).
- `facade.Tell(RawDataTest)` to route through P6 actor + mock client.

### Phase 3 — P6 actor + mock

- Implement `P6SyncActor` in tenant customization.
- Handle `RawDataTest` and `StartP6Sync` messages.
- Mock `IP6Client` returns sample EPPM data for display.
- Register actor at startup.

### Phase 4 — Change handler bridge

- Bridge publishes core update events onto Akka bus on `SaveChanges`.
- P6 actor handles only events it deems relevant.
- Tenant mapper converts those events to EPPM messages.

### Phase 5 — DataActor + real P6

- Core `DataActor` handles update messages from P6 actor when full sync persists to F2P.
- Swap mock for real `P6.Client`.

---

## Today vs tomorrow

| | Today | Tomorrow |
|---|-------|----------|
| **Tenant variance** | Inherit & override core services on forked branch | Register actors + mapper |
| **P6 integration** | Buried in handlers / subclasses | `P6SyncActor` + shared `P6.Client` |
| **Trigger** | SaveChanges change handlers | Same — bridge publishes to bus |
| **DB writes from P6** | Ad-hoc in customization | `DataActor` in core |
| **New tenant** | New branch, merge pain | New mapper + actor registration |

---

## One-liner

> Core runs the ActorSystem and owns data import via DataActor. A thin bridge publishes EF state changes onto the Akka bus. Tenant customization registers a P6 actor that handles sync-start messages and relevant core update events, maps to EPPM, and calls a shared P6 client — instead of overriding core services on a forked branch.

---

*Document generated from design discussion — September 2026*
