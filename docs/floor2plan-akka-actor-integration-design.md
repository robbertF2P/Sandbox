# Floor2Plan - P6 Integration via Akka Actors

**Purpose:** A reliable, flexible P6 connector — reusable across customers, adaptable to any mapping.

**Status:** Draft for discussion — September 2026

---

## Why this approach

P6 integration today lives in forked branches and service overrides. Hard to reuse, hard to maintain per customer.

This design separates concerns:

| | Core | Customer |
|---|------|----------|
| **Owns** | Data, ActorSystem, facade | P6 actor, mapping, P6 config |
| **Ships as** | Shared platform | Small customization project |

One shared **P6.Client**, one **mapper per customer**. Customers with an existing P6 integration plug in their mapping — same actor shape, different mapper.

Akka.NET runs P6 calls asynchronously with retries and supervision, without blocking the web app.

---

## Context

### Today

```mermaid
flowchart TB
  subgraph tenant["Tenant customization branch"]
    OVR["Service overrides"]
    SUB["git submodule to core"]
  end

  subgraph core["Core repo"]
    SVC["Core services"]
    DB[("DB")]
  end

  SUB --> core
  OVR --> SVC
  SVC --> DB
```

### Target

```mermaid
flowchart TB
  subgraph core["Core"]
    FACADE["IActorSystemFacade"]
    AS["ActorSystem"]
    DATA["DataActor"]
    CLIENT_LIB["P6.Client shared"]
  end

  subgraph customer["Customer customization"]
    ACTOR["P6SyncActor"]
    MAP["Mapper"]
  end

  FACADE --> AS
  AS --> ACTOR
  ACTOR --> MAP --> CLIENT_LIB
  ACTOR --> DATA
```

Core stays stable. Customer brings actor + mapper. Any mapping fits.

---

## View 1 — .NET components

```mermaid
flowchart TB
  subgraph host["Host"]
    API["API and services"]
    SVC["Core application services"]
    EF["DbContext"]
    HOST["Akka hosting"]
    AS["ActorSystem"]
    FACADE["IActorSystemFacade"]
    DATA_ACTOR["DataActor"]
  end

  subgraph customer["Customer customization"]
    P6_ACTOR["P6SyncActor"]
    MAP["Mapper"]
    REG["Register at startup"]
  end

  subgraph shared["Shared"]
    P6_CLIENT["P6.Client"]
    CONTRACTS["Contracts"]
  end

  subgraph external["External"]
    DB[("F2P database")]
    P6API["P6 EPPM API"]
  end

  API --> SVC --> EF --> DB
  HOST --> AS
  FACADE --> AS
  FACADE --> P6_ACTOR
  DATA_ACTOR --> SVC
  REG --> FACADE
  P6_ACTOR --> MAP --> P6_CLIENT --> P6API
  P6_ACTOR --> DATA_ACTOR
  P6_ACTOR --> CONTRACTS
```

| Component | Role |
|-----------|------|
| **IActorSystemFacade** | Register customer actors; `Tell` commands (`StartP6Sync`, `RawDataTest`) |
| **P6.Client** | Shared HTTP client — auth, retries, optional request/response logging |
| **Mapper** | Customer-specific — F2P shapes to EPPM |
| **P6SyncActor** | Sync commands, P6 HTTP via PipeTo, forwards updates to DataActor |
| **DataActor** | Persists inbound P6 data via application services |
| **Contracts** | Typed commands and events with correlation fields |

### Commands vs events

| Path | Mechanism | When |
|------|-----------|------|
| **Commands** | `facade.Tell(...)` | Sync start, raw data test, updates to DataActor |
| **Domain events** | EventStream after commit | P6 actor subscribes to changes it cares about |

---

## View 2 — Actor flows

```mermaid
flowchart TB
  BTN["Raw data test button"] --> FACADE["Facade Tell"]
  FACADE --> P6["P6SyncActor"]
  P6 --> MAP["Mapper"]
  MAP --> P6API["P6 API"]
  P6 --> DATA["DataActor"]
  DATA --> DB[("F2P DB")]
```

### Raw data test

Connector bypass — no import, no DB write. Calls P6.Client read and displays the result. Verifies connectivity and mapping.

### Sync start

`StartP6Sync` → P6 actor → P6 API → DataActor when data lands in F2P.

### Reactive sync

After commit, publish to EventStream → P6 actor handles relevant events.

---

## Flexible mapping

```mermaid
flowchart LR
  subgraph shared["Shared"]
    CLIENT["P6.Client"]
  end

  subgraph customers["Per customer"]
    M1["Customer A mapper"]
    M2["Customer B mapper"]
    A1["P6SyncActor"]
    A2["P6SyncActor"]
  end

  M1 --> A1 --> CLIENT
  M2 --> A2 --> CLIENT
```

| Customer situation | What they bring |
|------------------|-----------------|
| **New P6 integration** | Mapper + actor + config |
| **Existing P6 integration** | Mapping rules in the mapper; same actor shape |
| **Different EPPM fields** | Mapper only — P6.Client unchanged |

New customer: reference **P6.Client**, implement **mapper**, register **actor**.

---

## Suggested interfaces

```csharp
public interface IActorSystemFacade
{
    void Tell<TCommand>(TCommand command) where TCommand : IActorCommand;
    IActorRef RegisterActor(string name, Props props);
}

public interface IP6Client
{
    Task<P6Response> SendAsync(EppmMessage message, P6CallContext context, CancellationToken ct);
}

public interface IEventToEppmMapper
{
    bool CanHandle(IActorMessage message);
    EppmMessage Map(IActorMessage message);
}
```

`Props` wraps the factory that creates the actor — customer builds it with their mapper and client at registration.

---

## Observability

- `SyncId` per sync run, `CorrelationId` from HTTP, `TenantId` on log scope
- Optional P6 request/response logging when debugging mapping
- Filter logs by `SyncId` to trace a sync end-to-end

---

## Phased rollout

| Phase | Goal |
|-------|------|
| **1** | ActorSystem + facade in core. Dummy actor receives a message. |
| **2** | Raw data button → P6 actor → mock client → display result. |
| **3** | Real mapper for first customer. Mock or real P6.Client. |
| **4** | DataActor persists inbound data. Full sync round-trip. |
| **5** | After-commit EventStream for reactive sync. |

---

## Implementation notes

- Publish after commit, not inside `SaveChanges`
- P6SyncActor: state machine, PipeTo for HTTP, no blocking in Receive
- DataActor calls application services, not DbContext directly
- Typed messages in Contracts; correlation on every sync run
- Idempotent P6 calls for reactive sync

---

*September 2026*
