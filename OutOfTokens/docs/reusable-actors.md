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

## Planned (P1+)

| Actor | Role | Motivation |
|-------|------|------------|
| **ExclusiveGateActor** | At-most-one in-flight operation | Sync/import mutex (`P6Actor._syncRunning`) |
| **KeyedAccumulatorActor&lt;TKey, TItem&gt;** | In-memory keyed aggregation | `P6RawDataStoreActor` pattern |
| **EventStreamBridgeActor** | Subscribe + side-effect | Progress logging, SignalR push |
| **BatchOrchestratorActor&lt;TItem&gt;** | Bounded-concurrency batch | `P6SyncOrchestratorActor` outer loop |
| **PipeWorkerActor&lt;TIn, TOut&gt;** | One-shot PipeTo worker | `P6LoginActor` shape |

---

## Design rules

- **No `Ask` inside actors** — `Tell`, `Forward`, `PipeTo`, `Become` only ([platform-actor-standard.md](../../docs/monolith-modularization/platform-actor-standard.md))
- **Inject behavior via `Props`** — `Func` delegates and `ISessionGateBehavior`, not domain messages in generic actors
- **Compose, don't inherit** — spawn `PagedFetchActor` inside `SessionGateActor` dispatch; keep domain actors thin
