# Platform.Serilog.Logging

Shared Serilog pipeline for SandBox modules.

- **Development:** Console + Datalust Seq (`Seq:ServerUrl`)
- **Production:** Console + Azure Application Insights
- **Tests:** `Platform.Serilog.Logging.Testing` → xUnit3 sink

## Consumer setup

See [platform-logging-standard.md](../docs/monolith-modularization/platform-logging-standard.md).

Quick start for a new module:

```bash
./scripts/add-platform-logging-to-module.sh MyModule
```

## Pack

```bash
./scripts/pack-platform-logging.sh 1.0.0
```

Outputs `Platform.Serilog.Logging.*.nupkg` to `local-feed/`.

## API surface

| Type | Purpose |
|------|---------|
| `SerilogLogging.ConfigureShared` | Enrichers + level overrides |
| `SerilogLogging.ConfigureApplicationSinks` | Environment-based Seq / App Insights |
| `SerilogLogging.CreateBootstrapLogger` | Early startup logger |
| `HostBuilderExtensions.UsePlatformSerilog` | Generic host integration |
| `WebApplicationBuilderExtensions.AddPlatformLogging` | ASP.NET one-liner |
| `WebApplicationBuilderExtensions.UsePlatformRequestLogging` | HTTP request logging |

Testing helpers live in `Platform.Serilog.Logging.Testing`.
