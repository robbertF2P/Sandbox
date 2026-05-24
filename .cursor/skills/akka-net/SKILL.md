---
name: akka-net
description: |
  Guides Akka.NET actor development: ReceiveActor, Props, messaging, timers, hosting in ASP.NET Core,
  and TestKit tests. Use when:
  - Creating or modifying actors, messages, or events
  - Wiring actor systems via Akka.Hosting or IHostedService
  - Writing actor tests with Akka.TestKit
  - Debugging mailbox behavior, supervision, or actor lifecycle
  - Integrating actors with SignalR or other IO boundaries
paths:
  - "AkkaSignalRVuePoc/**/*.cs"
  - "**/*Akka*.cs"
  - "**/*Actor*.cs"
metadata:
  version: 1.0.0
---

# Akka.NET Development

Apply this skill for Akka.NET work. The primary reference implementation in this repo is `AkkaSignalRVuePoc`.

**Core principle:** Keep actor logic pure; push IO (SignalR, HTTP, databases) behind abstractions injected via `Props`.

## Solution layout (this repo)

| Project | Responsibility |
|---------|----------------|
| `AkkaSignalRVuePoc.Contracts` | Messages (`IActorSystemMessage`), events (`IActorSystemEvent`), commands |
| `AkkaSignalRVuePoc.Core` | Actor implementations and publisher abstractions |
| `AkkaSignalRVuePoc.Api` | ASP.NET host, `AkkaHostingExtensions`, facades, SignalR hub |

Workspace rule `.cursor/rules/actor-system-contracts.mdc` is authoritative for contracts:

- Actor implementations live in **Core**.
- Messages and events live in **Contracts**.
- Events are C# `record` types named in past tense PascalCase (e.g. `ActorSystemStarted`, `ProcessFinished`).
- Commands implement `IActorSystemMessage`.

## Actor implementation checklist

1. **Class shape:** `sealed class MyActor : ReceiveActor` (add `IWithTimers` when using `Timers`).
2. **Logging:** `private readonly ILoggingAdapter _log = Context.GetLogger();`
3. **State:** Instance fields only; the mailbox serializes access — do **not** use `Interlocked` for per-actor state.
4. **Handlers:** Configure in the constructor with `Receive<T>(handler)`.
5. **Props factory:** Static `Props` method on the actor class; use `Akka.Actor.Props.Create(() => new MyActor(...))` when the actor also exposes a static `Props` name (qualify to avoid ambiguity).
6. **Timers:** Implement `IWithTimers`, use `Timers.StartPeriodicTimer` in `PreStart`, `Timers.Cancel` in `PostStop`.
7. **Child actors:** Create via `Context.ActorOf` or receive `IActorRef` from parent; prefer telling refs over looking up by path in app code.

### Example Props pattern

```csharp
public static Props Props(IActorRef hubPush, TimeSpan? interval = null)
{
    return Akka.Actor.Props.Create(() => new FrontendPushActor(hubPush, interval ?? DefaultInterval));
}
```

## Hosting in ASP.NET Core

Registration is centralized in `AkkaHostingExtensions.AddAkkaActors`:

- `services.AddAkka<AkkaActorHostedService>(...)` with `WithActors` callback.
- Named top-level actors (e.g. `signalr-hub-push`, `live-message-root`, `frontend-push`).
- `registry.Register<LiveMessageRootActor>(rootActor)` for typed resolution.
- `IActorSystemCommandFacade` wraps the root actor for API commands.

When adding a new top-level actor:

1. Implement the actor in **Core** with a `Props` factory.
2. Add messages/commands in **Contracts** if needed.
3. Register in `AkkaHostingExtensions.WithActors` with a stable actor name.
4. Expose access via facade or `IRequiredActor<T>` only if the API layer needs it.

Configuration example: `BackgroundProcess` section maps to `BackgroundProcessTiming` via `IConfiguration`.

## Messaging conventions

| Kind | Type | Example |
|------|------|---------|
| Command | class/record implementing `IActorSystemMessage` | `PublishLiveMessageCommand` |
| Domain message | POCO or record | `PushMessage`, `PushTick` |
| Event (pub/sub) | record implementing `IActorSystemEvent` | `ActorSystemStarted` |

- Tell with `actor.Tell(message)`; ask only when you need a reply and handle timeouts.
- Subscribe to the event stream with `Context.System.EventStream.Subscribe(Self, typeof(TEvent))`.
- Publish events with `Context.System.EventStream.Publish(evt)`.

## IO boundary (SignalR)

- `SignalRHubActor` receives `PushMessage` and calls `ISignalrHubWrapper.PublishActorMessageAsync`.
- `SignalRLiveMessageClientPublisher` implements the wrapper using `IHubContext<LiveMessagesHub>`.
- Actors must not reference ASP.NET types directly; depend on `ISignalrHubWrapper` or `IActorRef` only.

## Testing with TestKit

Actor tests inherit `ActorTestBase<TTest>` (`Akka.Hosting.TestKit`):

- Provides `RecordingHubContext`, `CreateHubPushActor()`, and Serilog test logging.
- Override `ConfigureServices` / `ConfigureAkka` only when the test needs extra setup.
- Use `Sys.ActorOf(SomeActor.Props(...))` to spawn actors under test.
- Stop actors with `GracefulStop` when verifying shutdown or avoiding test pollution.
- Assert SignalR side effects via `HubContext.RecordedCalls` and `GetPushMessage`.

Run tests:

```bash
cd AkkaSignalRVuePoc
dotnet test AkkaSignalRVuePoc.slnx --filter "FullyQualifiedName~Actors"
```

## Supervision and failure

- Prefer letting parent actors supervise children defined in the same feature area.
- Log failures with `_log.Error(ex, "context")`; avoid swallowing exceptions without logging.
- For POC/demo actors, keep supervision defaults unless requirements specify restarts or backoff.

## Anti-patterns

- Blocking inside `Receive` handlers (`Thread.Sleep`, `.Result`, `.Wait()`).
- Sharing mutable static state between actors.
- Passing `IServiceProvider` into actors instead of resolving dependencies at `Props` creation time.
- Sending large object graphs; prefer immutable records with IDs.
- Creating a new `ActorSystem` per request — use the hosted singleton system.

## Related skills and rules

- `dotnet-core-csharp-development` — ASP.NET Core host, DI, and `dotnet` CLI.
- `.cursor/rules/actor-system-contracts.mdc` — message/event placement and naming.
- `AkkaSignalRVuePoc/.cursor/rules/csharp-resharper-style.mdc` — C# style including Akka logging and test base usage.

Official docs: https://getakka.net/ (API and concepts). Prefer patterns already used in `AkkaSignalRVuePoc.Core.Actors` over introducing new frameworks.
