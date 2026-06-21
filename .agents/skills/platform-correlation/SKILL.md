---
name: platform-correlation
description: |
  End-to-end correlation tracing (CorrelationId, UseCase) across HTTP, Akka actors, domain events, and SignalR.
  Use when:
  - Tracing a use case through facades, actors, events, or SignalR
  - Wiring AskCorrelated/TellCorrelated or CorrelatedMessageEnvelope
  - Fixing Ask timeouts or lost Sender in async actors
  - Adding correlation fields to IDataEvent or notification DTOs
paths:
  - "Platform.Serilog.Logging/**"
  - "build/Platform.Logging.Akka.props"
  - "**/*Actor*.cs"
  - "**/*Facade*.cs"
  - "**/Program.cs"
  - "docs/monolith-modularization/platform-correlation-standard.md"
metadata:
  version: 1.0.0
---

# Platform correlation

**Authoritative doc:** `docs/monolith-modularization/platform-correlation-standard.md`

**Goal:** One `CorrelationId` per use-case flow, plus a semantic `UseCase` name — queryable in Seq as `CorrelationId = '…' and UseCase = 'Import.Start'`.

## Concepts

| Field | Set at | Example |
|-------|--------|---------|
| `CorrelationId` | HTTP middleware or `CorrelationFlow.FromCurrentOrNew` | GUID / trace id |
| `UseCase` | Facade or `X-Use-Case` header | `Catalog.CreateProject` |
| `CausationId` | Child messages (optional) | step id |

All fields flow into Serilog via `CorrelationLogEnricher` + `CorrelationScope`.

## HTTP

```csharp
app.UsePlatformCorrelationPipeline(); // before
app.UsePlatformRequestLogging();
```

Headers: `X-Correlation-Id`, `X-Use-Case`, `X-Causation-Id` (response echoes correlation id).

## Akka boundary (facade)

```csharp
await _rootActor.AskCorrelated<CreateProjectResult>(
    command,
    "Catalog.CreateProject",
    timeout,
    cancellationToken);

_rootActor.TellCorrelated(new StartBackgroundProcessCommand(), "Background.Start");
```

Import `build/Platform.Logging.Akka.props` on API + Core actor projects.

## Actor patterns

### Sync routing (forward / tell)

Use `PlatformReceiveActor` + `ReceiveCorrelated`:

```csharp
ReceiveCorrelated<GetAllProjectsQuery>((query, flow) =>
    _projectData.Forward(flow.Wrap(query)));
```

After `Become`, call `RegisterEnvelopeHandler()` when returning to a state that receives envelopes.

### Async EF / IO (critical)

**Do not** rely on `Sender` after `await` inside nested `PlatformReceiveActor` async dispatch.

Capture `Sender` in the **same** `ReceiveAsync<CorrelatedMessageEnvelope>` delegate:

```csharp
ReceiveAsync<CorrelatedMessageEnvelope>(async envelope =>
{
    var sender = Sender;
    var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
    using CorrelationScope scope = flow.BeginScope();

    if (envelope.Message is CreateProjectCommand command)
    {
        // await db work…
        sender.Tell(result);
    }
});
```

Reference: `OrganisationDataActor`, `ProjectDataActor`, `DataManagerActor` in `AkkaSignalRVuePoc.Core`.

### Mixed hub actors

`SignalRHubActor` accepts **both** plain messages (timer/heartbeat) and `CorrelatedMessageEnvelope` (HTTP-driven flows).

## Domain events + SignalR

Publish with correlation from `CorrelationFlow`:

```csharp
Context.System.EventStream.Publish(new ProjectCreated(
    project, occurredAt, flow.CorrelationId, flow.UseCase));
```

Include `CorrelationId` / `UseCase` on `IDataEvent` implementations and SignalR notification DTOs.

## Use-case naming

Dotted verbs: `Import.Start`, `Import.Persist`, `Hours.Book`, `Catalog.CreateProject`, `Background.Start`, `LiveMessage.Publish`.

Set at the **system boundary** (HTTP facade); child steps may use `flow.WrapChild(message, "Import.PersistData")`.

## Tests

Use `AskCorrelated` in actor tests (`ActorTestCorrelation.AskAsync` in Akka POC tests).

## Troubleshooting

| Symptom | Likely cause | Fix |
|---------|--------------|-----|
| Ask timeout after `await` in data actor | Lost `Sender` | Inline envelope dispatch; capture `sender` before await |
| Second mutation on same manager hangs | `Become` dropped envelope handler | `RegisterEnvelopeHandler()` when returning to ready |
| SignalR never receives push | Hub only handles envelopes | Also `ReceiveAsync<PlainMessage>` for timer actors |
| Logs missing correlation | Scope not opened | `using CorrelationScope scope = flow.BeginScope()` |

## Checklist (new feature)

- [ ] `UsePlatformCorrelationPipeline()` in host
- [ ] Facade uses `AskCorrelated` / `TellCorrelated` with explicit use-case name
- [ ] Async persistence actors use inline envelope dispatch
- [ ] Events and SignalR DTOs carry correlation fields
- [ ] Seq query verified for happy path
