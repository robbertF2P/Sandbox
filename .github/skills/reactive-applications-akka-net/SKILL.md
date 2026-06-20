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
  version: 1.1.0
  source: "Anthony Brown — Reactive Applications with Akka .NET (Manning, 2019, ISBN 9781617292989)"
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

**Brown's distinction:** reactive programming (e.g. Rx) is **event-driven** — data broadcast to all listeners. Reactive systems are **message-driven** — individually addressable components receive **targeted** messages. Akka.NET is the latter.

## Reactive Manifesto (design lens)

Brown frames the Manifesto as the outcome of patterns seen across IoT, e-commerce, finance, and petabyte-scale analytics. **Responsiveness is the most important property** — timely behavior under normal and degraded conditions. The other traits exist to preserve it.

| Trait | Meaning in practice (Brown) |
|-------|------------------------------|
| **Responsive** | Timely under load and failure; definition varies by domain (HTTP latency vs streaming throughput) |
| **Resilient** | Failures **contained** to the smallest area; automatic recovery without burdening the client |
| **Elastic** | Expand under load, **shrink** during inactivity — avoid paying for idle capacity |
| **Message-driven** | Async, non-blocking communication; reroute at runtime when components fail or bottleneck |

**How they connect:** messaging powers resilience, elasticity, and responsiveness. Resilience infrastructure also enables elasticity — they are shared concerns, not independent add-ons.

**Red flag:** shared mutable state, synchronous RPC chains, or one global lock for concurrency.

## When to use Akka.NET

**Good fits (ch. 1–2):**

- Integrating **many components** that must react immediately to each other's results (e.g. airport gate displays fed by ATC, runway ops, airline scheduling)
- High concurrency with **isolated units of work** (sessions, carts, orders, devices, imports)
- **Stateful** processing where one mailbox serializes access per unit
- **Failure domains** that should restart independently (supervision trees)
- **Distributed** or clustered workloads (routers, remoting, cluster sharding)
- Long-running background processes with timers and lifecycle
- E-commerce-style flows: cart actor → checkout → payment gateway → shipping, with routers for throughput and pub/sub for downstream reactions (pricing, recommendations)

**Poor fits (ch. 1):**

- Simple CRUD backed by a basic database — Akka.NET surfaces distributed-system complexity (partial failure, consistency, harder debugging) without benefit
- Systems with **no real concurrency** need — adds mental overhead for data races and deadlocks without simplifying anything
- Request/response-only APIs with no stateful or streaming behavior
- Teams unwilling to adopt message-passing mental model
- Replacing a well-working `async`/`await` pipeline with actors "for consistency"

Akka.NET is a **platform for reactive systems** across IoT, e-commerce, and finance — fit depends on integration complexity and concurrency needs, not domain label alone.

## Actor model fundamentals

Brown condenses the actor into three concepts — **communication**, **processing**, and **state** — forming a sealed boundary (ch. 3):

| Concept | Role |
|---------|------|
| **Communication** | Unique address + mailbox; async message passing (like SMS to a phone number) |
| **Processing** | Behavior invoked per message; at most one message processed at a time per actor |
| **State** | Private memory; no external direct access — ask via messages only |

An **actor** embodies:

- **Identity** — addressable via `IActorRef` (not the instance)
- **State** — private; only the actor's mailbox thread mutates it
- **Behavior** — how incoming messages are handled
- **Mailbox** — FIFO queue; one message processed at a time per actor

**Immutability is required** for messages — mutable messages break concurrency safety guarantees.

An actor can:

- **Send** messages (`Tell`, `Ask`)
- **Create** child actors (`Context.ActorOf`) — delegate like assigning work to another person
- **Change behavior** (`Become` / `Unbecome`, or FSM)
- **Supervise** children (restart/stop/resume on failure)
- **Ignore** a message (sender may retry if important)

### `IActorRef` vs `ActorSelection` (ch. 3, 6)

| | `IActorRef` | `ActorSelection` |
|---|-------------|------------------|
| Points to | Specific actor instance | Path that resolves to current instance |
| On restart | Reference may change | Still resolves to restarted actor |
| Use when | Normal tell/ask to known actor | At-least-once delivery, remote targets that may restart |

For guaranteed delivery across failures, prefer `ActorSelection` over `IActorRef` when the target may be recreated.

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

### Props, HOCON, and deployment

