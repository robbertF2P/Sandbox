---
name: reactive-applications-akka-net
description: |
  Guides reactive system design and Akka.NET development following Anthony Brown's
  "Reactive Applications with Akka .NET" (Manning, 2019). Use when:
  - Deciding whether Akka.NET fits a problem (vs threads, locks, or reactive libraries)
  - Designing actor-based systems with messaging, supervision, and failure isolation
  - Modeling state with behaviors, FSMs, routers, remoting, or clustering
  - Integrating actors with ASP.NET Core, SignalR, persistence, or custom IO
  - Testing actor systems (unit, integration, multi-node) or reviewing reactive design
  For repo-specific Akka.Hosting patterns in this workspace, also use the `akka-net` skill.
paths:
  - "**/*Akka*.cs"
  - "**/*Actor*.cs"
  - "**/AkkaSignalRVuePoc/**"
  - "**/ApiImportActorPoc/**"
metadata:
  version: 1.0.0
  source: "Anthony Brown — Reactive Applications with Akka .NET (Manning, 2019)"
---

# Reactive Applications with Akka .NET

Apply this skill for **reactive system design** and Akka.NET architecture. Brown's book teaches building reliable, concurrent, scalable .NET systems through the actor model and message passing — without manual thread/lock management.

**Core principle:** Isolate components, communicate asynchronously, and let failures stay local. Design for **resilience, elasticity, and responsiveness** at the system level.

For concrete patterns already used in this repo (`AkkaSignalRVuePoc`, `ApiImportActorPoc`), combine with the **`akka-net`** skill.

## Reactive systems vs reactive programming

| | Reactive **systems** | Reactive **programming** |
|---|----------------------|--------------------------|
| Scope | Architecture — multiple services/components | Libraries — Rx, LINQ, async streams |
| Goal | Resilience, elasticity, isolation under load | Composable event/async pipelines |
| Tooling | Akka.NET, messaging, supervision | `IObservable`, `async`/`await` |

Do not conflate them. Akka.NET builds **reactive systems**; you may still use reactive programming inside actors for local streams.

## Reactive Manifesto (design lens)

Every architectural choice should support these traits:

| Trait | Meaning in practice |
|-------|---------------------|
| **Responsive** | Timely responses under normal and failure conditions; degrade gracefully |
| **Resilient** | Failures stay isolated; recovery without full-system restart |
| **Elastic** | Scale up/out under load; scale down when idle |
| **Message-driven** | Async messages between loosely coupled components; back-pressure where needed |

**Red flag:** shared mutable state, synchronous RPC chains, or one global lock for concurrency.

## When to use Akka.NET

**Good fits:**

- High concurrency with **isolated units of work** (sessions, orders, devices, imports)
- **Stateful** processing where one mailbox serializes access per unit
- **Failure domains** that should restart independently (supervision trees)
- **Distributed** or clustered workloads (routers, remoting, cluster sharding)
- Long-running background processes with timers and lifecycle

**Poor fits:**

- Simple CRUD with no concurrency or resilience requirements
- Request/response-only APIs with no stateful or streaming behavior
- Teams unwilling to adopt message-passing mental model
- Replacing a well-working `async`/`await` pipeline with actors "for consistency"

## Actor model fundamentals

An **actor** embodies:

- **Identity** — addressable via `IActorRef` (not the instance)
- **State** — private; only the actor's mailbox thread mutates it
- **Behavior** — how incoming messages are handled
- **Mailbox** — FIFO queue; one message processed at a time per actor

An actor can:

- **Send** messages (`Tell`, `Ask`)
- **Create** child actors (`Context.ActorOf`)
- **Change behavior** (`Become` / `Unbecome`, or FSM)
- **Supervise** children (restart/stop/resume on failure)

### Messaging conventions

| Pattern | Use |
|---------|-----|
| `Tell` (fire-and-forget) | Default; no reply expected |
| `Ask` | Request/reply; always handle timeout and failure |
| `Forward` | Pass message while preserving original sender |
| `PipeTo` | Deliver async task result back to self or another actor |

**Rules:**

- Messages should be **immutable** POCOs or records.
- Prefer **small messages** (IDs, commands) over large object graphs.
- Never block inside `Receive` (no `.Result`, `.Wait()`, `Thread.Sleep`).
- One `ActorSystem` per application — not per request.

