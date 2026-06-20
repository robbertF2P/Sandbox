# Platform correlation standard

End-to-end tracing for use-case flows across HTTP, Akka actors, domain events, and SignalR.

## Concepts

| Field | Where set | Example |
|-------|-----------|---------|
| `CorrelationId` | HTTP middleware (or generated) | `a1b2c3...` |
| `UseCase` | Facade / `X-Use-Case` header | `Import.Start` |
| `CausationId` | Child actor messages (optional) | parent step ID |
| `TraceId` / `SpanId` | `Activity.Current` when OpenTelemetry is enabled | W3C trace |

All fields are pushed to Serilog via `LogContext` and appear on every log line in Seq / Application Insights.

## HTTP adoption

1. Import `build/Platform.Logging.Host.props` (includes `Platform.Serilog.Logging`).
2. Wire pipeline **before** request logging:

```csharp
var app = builder.Build();
app.UsePlatformCorrelationPipeline();
app.UsePlatformRequestLogging();
```

3. Clients may send `X-Correlation-Id` and `X-Use-Case`; the response echoes `X-Correlation-Id`.

## Akka adoption

1. Import `build/Platform.Logging.Akka.props` on API and Core actor projects.
2. At the **system boundary** (facade), use correlated Ask/Tell:

```csharp
await _rootActor.AskCorrelated<StartImportResult>(
    new StartImportCommand(payload),
    "Import.Start",
    timeout,
    cancellationToken);
```

3. In actors, receive via envelope unwrapping:

```csharp
ReceiveCorrelated<StartImportCommand>((command, flow) =>
{
    _child.Forward(flow.Wrap(command));
});
```

4. **Async EF handlers** — capture `Sender` in the same `ReceiveAsync<CorrelatedMessageEnvelope>` delegate (do not rely on nested `PlatformReceiveActor` async dispatch):

```csharp
ReceiveAsync<CorrelatedMessageEnvelope>(async envelope =>
{
    var sender = Sender;
    var flow = new CorrelationFlow(envelope.CorrelationId, envelope.UseCase, envelope.CausationId);
    using CorrelationScope scope = flow.BeginScope();
    // await persistence, then sender.Tell(result);
});
```

`PlatformReceiveActor` remains suitable for synchronous routing actors (forward/tell). Timer-driven actors may omit correlation (null is acceptable).

5. Publish domain events with correlation from `CorrelationFlow`:

```csharp
Context.System.EventStream.Publish(new ImportStarted(
    sessionId, name, occurredAt, flow.CorrelationId, flow.UseCase));
```

6. Include correlation on SignalR notification DTOs.

## Use-case naming convention

Use dotted verbs aligned to API/actor responsibilities:

| Use case | When |
|----------|------|
| `Import.Start` | POST `/api/import` |
| `Import.Persist` | POST `/api/import/{id}/persist` |
| `Hours.Book` | POST hours on assignment |
| `Catalog.CreateProject` | Project mutation |
| `Background.Start` | Long-running process |
| `LiveMessage.Publish` | Push live message |

## Querying logs (Seq)

```
CorrelationId = 'abc123'
```

Or combine with use case:

```
CorrelationId = 'abc123' and UseCase = 'Import.Start'
```

## New module checklist

- [ ] `UsePlatformCorrelationPipeline()` in host
- [ ] `Platform.Logging.Akka.props` on actor projects
- [ ] Facade uses `AskCorrelated` / `TellCorrelated` with explicit use-case names
- [ ] Async EF actors capture `Sender` in `ReceiveAsync<CorrelatedMessageEnvelope>` (see correlation doc)
- [ ] `IDataEvent` publications include `CorrelationId` and `UseCase`
- [ ] SignalR payloads expose correlation for client debugging

## Repack

```bash
./scripts/pack-platform-logging.sh 1.1.0
```