- **`Props`** — recipe for creating an actor (type + constructor args + dispatcher/router config). Use `system.DI().Props<T>()` when integrating Autofac/Ninject/Windsor adapters.
- **Actor system** — root container; hosts guardians, dispatchers, event stream. One per application.
- **HOCON** (Human Optimized Config Object Notation) — Akka.NET's config format (JSON superset with comments, `key = value`, dotted paths, time units like `120 s` / `2 m`). Load via `ConfigurationFactory.ParseString` or embedded `App.config` CDATA. Only override what differs from defaults.

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
| **Restart** | New actor instance; **state lost** unless persisted |
| **Stop** | Terminate child permanently |
| **Resume** | Continue with same instance |
| **Escalate** | Delegate failure to own supervisor |

Design supervision to match **failure domains** — restart the smallest unit that can recover cleanly. A **strong actor hierarchy** (ch. 6) limits blast radius so one conversation's failure does not take down unrelated work.

### Error kernel pattern (ch. 6)

Push **dangerous or stateless work** to child actors. Keep long-lived state in the parent. If the child fails and restarts, the parent's accumulated state survives. Without this, frequent restarts wipe in-memory state.

### Actor lifecycle (ch. 6)

States: Starting → Running → Stopping → Terminated. Override `PreStart`, `PreRestart`, `PostRestart` for lifecycle hooks.

| Shutdown | Behavior |
|----------|----------|
| **`PoisonPill`** | Process all queued messages first, then stop — graceful drain |
| **`Context.Stop(ref)`** | Stop after current message — immediate |

Messages to **terminated** actors go to the **dead letters** mailbox (logged, not delivered).

**`PreRestart`:** receive the failing message and `Self.Tell(message)` to retry after restart.

### DeathWatch and reaper pattern (ch. 6)

- **`Context.Watch(ref)`** + `Receive<Terminated>` — react when another actor dies
- **Reaper pattern** — a coordinator watches N workers; when all have terminated (often via `PoisonPill` after work), proceed past a barrier (e.g. shutdown cluster node)

### Application-level failures

- **Fail fast** inside actors; let supervision handle recovery.
- Distinguish **interface-level** failures (bad input — may be unrecoverable, stop not restart) from **infrastructure** failures (network, DB).
- Supervision reduces over-dependence on external services — failure handling is core to Akka.NET.

### Transport-level failures

In distributed systems:

- Messages may be **lost** or **duplicated** — design idempotent handlers where needed.
- **At-least-once delivery** actors use a simple FSM: awaiting acknowledgment ↔ successfully delivered (ch. 6).
- Use **`ActorSelection`** (not `IActorRef`) when target may restart across network failures.
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

### Cluster (Akka.Cluster) — ch. 12

**Glossary:**

| Term | Role |
|------|------|
| **Node** | One actor system instance in the cluster |
| **Seed node** | Well-known join address for new nodes |
| **Gossip protocol** | Propagates membership changes to all nodes |
| **Leader** | Deterministically chosen node that applies membership changes |
| **Failure detector** | Heartbeat-based unreachability detection |
| **Role** | Tags nodes for workload affinity (e.g. `network` vs `compute`) |

**Benefits:** elastic scale-out, fault tolerance (redeploy actors from failed nodes), peer-to-peer (no coordinator bottleneck or single point of failure).

**Cluster-aware routers** — same router types as ch. 7, but react to gossip (add/remove routees as nodes join/leave). Prefer over raw Akka.Remote router paths that create single-node failure points.

**Cluster sharding** — partition entities by shard key across nodes. **Partition at a locality boundary** (e.g. per-home in IoT) so related child actors stay on the same machine for fast communication.

**Cluster singleton** — one actor instance cluster-wide (with failover).

**Distributed pub/sub** — topic or point-to-point messaging; react to cluster events immediately.

**Cluster client** — lightweight client talking to cluster without hosting actors.

**Config essentials:** `actor.provider = ClusterActorRefProvider`, remote port/hostname, `seed-nodes`, optional `cluster.roles`. All nodes must share the **same actor system name**.

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

## Persistence and event sourcing (ch. 11)

**Akka.Persistence** for actors that must survive restarts:

- **Event sourcing** — persist events; replay to rebuild state (not every reading — persist behavior-changing events)
- **Snapshots** — periodic state checkpoints for faster recovery
- **Journals** — pluggable storage backends; async write journals for throughput
- **At-least-once delivery** — reliable messaging across restarts
- **Upgrade strategies** — evolve event-sourced schemas without breaking replay

