# Platform logging standard

Central Serilog configuration for every SandBox module and future bounded-context extraction.

## Packages

| Package | Role |
|---------|------|
| `Platform.Serilog.Logging` | Shared enrichers + environment sinks (Seq / Application Insights) |
| `Platform.Serilog.Logging.Testing` | Same pipeline + `Serilog.Sinks.XUnit3` for tests |

Both ship as NuGet to `local-feed/` (version `1.1.0`). See also [platform-correlation-standard.md](./platform-correlation-standard.md) for end-to-end tracing.

```bash
./scripts/pack-platform-logging.sh [version]
# or all platform packages:
./scripts/pack-local-platform-packages.sh [version]
```

## MSBuild adoption (one import per project type)

Import the matching props file from `build/` — no project references to `Platform.Serilog.Logging` source.

| Project kind | Import |
|--------------|--------|
| ASP.NET host (`*.Api`, workers) | `build/Platform.Logging.Host.props` |
| Class library / domain module | `build/Platform.Logging.Library.props` |
| Test project (`*.Tests`) | `build/Platform.Logging.Tests.props` |

Each module's `Directory.Packages.props` must import central versions:

```xml
<Import Project="..\build\Platform.Logging.Versions.props" />
```

Adjust the relative path to `build/` from your module root.

## Runtime wiring

### ASP.NET host

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddPlatformLogging("My Module Name");

var app = builder.Build();
app.UsePlatformRequestLogging();
```

`AddPlatformLogging` sets the bootstrap logger and calls `UsePlatformSerilog`.

### Class library

Inject `ILogger<T>`. Optional standalone wiring:

```csharp
services.AddLogging(b => b.AddSerilog(SerilogLogging.CreateBootstrapLogger(configuration, applicationName: "MyModule")));
```

### Tests

Reference `Platform.Serilog.Logging.Testing` via `Platform.Logging.Tests.props`.

```csharp
global::Serilog.ILogger logger = SerilogTestLogging.CreateTestLogger();
builder.AddPlatformSerilog(logger);
```

`SerilogTestAssemblyInitializer` sets a default xUnit sink when the test assembly loads.

## Sink routing

| Environment | Sinks | Config keys |
|-------------|-------|-------------|
| Development | Console + Seq | `Seq:ServerUrl` |
| Production | Console + Application Insights | `ApplicationInsights:ConnectionString` or `APPLICATIONINSIGHTS_CONNECTION_STRING` |
| Tests | xUnit test output only | (none — via Testing package) |

## New module checklist

1. Enable central package management in `Directory.Packages.props` and import `build/Platform.Logging.Versions.props`.
2. Add the correct `build/Platform.Logging.*.props` import to host / library / test `.csproj` files.
3. Add `appsettings.Development.json` with `Seq:ServerUrl` and `appsettings.Production.json` with Application Insights connection string placeholder.
4. Wire `AddPlatformLogging` in the host `Program.cs`.
5. Use `SerilogTestLogging` in test fixtures; run `./scripts/add-platform-logging-to-module.sh` for a scaffold.

## Source vs package

- **Consumers** use NuGet from `local-feed/` via the `build/` props imports.
- **Platform.Serilog.Logging solution** uses project references internally; `Platform.Serilog.Logging/nuget.config` disables `local-feed` to avoid ambiguous package resolution during development.
