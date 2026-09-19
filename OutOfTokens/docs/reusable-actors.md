# Reusable actor catalog

Generic actors in `Infrastructure.Akka` — message-agnostic building blocks for integration connectors and workflow hosts.

**Visual overview:** [actor-overview.md](actor-overview.md)

---

## Implemented (P0)

| Actor | Package path | When to use |
|-------|--------------|-------------|
| **ReplyTargetWorkerActor** | [Actors/Workers/ReplyTargetWorkerActor.cs](../Infrastructure.Akka/Actors/Workers/ReplyTargetWorkerActor.cs) | Ephemeral worker that replies once then stops |
| **PagedFetchActor&lt;TItem&gt;** | [Actors/Workers/PagedFetchActor.cs](../Infrastructure.Akka/Actors/Workers/PagedFetchActor.cs) | Remote API returns pages; accumulate until done |
| **PagedFetchOptions&lt;TItem&gt;** | [Actors/Workers/PagedFetchOptions.cs](../Infrastructure.Akka/Actors/Workers/PagedFetchOptions.cs) | Injected fetch/merge/reply delegates |
| **SessionGateActor** | [Actors/Session/SessionGateActor.cs](../Infrastructure.Akka/Actors/Session/SessionGateActor.cs) | Authenticated session with login-on-demand |
| **ISessionGateBehavior** | [Actors/Session/ISessionGateBehavior.cs](../Infrastructure.Akka/Actors/Session/ISessionGateBehavior.cs) | Connector-specific login, dispatch, logout |
| **ISessionGateHost** | [Actors/Session/ISessionGateHost.cs](../Infrastructure.Akka/Actors/Session/ISessionGateHost.cs) | Host surface for behavior registration |

### Minimal usage

```csharp
// Paged fetch
Context.ActorOf(PagedFetchActor<MyDto>.Props(options))
    .Tell(new PagedFetchActor<MyDto>.Start(replyTo));

// Session gate
Context.ActorOf(SessionGateActor.Props(new MyConnectorSessionBehavior(...)));
```

### Reference implementation

P6 connector composes all P0 actors — see [P6SessionGateBehavior.cs](../Floor2Plan.Connectors.P6/Actors/P6SessionGateBehavior.cs) and [P6ProjectCatalogActor.cs](../Floor2Plan.Connectors.P6/Actors/P6ProjectCatalogActor.cs).

---

## Implemented (P1)

| Actor | Package path | When to use |
|-------|--------------|-------------|
| **ExclusiveGateActor** | [Actors/Guards/ExclusiveGateActor.cs](../Infrastructure.Akka/Actors/Guards/ExclusiveGateActor.cs) | At-most-one in-flight operation (sync/import mutex) |
| **KeyedAccumulatorActor&lt;TKey, TValue&gt;** | [Actors/State/KeyedAccumulatorActor.cs](../Infrastructure.Akka/Actors/State/KeyedAccumulatorActor.cs) | In-memory keyed clear / append / query |
| **KeyedAccumulatorState&lt;TKey, TValue&gt;** | [Actors/State/KeyedAccumulatorState.cs](../Infrastructure.Akka/Actors/State/KeyedAccumulatorState.cs) | Shared state helper (domain actors with custom messages) |
| **PagedFetchOptions.OnItemsAdded** | [Actors/Workers/PagedFetchOptions.cs](../Infrastructure.Akka/Actors/Workers/PagedFetchOptions.cs) | Side-effect per page (stream to store while paging) |

### P1 usage

```csharp
// Exclusive gate
_syncGate.Tell(new ExclusiveGateActor.Begin(work, replyTo));
_syncGate.Tell(new ExclusiveGateActor.Finished());

// Keyed accumulator
Context.ActorOf(KeyedAccumulatorActor<MyKey, MyItem>.Props());
```

P6 reference: [P6Actor.cs](../Floor2Plan.Connectors.P6/Actors/P6Actor.cs) (`ExclusiveGateActor`), [P6RawDataStoreActor.cs](../Floor2Plan.Connectors.P6/Actors/P6RawDataStoreActor.cs) (`KeyedAccumulatorState`), [P6CatalogWorkerActor.cs](../Floor2Plan.Connectors.P6/Actors/P6CatalogWorkerActor.cs) (`PagedFetchActor` + `OnItemsAdded`).

---

## Planned (P2+)

| Actor | Role | Motivation |
|-------|------|------------|
| **EventStreamBridgeActor** | Subscribe + side-effect | Progress logging, SignalR push |
| **BatchOrchestratorActor&lt;TItem&gt;** | Bounded-concurrency batch | `P6SyncOrchestratorActor` outer loop |
| **PipeWorkerActor&lt;TIn, TOut&gt;** | One-shot PipeTo worker | `P6LoginActor` shape |

---

## Design rules

- **No `Ask` inside actors** — `Tell`, `Forward`, `PipeTo`, `Become` only ([platform-actor-standard.md](../../docs/monolith-modularization/platform-actor-standard.md))
- **Inject behavior via `Props`** — `Func` delegates and `ISessionGateBehavior`, not domain messages in generic actors
- **Compose, don't inherit** — spawn `PagedFetchActor` inside `SessionGateActor` dispatch; keep domain actors thin
