# OutOfTokens

Standalone extraction of the **P6 EPPM connector** actor pipeline — compiles and tests without the full monolith. Also the home of **reusable Akka.NET actors** (`Infrastructure.Akka`).

---

## Start here

| Doc | What you get |
|-----|----------------|
| **[docs/actor-overview.md](docs/actor-overview.md)** | Visual pipeline diagram, session lifecycle, code samples — **best for advocating Akka.NET** |
| **[docs/reusable-actors.md](docs/reusable-actors.md)** | Generic actor catalog with source links |
| **[docs/partial-project-sync.md](docs/partial-project-sync.md)** | Partial / delta sync per selected project |
| [docs/monolith-modularization/platform-actor-standard.md](../docs/monolith-modularization/platform-actor-standard.md) | Platform 2.0 actor rules |

---

## Solution layout

```text
OutOfTokens/
  Infrastructure.Akka/          ← reusable actors (SessionGate, PagedFetch, …)
  Floor2Plan.Connectors.P6/     ← P6 connector (composes generic actors)
  Floor2Plan.UnitTest.Connectors.P6/
  Stubs/                        ← fake monolith dependencies for standalone build
  OutOfTokens.sln
```

---

## Build & test

```bash
cd OutOfTokens
dotnet build OutOfTokens.sln
dotnet test OutOfTokens.sln
```

Actor tests inherit `AkkaSerilogTestKit` using `Platform.Serilog.Logging.Testing` — pipeline logs appear in xUnit test output during `dotnet test` (use `--logger "console;verbosity=detailed"` or the IDE test view).

`P6Connector.SyncAllAsync` queues sync via **Tell**; live progress is written to the sync log by `P6SyncProgressActor` (scoped `IProcessLogger` per progress message).

---

## Related learning paths

| Resource | Role |
|----------|------|
| [AkkaTeach](../AkkaTeach/README.md) | Phased Akka.NET concepts (Tell, Become, PipeTo) |
| [AkkaSignalRVuePoc](../AkkaSignalRVuePoc/) | Hosting + SignalR |
| [ApiImportActorPoc](../ApiImportActorPoc/) | Import workflow orchestration |