### Props and deployment

- **`Props`** — recipe for creating an actor (type + constructor args + dispatcher/router config).
- **Actor system** — root container; hosts guardians, dispatchers, event stream.
- **HOCON** — configuration for dispatchers, mailboxes, remoting, cluster (load via `ActorSystem` or hosting extensions).

Resolve dependencies at **Props creation time**, not by injecting `IServiceProvider` into actors.

## State and behavior

### Switchable behaviors

Use `Become` / `Unbecome` when an actor handles different message sets in different lifecycle phases (e.g. idle vs processing vs shutting down).

### Finite state machines (FSM)

Model explicit states and transitions when:

- Behavior is **state-dependent** with clear transitions
- Invalid transitions must be rejected
- Timeouts or events drive state changes

Akka.NET provides `FSM<TState, TData>` or model states with `Become` in `ReceiveActor`.

**Good:** shopping cart → `Empty` | `HasItems` | `CheckingOut` with guarded transitions.

**Bad:** boolean flags scattered across handlers without transition rules.

## Failure handling

### Supervision tree

Parents supervise children. On child failure, the supervisor chooses:

| Directive | Effect |
|-----------|--------|
| **Restart** | New actor instance; state lost unless persisted |
| **Stop** | Terminate child permanently |
| **Resume** | Continue with same instance |
| **Escalate** | Delegate failure to own supervisor |

Design supervision to match **failure domains** — restart the smallest unit that can recover cleanly.

### Application-level failures

- **Fail fast** inside actors; let supervision handle recovery.
- Use **`Watch`** / `Receive<Terminated>` to react when other actors die.
- Distinguish **interface-level** failures (bad input) from **infrastructure** failures (network, DB).

### Transport-level failures

In distributed systems:

- Messages may be **lost** or **duplicated** — design idempotent handlers where needed.
- Use **at-least-once** delivery patterns (Akka.Persistence, acknowledgments) when required.
- **DeathWatch** across remoting detects remote actor loss.

## Scaling

### Scale up vs scale out

| | Scale up | Scale out |
|---|----------|-----------|
| Mechanism | More threads/cores on one machine | More machines / nodes |
| Akka.NET | Dispatchers, router pools on local system | Akka.Remote, Akka.Cluster |

### Routers

Distribute messages across routees:

| Strategy | When |
|----------|------|
| **Round-robin** | Even work distribution |
| **Random** | Simple load spread |
| **Smallest mailbox** | Variable-cost messages |
| **Consistent hashing** | Sticky routing by key (e.g. user ID) |
| **Scatter-gather first** | Fan-out; take first successful reply |
| **Tail-chopping** | Fan-out with fallback latency |

**Pools** — router creates/manages routees. **Groups** — router forwards to existing actor paths.

### Cluster (Akka.Cluster)

- **Cluster-aware routers** — route across nodes
- **Cluster singleton** — one actor instance cluster-wide
- **Cluster sharding** — entity actors by shard key; passivation under memory pressure
- **Distributed pub/sub** — topic or point-to-point messaging across cluster

## Composing actor systems

### Remoting (Akka.Remote)

- Link actor systems across machines
- **Remote deployment** — spawn actors on another node
- Configure transport, serialization, and **security** (restrict message types and remote targets)

Prefer **Cluster** over raw remoting for elastic production deployments.

## Integration boundaries

Keep actors **pure**; push IO to the edges:

| Boundary | Pattern |
|----------|---------|
| **ASP.NET Core** | Hosted `ActorSystem`; API sends commands via facade/`IActorRef` |
| **SignalR** | Actor tells hub wrapper; no `IHubContext` inside actor |
| **Database** | Resolve at Props time; avoid scoped `DbContext` in singleton actors — use `IDbContextFactory` or application-layer persistence |
| **Custom protocols** | `akka.io` TCP/UDP listeners; bridge bytes ↔ domain messages |

This repo's `AkkaSignalRVuePoc` follows this: `ISignalrHubWrapper` injected via Props, hub actor bridges to SignalR.

## Persistence and event sourcing

**Akka.Persistence** for actors that must survive restarts:

