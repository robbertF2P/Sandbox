# Module composition — DI without ABP

**Purpose:** Standard for registering extracted bounded contexts in Platform 2.0 and the F2P monolith refactor. **New modules use Microsoft’s built-in DI** — not ABP modules, `AbpDbContext`, or Volo.Abp.* wiring.

**Audience:** Engineers and AI agents scaffolding or extracting modules.

**Legacy note:** The existing monolith still runs on ABP during the strangler. That is inventory, not target design. **Do not add new `AbpModule` subclasses or ABP package dependencies** to extracted modules.

---

## Core rules

| Rule | Detail |
|------|--------|
| **No ABP in new modules** | No `Volo.Abp.*`, `AbpModule`, `IAbpApplication`, `AbpDbContext`, ABP dynamic proxies |
| **Extension methods on `IServiceCollection`** | One `Add<Context>Module` (or split by layer) per bounded context — [Microsoft DI guidance](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) |
| **Explicit registration** | Every service registered in code you can grep; no implicit convention scanning unless team-approved (e.g. Scrutor with a named assembly list) |
| **Host is composition root** | Only `Program.cs` / host startup calls `Add*Module`; modules do not reference each other’s Infrastructure |
| **Endpoints via `WebApplication`** | `Map<Context>Endpoints` extension on `WebApplication` or `IEndpointRouteBuilder` |
| **Strangler to legacy ABP** | New module → `[StranglerAdapter]` → legacy service interface; adapter lives in Infrastructure, registered explicitly |

---

## Project layout

```text
<Context>.Application/
  DependencyInjection.cs          ← optional: Add<Context>Application
  Ports/                            ← interfaces only

<Context>.Infrastructure/
  DependencyInjection.cs          ← Add<Context>Infrastructure(services, configuration)
  Persistence/                      ← EF DbContext, repositories

<Context>.Api/
  DependencyInjection.cs          ← Add<Context>Api(services)
  <Context>Endpoints.cs           ← Map<Context>Endpoints(app)
```

`Domain` has **no** DI registration class — no framework references.

---

## Registration pattern

### Application + Infrastructure

```csharp
// Import.Infrastructure/DependencyInjection.cs
namespace Import.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddImportInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<ImportDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<IImportJobRunner, ImportJobRunner>();
        services.AddScoped<IStranglerImportAdapter, LegacyImportStranglerAdapter>();

        return services;
    }
}
```

### Module facade (single entry for the host)

```csharp
// Import.Api/DependencyInjection.cs
namespace Import.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddImportModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddImportApplication();
        services.AddImportInfrastructure(configuration);

        return services;
    }

    public static WebApplication MapImportModule(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapImportEndpoints();
        return app;
    }
}
```

### Host (`Program.cs`)

```csharp
builder.Services.AddImportModule(builder.Configuration);
// other modules...

var app = builder.Build();

app.MapImportModule();
```

---

## Lifetimes

| Lifetime | Use for |
|----------|---------|
| `Singleton` | Caches, options, stateless facades |
| `Scoped` | DbContext, repositories, use-case handlers (default for request work) |
| `Transient` | Lightweight stateless helpers only |

Match neighbouring monolith registrations when bridging via strangler adapters.

---

## Options and configuration

```csharp
services.AddOptions<ImportOptions>()
    .Bind(configuration.GetSection(ImportOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

Use `IOptions<T>` / `IOptionsSnapshot<T>` in constructors — not `IConfiguration` in domain services.

---

## Hangfire / background work

Register jobs in the module’s Infrastructure extension; enqueue from Application:

```csharp
services.AddScoped<ImportDisciplineJob>();
// Host Hangfire config calls AddHangfire once; module only registers job types
```

Do not use `IBackgroundJobManager` (ABP). Use Hangfire API directly or a thin port interface defined in Application.

---

## EF Core

- `DbContext` registered with `AddDbContext<T>` in Infrastructure extension only.
- Migrations project references Infrastructure; host or `DatabaseDeployer` runs migrations.
- No `AbpDbContext<T>` — plain `DbContext` or team `BaseDbContext` without ABP base types in **new** code.

---

## Testing

```csharp
// Characterization / integration test host
var builder = WebApplication.CreateBuilder();
builder.Services.AddImportModule(builder.Configuration);
// override with test doubles after AddImportModule if needed:
builder.Services.AddScoped<IImportJobRunner, FakeImportJobRunner>();
```

Test projects call the same `Add*Module` extensions as production — no parallel ABP test module.

---

## Anti-patterns

| Avoid | Use instead |
|-------|-------------|
| `AbpModule` / `[DependsOn]` | `IServiceCollection` extension methods |
| Service locator (`IServiceProvider` in domain) | Constructor injection |
| Hidden assembly scanning for “all handlers” | Explicit `AddScoped<IHandler, Handler>()` or documented Scrutor scan list |
| Module A references module B Infrastructure | Contracts + host wires both |
| New code inheriting `AbpDbContext` | `DbContext` + explicit `OnModelCreating` |

---

## Agent checklist (implementation)

- [ ] No new `Volo.Abp` package references in module projects
- [ ] `Add<Context>Module` extension exists and is called from host only
- [ ] All registrations visible in `DependencyInjection.cs` for the layer
- [ ] Endpoints mapped via `Map<Context>Endpoints` / `Map<Context>Module`
- [ ] Strangler adapters registered explicitly with `[StranglerAdapter]` and removal ticket
- [ ] Tests use same `Add*Module` path as production

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-21 | Initial standard; ABP excluded from new modules |
