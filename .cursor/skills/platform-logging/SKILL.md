---
name: platform-logging
description: |
  Central Serilog logging for SandBox modules via Platform.Serilog.Logging NuGet and MSBuild props.
  Use when:
  - Adding or changing logging in a host, library, or test project
  - Wiring Seq (dev), Application Insights (prod), or xUnit test sink
  - Adopting build/Platform.Logging.*.props in a new module
  - Packing or versioning Platform.Serilog.Logging packages
paths:
  - "Platform.Serilog.Logging/**"
  - "build/Platform.Logging*.props"
  - "**/Program.cs"
  - "**/*.Tests/**"
  - "scripts/pack-platform-logging.sh"
  - "scripts/add-platform-logging-to-module.sh"
metadata:
  version: 1.0.0
---

# Platform logging

**Authoritative doc:** `docs/monolith-modularization/platform-logging-standard.md`

**Principle:** Consumers use NuGet + MSBuild props — not project references to `Platform.Serilog.Logging` source (except when developing the platform package itself).

## Packages

| Package | Purpose |
|---------|---------|
| `Platform.Serilog.Logging` | Enrichers, sinks (Seq / App Insights), host extensions |
| `Platform.Serilog.Logging.Testing` | xUnit Serilog sink + `SerilogTestLogging` |
| `Platform.Serilog.Logging.Akka` | Akka correlation helpers (see `platform-correlation`) |

Versions: `build/Platform.Logging.Versions.props` (currently `1.1.0`). Feed: `local-feed/`.

## MSBuild adoption

| Project kind | Import |
|--------------|--------|
| ASP.NET host (`*.Api`) | `build/Platform.Logging.Host.props` |
| Class library | `build/Platform.Logging.Library.props` |
| Tests (`*.Tests`) | `build/Platform.Logging.Tests.props` |

Every module `Directory.Packages.props`:

```xml
<Import Project="..\build\Platform.Logging.Versions.props" />
```

Adjust relative path to `build/` from module root.

## Host wiring

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformLogging("My Module API");

var app = builder.Build();
app.UsePlatformCorrelationPipeline(); // before request logging — see platform-correlation
app.UsePlatformRequestLogging();
```

## Tests

Via `Platform.Logging.Tests.props`:

```csharp
global::Serilog.ILogger logger = SerilogTestLogging.CreateTestLogger();
builder.AddPlatformSerilog(logger);
```

`SerilogTestAssemblyInitializer` registers the xUnit sink for the test assembly.

## Sink routing

| Environment | Sinks | Config |
|-------------|-------|--------|
| Development | Console + Seq | `Seq:ServerUrl` |
| Production | Console + Application Insights | `ApplicationInsights:ConnectionString` |
| Tests | xUnit only | Testing package |

## Pack / repack

```bash
./scripts/pack-platform-logging.sh [version]
```

Updates `local-feed/Platform.Serilog.Logging*.nupkg`. Bump `build/Platform.Logging.Versions.props` when releasing.

## New module checklist

- [ ] `Directory.Packages.props` imports `Platform.Logging.Versions.props`
- [ ] Correct `Platform.Logging.*.props` on host / library / test projects
- [ ] `appsettings.Development.json` → `Seq:ServerUrl`
- [ ] `appsettings.Production.json` → App Insights connection string placeholder
- [ ] `AddPlatformLogging` in host `Program.cs`
- [ ] Tests use `SerilogTestLogging` / `AddPlatformSerilog`

## Do / don't

| Do | Don't |
|----|-------|
| Import props files | Copy-paste Serilog `Program.cs` bootstrap per module |
| Use `ILogger<T>` in libraries | Reference platform source from consumer modules |
| Run pack script after platform API changes | Hand-edit `.nupkg` files |
| Keep application name meaningful in `AddPlatformLogging("…")` | Use generic `"API"` everywhere |
