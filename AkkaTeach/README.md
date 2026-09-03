# AkkaTeach

A small teaching application that demonstrates core Akka.NET concepts:

- **Actor communication** — coordinators, child actors, and request/reply
- **Become / Unbecome** — a session actor switches between Idle, Active, and Completed states
- **API client + worker pool** — paginated mock API collection processed by a `RoundRobinPool` of workers
- **Event stream** — domain events published for cross-actor observation
- **Hosting** — `Akka.Hosting` registers actors in DI; a background worker drives demo traffic
- **Logging** — Serilog for the host; Akka's built-in logger adapter inside actors

## Projects

| Project | Purpose |
|---------|---------|
| `AkkaTeach.Contracts` | Messages, queries, and events |
| `AkkaTeach.Core` | Actors + `IDataApiClient` port |
| `AkkaTeach.Worker` | Host, `MockDataApiClient`, background worker |
| `AkkaTeach.Tests` | TestKit unit tests |

## Run the worker

```bash
cd AkkaTeach
dotnet run --project src/AkkaTeach.Worker
```

The background worker triggers a full data-ingestion cycle every 30 seconds:

1. `DataIngestionActor` fetches paginated pages from `MockDataApiClient`
2. Each record is routed to a **worker pool** (`RoundRobinPool`)
3. Progress and completion events are published on the event stream

Default dev config: **5 pages × 20 records = 100 items** with a pool of **4 workers**.

Tune volume in `appsettings.json`:

```json
"DataIngestion": {
  "WorkerPoolSize": 4,
  "PageSize": 50,
  "TotalPages": 10,
  "FetchDelayMilliseconds": 100
}
```

That yields **500 records** per cycle — useful for seeing concurrent worker processing in the logs.

## Run tests

xUnit v3 runs the test project as an executable:

```bash
cd AkkaTeach
dotnet run --project tests/AkkaTeach.Tests
```

## Actor model guide

See **[docs/actor-model-guide.md](docs/actor-model-guide.md)** for a topic-by-topic walkthrough of the actor model with code samples and best practices from this repo.

## Key actors

### `PipeToDemoActor`

A **minimal, focused** example of `PipeTo`. Read the XML comments on the class first.

1. `FetchQuoteCommand` starts `IQuoteService.FetchQuoteAsync`
2. `PipeTo(Self, ...)` maps the completed `Task` to mailbox messages
3. While waiting, `GetFetchStatusQuery` still returns `"Fetching"` — proof the actor never blocked

See `PipeToDemoActorTests.FetchQuote_UsesPipeTo_MailboxStaysResponsiveWhileWaiting`.

### `AddressingDemoActor` + `GreeterActor`

A **minimal, focused** example of addressing another actor. Read `AddressingDemoActor` first.

| Pattern | Code | When |
|---------|------|------|
| Direct | `greeter.Tell(msg, probe)` | You hold the `IActorRef` |
| Tell + sender | `greeter.Tell(msg, Sender)` | Middleman; reply goes to original caller |
| Forward | `greeter.Forward(msg)` | Same as above, sender preserved automatically |
| Child | `Context.ActorOf(...)` → store `IActorRef` | Greeter lives under the front desk |

See `AddressingDemoActorTests`.

### `PeerActor` + `PeerIntroducerActor`

Demonstrates **any actor can message any other actor** — no parent/child relationship required.

1. Three peer actors are created as **siblings** under `/user`
2. `PeerIntroducerActor` gives each one every other peer's `IActorRef` (address book)
3. Alice `Tell`s Bob directly; Bob `Tell`s Carol — no coordinator in the path

```
/user/alice  ──Tell──►  /user/bob
/user/bob    ──Tell──►  /user/carol
```

See `PeerActorTests.AnyPeer_CanMessageAnyOtherPeer_DirectlyViaIActorRef`.

### `DataIngestionActor`

The API-client showcase. Depends on `IDataApiClient` (injected via DI — swap `MockDataApiClient` for `HttpClient` in production). Fetches pages asynchronously with `PipeTo`, fans records out to a router-backed pool, and uses `Become` to track each fetch/process cycle.

### `DataRecordWorkerActor`

Pool worker that processes one `ExternalDataRecord` at a time. Multiple instances run behind a `RoundRobinPool` router.

### `WorkCoordinatorActor`

Demonstrates parent/child communication: the coordinator sends work to a child with `Self` as sender, uses `Become` while waiting for the reply, then forwards the result to the original caller.

### `SessionActor`

Demonstrates behavior switching: **Idle → Active → Completed → Idle** (via `ResetSessionCommand`).

## Swapping in a real API client

1. Implement `IDataApiClient` with `HttpClient` (pagination, auth, retries).
2. Register it in DI instead of `MockDataApiClient`.
3. Actors stay unchanged — only the IO boundary swaps.

## Packages

Uses Akka.NET **1.5.71**:

- `Akka`, `Akka.Hosting`, `Akka.Logger.Serilog`
- `Akka.Hosting.TestKit`, `xunit.v3`