Use when actor state is **authoritative** and must survive crashes. For cluster sharding, Brown recommends persistent actors when entities may move between nodes.

**Anti-pattern:** active record pattern inside actors — persistence belongs in the event/journal model, not ad-hoc DB writes scattered in handlers.

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

Brown's e-commerce case study (ch. 2) and production IoT case study (ch. 13) share this progression:

1. **Identify bounded capabilities** — cart, catalog, payment, shipping (or home → room → sensor hierarchy)
2. **Assign state** — one actor (or shard) per cart, order, session, or home
3. **Define messages** — commands in imperative form; events in past tense; immutable types
4. **Model lifecycle** — FSM or `Become` for cart/checkout states
5. **Plan failure domains** — error kernel: stateful parent, risky children; supervision per area
6. **Scale throughput** — routers (pools for compute, consistent hash for sticky sessions)
7. **Expose to clients** — Akka.Remote proxy or ASP.NET/SignalR at the edge (ch. 8, 10)
8. **Persist authoritative state** — Akka.Persistence for carts/orders that must survive restart
9. **Scale out** — Akka.Cluster with sharding at a locality boundary; autoscaled nodes
10. **React to changes** — distributed pub/sub for downstream systems (recommendations, pricing)
11. **Test by scenario** — unit (behavior), integration (flows), multinode (partitions)

### Production checklist (ch. 13)

| Concern | Book guidance |
|---------|---------------|
| Actor design | Independent actors, no direct dependencies — place anywhere in cluster |
| Failure | Supervision + persistence; partition at entity boundary |
| Scale | Cluster sharding; add nodes on demand |
| Configuration | HOCON per environment; roles per hardware profile |
| Data ingestion | Dedicated ingest actors; don't block mailbox on IO |
| Real-time UI | SignalR bridge actor (ch. 10) |
| Testing | TestKit + MultiNode for distributed behavior |

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
- **Mutable messages** — breaks actor concurrency guarantees
- Shared static mutable state between actors
- `IServiceProvider` injected into actors instead of resolved dependencies in Props
- One giant actor owning unrelated concerns
- `Ask` without timeout handling
- New `ActorSystem` per HTTP request
- Direct ASP.NET/EF types inside actor classes
- Supervision that restarts without addressing root cause (restart loops)
- Restarting **stateful parent** actors instead of using error kernel pattern
- Using `IActorRef` for at-least-once delivery when target may restart
- Using actors where a simple background service would do
- Basic CRUD with Akka.NET "for scalability" — debugging and consistency cost with no concurrency win

## C# mapping cheatsheet

| Concept | Typical expression |
|---------|-------------------|
| Actor | `sealed class OrderActor : ReceiveActor` |
| Message | `record PlaceOrder(OrderId Id, …)` |
| Props | `public static Props Props(…) => Akka.Actor.Props.Create(() => new OrderActor(…));` |
| Tell | `_orderActor.Tell(new PlaceOrder(…));` |
| Ask | `await _orderActor.Ask<OrderPlaced>(cmd, timeout);` |
| Supervision | `SupervisorStrategy` on parent or `OneForOneStrategy` in `SupervisorStrategy` override |
| Shutdown | `actor.Tell(PoisonPill.Instance)` or `Context.Stop(ref)` |
| Watch | `Context.Watch(ref); Receive<Terminated>(…)` |
| Reaper | Coordinator actor watches workers until all `Terminated` |
| FSM | `class CartFsm : FSM<CartState, CartData>` |
| Router | `FromConfig.Instance` or `RouterPool` in HOCON / `Props.WithRouter` |
| Persistence | `class OrderActor : ReceivePersistentActor` |
| Test | `Sys.ActorOf(…); probe.ExpectMsg<T>(…);` |

## Related skills and references

- **`akka-net`** — repo-specific Akka.Hosting, contracts layout, `AkkaSignalRVuePoc` patterns
- **`dotnet-core-csharp-development`** — ASP.NET Core host, DI, testing
- **`dotnet-ef-core`** — DbContext scoping rules for actor boundaries
- **`domain-driven-design`** — actors orchestrate; domain holds business rules
- Anthony Brown — *Reactive Applications with Akka .NET* (Manning, 2019, ISBN 9781617292989) — this skill's source; 13 chapters, sample code at [manning.com/books/reactive-applications-with-akka-net](https://www.manning.com/books/reactive-applications-with-akka-net)
- Official docs: https://getakka.net/
