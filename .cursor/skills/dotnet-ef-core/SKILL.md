---
name: dotnet-ef-core
description: |
  Guides Entity Framework Core data access: DbContext, entities, configurations, migrations,
  LINQ queries, and ASP.NET Core integration. Use when:
  - Adding or changing DbContext, entities, or fluent configurations
  - Creating or applying EF Core migrations
  - Writing LINQ queries, includes, or projections
  - Registering EF Core in dependency injection
  - Testing data access or troubleshooting tracking/query issues
  - Choosing between EF Core and raw SQL/Dapper for a feature
paths:
  - "**/*DbContext*.cs"
  - "**/Migrations/**/*.cs"
  - "**/*Migration*.cs"
  - "**/Entities/**/*.cs"
  - "**/Data/**/*.cs"
  - "**/*.csproj"
metadata:
  version: 1.0.0
---

# Entity Framework Core

Apply this skill for **EF Core** (`Microsoft.EntityFrameworkCore`) on modern .NET (ASP.NET Core, worker services, class libraries).

**Do not apply EF Core patterns to legacy EF6** code under `DrivenIt.Foundation/` (packages `EntityFramework` 6.x, `IdentityDbContext` from ASP.NET Identity). That stack uses a different API and migration tooling.

For general C# and host setup, use the `dotnet-core-csharp-development` skill.

## Package baseline

Typical packages for SQL Server + design-time tools:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

Match the EF Core major version to the project's target framework. Add provider packages only for the database in use (Npgsql, SQLite, etc.).

## Project layout

| Piece | Location |
|-------|----------|
| `DbContext` | `Data/` or `Infrastructure/Persistence/` |
| Entity types | `Entities/` or feature folders |
| Fluent configs | `Configurations/` as `IEntityTypeConfiguration<T>` |
| Migrations | `Migrations/` (generated; do not hand-edit unless fixing a failed migration) |

Keep the data layer in a dedicated project when the solution grows; reference it from the API host only for registration and thin repositories if used.

## DbContext

- One context per bounded context / database; name it after the domain (`CatalogDbContext`, not `AppDbContext` unless truly app-wide).
- Prefer `DbSet<TEntity>` properties for aggregate roots exposed to the application.
- Override `OnModelCreating` only to call `ApplyConfigurationsFromAssembly` or register a few global conventions; put entity rules in `IEntityTypeConfiguration<T>`.
- Use `OnConfiguring` sparingly — prefer options configured in DI for ASP.NET Core apps.

```csharp
public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Organisation> Organisations => Set<Organisation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
```

## Entities and configuration

- Use explicit keys (`Id` or `{Name}Id`); configure composite keys in fluent API.
- Prefer `record` or `class` with init-only properties for DTO-like entities; avoid public setters on navigations unless needed.
- Map relationships explicitly (`HasOne`, `WithMany`, `OnDelete`) to avoid cascade surprises.
- Use owned types for value objects; use `HasConversion` for enums and small value types stored as strings.
- Index frequently filtered columns (`HasIndex`).

## ASP.NET Core registration

```csharp
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Catalog")));
```

| Lifetime | Rule |
|----------|------|
| `DbContext` | **Scoped** (default with `AddDbContext`) — one per HTTP request |
| `IDbContextFactory<T>` | Use for background workers or parallel work; create short-lived contexts with `await using` |

Never inject `DbContext` into **singleton** services (including Akka.NET actors). Resolve data in the API layer or use `IDbContextFactory<T>` inside scoped operations.

Connection strings belong in configuration (`ConnectionStrings` section), not in source code.

## Queries

- Prefer `AsNoTracking()` for read-only API responses.
- Project early with `Select` to DTOs instead of loading full graphs.
- Use `Include` / `ThenInclude` only when needed; consider split queries (`AsSplitQuery`) for large cartesian products.
- Avoid client evaluation: filter and project in SQL-translatable expressions.
- Paginate with `Skip` / `Take` and a stable order-by.

## Migrations workflow

From the project containing the context (startup project = API host if context is in another assembly):

```bash
dotnet ef migrations add InitialCatalog --project path/to/Data.csproj --startup-project path/to/Api.csproj
dotnet ef database update --project path/to/Data.csproj --startup-project path/to/Api.csproj
```

- One logical change per migration; use descriptive names (`AddProjectStatusColumn`).
- Review generated migration code before committing.
- For teams, commit migrations to source control; do not regenerate the same schema change under a new name.
- Production: apply via CI/CD or `dotnet ef database update`, not `EnsureCreated()` except in throwaway demos.

Install tools once: `dotnet tool install --global dotnet-ef` (or use `dotnet ef` as a local tool).

## Testing

| Approach | When |
|----------|------|
| In-memory provider | Fast unit tests; limited provider fidelity — do not rely on for SQL-specific behavior |
| SQLite (file or in-memory) | Closer relational behavior for integration tests |
| Testcontainers + real DB | Highest fidelity for migrations and provider features |

- Use a fresh database or transaction rollback per test when tests mutate data.
- Seed via `HasData` only for static reference data; use test helpers for dynamic fixtures.

## Performance and reliability

- Enable sensitive data logging only in Development.
- Use compiled queries or `AsNoTracking` for hot paths when profiling shows benefit.
- Retry on transient failures: `options.UseSqlServer(connectionString, o => o.EnableRetryOnFailure())` for Azure SQL / cloud DBs.
- Batch saves: single `SaveChangesAsync` per unit of work unless intermediate IDs are required.

## Anti-patterns

- Lazy loading enabled without understanding N+1 query cost.
- Returning `IQueryable` from repositories through API layers (unbounded queries, context lifetime issues). Use the `specification-pattern` skill — encapsulate query intent in named specifications.
- Mixing EF6 and EF Core against the same database with separate migration histories without a plan.
- `Database.EnsureCreated()` in production instead of migrations.
- Long-lived `DbContext` across multiple requests or actor messages.

## Related skills

- `dotnet-core-csharp-development` — DI, configuration, ASP.NET Core host, `dotnet` CLI.
- `specification-pattern` — named query rules; keep `IQueryable` inside repositories.
- `akka-net` — actors must not hold scoped `DbContext`; keep persistence at the host boundary.

Official reference: https://learn.microsoft.com/en-us/ef/core/
