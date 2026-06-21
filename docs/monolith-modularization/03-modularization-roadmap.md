# Modularization roadmap (SandBox POCs)

**Purpose:** Cross-cutting standards and extraction order for Platform 2.0 POCs in this repo, ahead of the full monolith strangler program in the external F2P repository.

**Related:** `docs/monolith-modularization/analysis-instructions.md`, `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md`

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

### 2. Correlation — end-to-end use case tracing

**Requirement:** Every HTTP request, actor command, domain event, and SignalR notification MUST carry a shared correlation identity so logs can be filtered as one use-case flow in Seq or Application Insights.

**Packages:** `Platform.Serilog.Logging` (HTTP + Serilog) and `Platform.Serilog.Logging.Akka` (actor envelopes).

| Concept | Header / property | Purpose |
|---------|-------------------|---------|
| `CorrelationId` | `X-Correlation-Id` | Stable ID for the full flow (generated if absent) |
| `UseCase` | `X-Use-Case` (optional) | Semantic operation name, e.g. `Import.Start` |
| `CausationId` | `X-Causation-Id` (optional) | Parent message ID for nested steps |

**HTTP hosts:**

```csharp
app.UsePlatformCorrelationPipeline();
app.UsePlatformRequestLogging();
```

**Actor boundaries:** wrap commands with `AskCorrelated` / `TellCorrelated` and `ReceiveCorrelated` (see [platform-correlation-standard.md](./platform-correlation-standard.md)).

### 3. Test logging — Serilog xUnit sink (mandatory)

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

### 4. Frontend design system — `@floorganise/css` (mandatory)

**Requirement:** Every **V2 frontend module** (Nx context lib, shell, or strangler-replaced screen) MUST use **`@floorganise/css`** — Floorganise Tailwind v4 theme tokens and semantic component aliases (`f2ps-*`, shell, panels).

- Import globally: `@import '@floorganise/css'` in shell or shared styles entry.
- Use semantic classes for brand and components; Tailwind utilities for layout.
- Do **not** introduce parallel global themes, ad-hoc brand colours, or third-party CSS kits as the primary styling layer.

**Source:** SandBox `FloorganiseCss/` — publish to internal npm feed; monolith consumes via package version, not SandBox project references.

**Reference:** [platform-frontend-standard.md](./platform-frontend-standard.md), `FloorganiseCss/showcase-angular/`

### 5. Shared UI components — `@floorganise/ui` (mandatory for V2)

**Requirement:** Cross-context and shell widgets (home tiles, buttons, forms, navigation, toasts, table chrome) MUST come from **`@floorganise/ui`** — a shared Angular library built on `@floorganise/css`.

- Context `libs/<context>/ui` — **bounded-context presentational components only**.
- Promote duplicated widgets to `@floorganise/ui` when used by two or more contexts.
- Seed components from `FloorganiseCss/showcase-angular/`; develop package in SandBox Phase A8.

**Applies to:** monolith `floor2plan-web` Nx workspace and any new V2 SPA modules during strangler migration.

### 6. Import pipeline package boundary

- `ImportPipeline.Domain` ships as **NuGet** (`ImportPipeline.Domain` 1.0.0, `local-feed/`).
- Consumers reference the package — not project references.
- Repack: `./scripts/pack-import-pipeline-domain.sh [version]`

### 7. Platform logging package boundary

- `Platform.Serilog.Logging` and `Platform.Serilog.Logging.Testing` ship as **NuGet** (`1.0.0`, `local-feed/`).
- Consumers import `build/Platform.Logging.Host.props`, `.Library.props`, or `.Tests.props` — not project references.
- Repack: `./scripts/pack-platform-logging.sh [version]` or `./scripts/pack-local-platform-packages.sh [version]`
- New modules: `./scripts/add-platform-logging-to-module.sh <ModuleRoot>`

### 8. Behaviour preservation

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

## Definition of done — frontend (per V2 module)

- [ ] `@floorganise/css` in workspace dependencies; global styles import present
- [ ] Shared chrome and generic widgets from `@floorganise/ui` (not local copies)
- [ ] Context-specific markup only in `libs/<context>/ui`
- [ ] Nx `type:ui` libs have no `data-access` / `HttpClient` imports
- [ ] Visual parity spot-checked against `FloorganiseCss/showcase-angular` or SmokeTests DOM selectors

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-20 | Initial SandBox POC roadmap; Serilog + xUnit sink requirement |
| 1.2 | 2026-06-20 | Platform.Serilog.Logging NuGet + `build/` MSBuild adoption standard |
| 1.3 | 2026-06-21 | `@floorganise/css` + `@floorganise/ui` required for all V2 frontend modules |