- **Event sourcing** — persist events; replay to rebuild state
- **Snapshots** — periodic state checkpoints for faster recovery
- **Journals** — pluggable storage backends
- **At-least-once delivery** — reliable messaging across restarts

Use when actor state is **authoritative** and must survive crashes — not for every actor.

## Testing

| Level | Tooling | Focus |
|-------|---------|-------|
| **Unit** | Akka.TestKit | Single actor behavior, FSM transitions, internal state via probes |
| **Integration** | TestKit + test actor system | Multi-actor flows, message sequences, time-based tests |
| **Distributed** | MultiNode TestKit | Cluster/remoting, network partitions, barriers |

**Practices:**

- Use **TestProbe** to stand in for collaborators
- Control time with `Within`, `AwaitCondition`, or virtual time schedulers
- **GracefulStop** actors between tests
- Assert on **messages sent/received**, not internal fields when possible

This repo uses `Akka.Hosting.TestKit` — see `ActorTestBase<T>` in `ApiImportActorPoc` tests.

## Reactive application design workflow

Brown's e-commerce case study pattern (generalized):

1. **Identify bounded capabilities** — cart, catalog, payment, shipping as separate actor areas
2. **Assign state** — one actor (or shard) per shopping cart, order, session
3. **Define messages** — commands in imperative form; events in past tense
4. **Plan failure domains** — which actors restart together under one supervisor
5. **Plan scale paths** — routers for throughput; cluster sharding for entity count
6. **Integrate at edges** — HTTP/SignalR in; persistence at actors that own state
7. **Test by scenario** — case-study chapters map to integration tests

## Practical workflow for agents

When designing or reviewing Akka.NET code:

1. **Classify the problem** — concurrency, resilience, distribution, or integration?
2. **Check fit** — would `async`/`await` + DI suffice? If yes, don't add actors.
3. **Draw actor boundaries** — one mailbox per isolation unit; no shared mutable state
4. **Define messages** — immutable, small, domain-named
5. **Place supervision** — parent owns children's failure policy
6. **Push IO out** — actors orchestrate; wrappers handle SignalR, HTTP, DB
7. **Choose scale mechanism** — pool router, cluster shard, or singleton
8. **Plan tests** — TestKit scenarios for happy path, failure, and timeout

## Red flags (challenge the design)

- Blocking or `.Result`/`.Wait()` inside `Receive`
- Shared static mutable state between actors
- `IServiceProvider` injected into actors instead of resolved dependencies in Props
- One giant actor owning unrelated concerns
- `Ask` without timeout handling
- New `ActorSystem` per HTTP request
- Direct ASP.NET/EF types inside actor classes
- Supervision that restarts without addressing root cause (restart loops)
- Using actors where a simple background service would do

## C# mapping cheatsheet

| Concept | Typical expression |
|---------|-------------------|
| Actor | `sealed class OrderActor : ReceiveActor` |
| Message | `record PlaceOrder(OrderId Id, …)` |
| Props | `public static Props Props(…) => Akka.Actor.Props.Create(() => new OrderActor(…));` |
| Tell | `_orderActor.Tell(new PlaceOrder(…));` |
| Ask | `await _orderActor.Ask<OrderPlaced>(cmd, timeout);` |
| Supervision | `SupervisorStrategy` on parent or `OneForOneStrategy` in `SupervisorStrategy` override |
| FSM | `class CartFsm : FSM<CartState, CartData>` |
| Router | `FromConfig.Instance` or `RouterPool` in HOCON / `Props.WithRouter` |
| Persistence | `class OrderActor : ReceivePersistentActor` |
| Test | `Sys.ActorOf(…); probe.ExpectMsg<T>(…);` |

## Related skills and references

- **`akka-net`** — repo-specific Akka.Hosting, contracts layout, `AkkaSignalRVuePoc` patterns
- **`dotnet-core-csharp-development`** — ASP.NET Core host, DI, testing
- **`dotnet-ef-core`** — DbContext scoping rules for actor boundaries
- **`domain-driven-design`** — actors orchestrate; domain holds business rules
- Anthony Brown — *Reactive Applications with Akka .NET* (Manning, 2019) — this skill's source
- Official docs: https://getakka.net/
