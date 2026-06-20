# Modularization roadmap (SandBox POCs)

**Purpose:** Cross-cutting standards and extraction order for Platform 2.0 POCs in this repo, ahead of the full monolith strangler program in the external F2P repository.

**Related:** `docs/monolith-modularization/copilot-analysis-instructions.md`, `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md`

---

## POC modules in scope

| Module | Role | Status |
|--------|------|--------|
| `ImportPipeline.Domain` | Config-driven row mapping kernel (NuGet) | Packaged `1.0.0` → `local-feed/` |
| `PrimaveraExcelReader` | Primavera Excel I/O + typed profiles + import DTO bridge | Active |
| `ApiImportActorPoc` | Actor-based import orchestration sketch | Reference |
| `AkkaSignalRVuePoc` | Reactive hosting + SignalR reference | Reference |

---

## Cross-cutting requirements (all new POC modules)

### 1. Structured logging — Serilog

**Requirement:** All application and library code that participates in the modular import/read pipeline MUST use **Serilog** via `Microsoft.Extensions.Logging.ILogger<T>` (or Akka `ILoggingAdapter` where actors are involved). Do not introduce ad-hoc `Console.WriteLine` or bespoke log helpers.

**Application hosts** (API, worker):

- Shared enrichers and minimum levels in a `SerilogLogging.ConfigureShared` helper per solution.
- Production sinks: Console + Seq (or Application Insights) as configured.
- Bootstrap logger for startup failures.

**Libraries** (e.g. `PrimaveraExcelReader`):

- Accept `ILogger<T>` via constructor injection; default to `NullLogger` when omitted.
- Log at meaningful boundaries: sheet read complete, row skipped/filtered, mapping invalid, parse errors, batch summary.
- No static logging except `Log.Logger` assignment in host/test bootstrap.

**Reference implementation:** `PrimaveraExcelReader/Logging/SerilogLogging.cs`, `ExcelReaderService`, `ImportPipelineRowMapping`.

### 2. Test logging — Serilog xUnit sink

**Requirement:** Automated tests MUST wire the **same Serilog pipeline** as production code, with **`Serilog.Sinks.XUnit3`** (`WriteTo.XUnit3TestOutput()`) so test output shows the log lines the deployed application would emit.

**Pattern:**

```csharp
// Tests/SerilogTestLogging.cs
return SerilogLogging.ConfigureShared(new LoggerConfiguration())
    .WriteTo.XUnit3TestOutput()
    .CreateLogger();
```

- Assembly `[ModuleInitializer]` sets `Log.Logger` for tests.
- Test services use `SerilogLoggerFactory` / `AddSerilog` — not a separate test-only logger implementation.
- Goal: when a characterization test fails, engineers see **production-shaped** log context in the test run, not a divergent format.

**Reference implementation:** `PrimaveraExcelReader.Tests/SerilogTestLogging.cs`, `SerilogTestAssemblyInitializer.cs`, `ExcelReaderTestLogging.cs`.

**Applies to:** `PrimaveraExcelReader`, `ApiImportActorPoc`, `AkkaSignalRVuePoc`, and every bounded-context module extracted from the monolith.

### 3. Import pipeline package boundary

- `ImportPipeline.Domain` ships as **NuGet** (`ImportPipeline.Domain` 1.0.0, `local-feed/`).
- Consumers (`PrimaveraExcelReader`, future import actors) reference the package — not project references.
- Repack: `./scripts/pack-import-pipeline-domain.sh [version]`

### 4. Behaviour preservation

- Characterization tests green before and after extraction (see quality framework G5).
- Import/read modules collect all row issues per sheet — no fail-fast on first bad row.

---

## Extraction sequence (recommended)

```text
1. ImportPipeline.Domain (pure mapping)     ← done
2. PrimaveraExcelReader (Excel ACL + profiles + Serilog)   ← in progress
3. Intermediate exchange format + golden files
4. ApiImportActorPoc → production import host slice
5. Per-integration packs (PLM, Primavera, IFS, …) as strangler adapters
```

---

## Definition of done — logging (per module)

- [ ] `SerilogLogging.ConfigureShared` (or equivalent) exists in host/library
- [ ] `ILogger<T>` injected at service boundaries; no silent swallow of mapping failures
- [ ] Tests use `Serilog.Sinks.XUnit3` with shared configuration — not a parallel logging stack
- [ ] At least one integration/characterization test exercises logging on the happy path
- [ ] Documented in this roadmap when module is complete

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-20 | Initial SandBox POC roadmap; Serilog + xUnit sink requirement |
