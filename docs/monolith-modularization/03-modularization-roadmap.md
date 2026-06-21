# Modularization roadmap (SandBox POCs)

**Purpose:** Cross-cutting standards and extraction order for Platform 2.0 POCs in this repo, ahead of the full monolith strangler program in the external F2P repository.

**Related:** `docs/monolith-modularization/copilot-analysis-instructions.md`, `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md`

---

## POC modules in scope

| Module | Role | Status |
|--------|------|--------|
| `Platform.Serilog.Logging` | Shared Serilog sink routing (Seq / App Insights / tests) | NuGet `1.0.0` → `local-feed/` |
| `ImportPipeline.Domain` | Config-driven row mapping kernel (NuGet) | Packaged `1.0.0` → `local-feed/` |
| `PrimaveraExcelReader` | Primavera Excel I/O + typed profiles + import DTO bridge | Active |
| `ApiImportActorPoc` | Actor-based import orchestration sketch | Reference |
| `AkkaSignalRVuePoc` | Reactive hosting + SignalR reference | Reference |

---

## Cross-cutting requirements (all new POC modules)

### 1. Structured logging — Serilog (platform standard)

**Requirement:** All application and library code MUST log through **Serilog** using `Microsoft.Extensions.Logging.ILogger<T>` (or Akka `ILoggingAdapter` for actors). No `Console.WriteLine`, no bespoke log helpers.

**Shared library:** `Platform.Serilog.Logging` (NuGet) — single source of truth for enrichers and environment-based sinks.

**Adoption:** import `build/Platform.Logging.*.props` per project type. See [platform-logging-standard.md](./platform-logging-standard.md).

| Environment | Sinks | Configuration |
|-------------|-------|---------------|
| **Development** (local) | Console + **Datalust Seq** | `Seq:ServerUrl` in `appsettings.Development.json` (e.g. `http://localhost:5341`) |
| **Production** | Console + **Azure Application Insights** | `ApplicationInsights:ConnectionString` or `APPLICATIONINSIGHTS_CONNECTION_STRING` |
| **Unit / integration tests** | **Serilog.Sinks.XUnit3** only | `Platform.Serilog.Logging.Testing` — same `ConfigureShared` pipeline as production |

**Application hosts** wire logging via:

```csharp
builder.AddPlatformLogging("Module Name");
app.UsePlatformRequestLogging();
```

**Libraries** (e.g. `PrimaveraExcelReader`):

- Accept `ILogger<T>` via constructor injection; default to `NullLogger` when omitted.
- Log at meaningful boundaries: sheet read complete, row skipped/filtered, mapping invalid, parse errors, batch summary.
- Optional module enricher: `.Enrich.WithProperty("Application", "ModuleName")`.

**Reference:** `Platform.Serilog.Logging/SerilogLogging.cs`, `HostBuilderExtensions.cs`

### 2. Test logging — Serilog xUnit sink (mandatory)

**Requirement:** Every unit and integration test project MUST reference `Platform.Serilog.Logging.Testing` and route logs through **`Serilog.Sinks.XUnit3`** (`WriteTo.XUnit3TestOutput()`).

```csharp
// Provided by Platform.Serilog.Logging.Testing
Serilog.ILogger logger = SerilogTestLogging.CreateTestLogger();
Log.Logger = logger;
builder.AddSerilog(logger, dispose: true);
```

- Assembly `[ModuleInitializer]` in `Platform.Serilog.Logging.Testing` sets a default test logger when the assembly loads.
- Test services use `SerilogTestLogging.CreateLogger<T>()` — **not** a separate test-only logging stack.
- Goal: test output shows the **same structured log shape** the deployed application emits.

**Applies to:** all `*.Tests` projects in SandBox POCs and every bounded-context module extracted from the monolith.

### 3. Import pipeline package boundary

- `ImportPipeline.Domain` ships as **NuGet** (`ImportPipeline.Domain` 1.0.0, `local-feed/`).
- Consumers reference the package — not project references.
- Repack: `./scripts/pack-import-pipeline-domain.sh [version]`

### 4. Platform logging package boundary

- `Platform.Serilog.Logging` and `Platform.Serilog.Logging.Testing` ship as **NuGet** (`1.0.0`, `local-feed/`).
- Consumers import `build/Platform.Logging.Host.props`, `.Library.props`, or `.Tests.props` — not project references.
- Repack: `./scripts/pack-platform-logging.sh [version]` or `./scripts/pack-local-platform-packages.sh [version]`
- New modules: `./scripts/add-platform-logging-to-module.sh <ModuleRoot>`

### 5. Behaviour preservation

- Characterization tests green before and after extraction (see quality framework G5).
- Import/read modules collect all row issues per sheet — no fail-fast on first bad row.

---

## Extraction sequence (recommended)

```text
1. Platform.Serilog.Logging (observability standard)   ← done
2. ImportPipeline.Domain (pure mapping)                ← done
3. PrimaveraExcelReader (Excel ACL + profiles)         ← in progress
4. Intermediate exchange format + golden files
5. ApiImportActorPoc → production import host slice
6. Per-integration packs (PLM, Primavera, IFS, …) as strangler adapters
```

---

## Definition of done — logging (per module)

- [ ] Imports `build/Platform.Logging.*.props` (or documents why host-only)
- [ ] Development → Seq; Production → Application Insights
- [ ] `ILogger<T>` injected at service boundaries
- [ ] Test project references `Platform.Serilog.Logging.Testing` with xUnit sink
- [ ] At least one integration/characterization test exercises logging on the happy path

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-20 | Initial SandBox POC roadmap; Serilog + xUnit sink requirement |
| 1.2 | 2026-06-20 | Platform.Serilog.Logging NuGet + `build/` MSBuild adoption standard |
