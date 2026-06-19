---
name: specification-pattern
description: |
  Guides the Specification pattern for encapsulating reusable business and query rules in C# / .NET.
  Use when:
  - Modeling composable domain rules (validation, eligibility, routing, matching)
  - Replacing leaked IQueryable, DbContext, or DbSet usage above the data layer
  - Designing repository query methods without proliferating custom ListBy* APIs
  - Implementing Ardalis.Specification with EF Core repositories
  - Choosing between Specification, Expression<Func<T,bool>>, or in-memory predicates
  - Reviewing whether query logic is scattered across controllers, services, and views
  Combine with domain-driven-design for conceptual foundations and dotnet-ef-core for DbContext rules.
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/docs/**"
metadata:
  version: 1.0.0
  source: "Eric Evans — Domain-Driven Design; Steve Smith — Ardalis.Specification; https://ardalis.com/avoid-dbcontext-iqueryable-proliferation/"
  companion: "domain-driven-design, dotnet-ef-core"
---

# Specification Pattern

Apply this skill when **named, reusable rules** must be expressed, composed, and tested — especially when those rules also drive **database queries** without leaking `IQueryable<T>` or `DbContext` outside infrastructure.

Match existing project conventions (see `dotnet-core-csharp-development`). For EF Core registration and context lifetime, use `dotnet-ef-core`.

## What it is

A **Specification** is a domain object that answers: *does this candidate satisfy the rule?*

In .NET data access, the same object also **describes the query** (filters, includes, ordering, projection) so EF Core can translate it to SQL inside the repository.

| Concern | Specification handles |
|---------|----------------------|
| Domain language | `ActiveOrdersForUserSpec`, `EligibleForDiscountSpec` |
| Reuse | Same rule in API, domain service, background job |
| Composition | AND / OR / NOT of rules |
| Testability | Test spec in memory without database |
| Encapsulation | Query built once; callers pass intent, not LINQ chains |

## The IQueryable anti-pattern (do not do this)

Returning `IQueryable<T>` (or passing `DbContext` / `DbSet<T>`) from repositories or services lets every layer append `.Where()` — efficient SQL at first, but:

- Query logic **proliferates** across controllers, business services, Razor views
- Callers may not know data is **deferred** (`IQueryable` implements `IEnumerable`)
- Custom methods in filters may **fail EF translation** at runtime
- **No encapsulation** — separation of concerns breaks even with layered architecture

**Wrong fix:** load everything and filter in memory.

**Right fix:** define the full query intent as a **named specification**; repository executes it once.

Reference: [Avoid Proliferating DbContext or IQueryable](https://ardalis.com/avoid-dbcontext-iqueryable-proliferation/) (Steve Smith).

## Architecture

```
Application / API  →  new SomeSpec(...)  →  IRepository<T>.ListAsync(spec)
                                                    ↓
                              Infrastructure: SpecificationEvaluator + DbContext (IQueryable inside only)
```

Rules:

- **`IQueryable<T>` stays inside** repository implementation (or private helpers)
- **Public repository interface** accepts `ISpecification<T>` (or returns materialized results)
- **Specifications are named types** — not anonymous lambdas passed from controllers
- **`DbContext` is scoped** — never in singletons (see `dotnet-ef-core`, `akka-net`)

## Evans DDD view (conceptual)

From Eric Evans — make implicit criteria **explicit**:

- A specification is a **predicate object** in the Ubiquitous Language
- Use when rules are **reused**, **combined**, or **central to domain reasoning**
- Classic example: `RouteSpecification` — does this `Itinerary` satisfy the route requirements?

Minimal domain interface (works without EF):

```csharp
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);
}
```

Composable decorators:

```csharp
public sealed class AndSpecification<T>(ISpecification<T> left, ISpecification<T> right)
    : ISpecification<T>
{
    public bool IsSatisfiedBy(T candidate) =>
        left.IsSatisfiedBy(candidate) && right.IsSatisfiedBy(candidate);
}
```

For persistence-heavy apps, prefer **Ardalis.Specification** (below) — it keeps Evans's intent and adds EF query building.

## Ardalis.Specification (recommended for EF Core)

Used in [eShopOnWeb](https://github.com/dotnet-architecture/eShopOnWeb) and [Clean Architecture template](https://github.com/ardalis/CleanArchitecture).

### Packages

```xml
<PackageReference Include="Ardalis.Specification" Version="9.*" />
<PackageReference Include="Ardalis.Specification.EntityFrameworkCore" Version="9.*" />
```

Match major version to your EF Core version. Docs: https://ardalis.github.io/Specification/

### Define a specification

```csharp
public sealed class ShippedOrdersForUserSpec : Specification<Order>
{
    public ShippedOrdersForUserSpec(string username)
    {
        Query.Where(o => !o.IsCanceled
                      && o.CreatedBy == username
                      && o.WasShipped);

        Query.OrderByDescending(o => o.OrderDate);
        Query.Include(o => o.Lines);
    }
}
```

- Inherit `Specification<T>` (or `Specification<T, TResult>` for projections)
- Configure via `Query` builder — **all** filter/include/order logic lives here
- Name types after **domain intent**, not SQL shape

### Apply in repository (infrastructure)

**Option A — built-in repository base**

```csharp
public interface IReadRepository<T> where T : class
{
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);
}
```

Extend `RepositoryBase<T>` from `Ardalis.Specification.EntityFrameworkCore`.

**Option B — custom repository with evaluator**

```csharp
public async Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default)
{
    var query = SpecificationEvaluator.Default
        .GetQuery(_dbContext.Set<T>().AsNoTracking(), spec);

    return await query.ToListAsync(ct);
}
```

**Option C — direct DbSet extension (small apps / internal only)**

```csharp
var orders = await _dbContext.Orders
    .WithSpecification(new ShippedOrdersForUserSpec(username))
    .ToListAsync(ct);
```

Prefer repository abstraction in layered or Clean Architecture solutions.

### Caller (application layer)

```csharp
var spec = new ShippedOrdersForUserSpec(username);
var orders = await _orderRepository.ListAsync(spec, cancellationToken);
```

No `.Where()` upstream. If a screen needs a different query, add or compose a **new spec class**.

## Query features (Ardalis `Query` builder)

| Need | Example |
|------|---------|
| Filter | `Query.Where(o => o.Status == OrderStatus.Active)` |
| Include | `Query.Include(o => o.Customer)` |
| Order | `Query.OrderBy(o => o.Name)` |
| Paging | `Query.Skip(20).Take(10)` |
| Projection | `Specification<Entity, Dto>` + `Query.Select(e => new Dto(...))` |
| Split query | `Query.AsSplitQuery()` (large graphs) |
| No tracking | `Query.AsNoTracking()` (read models) |

Keep expressions **EF-translatable** — avoid custom instance methods in `Where` unless mapped or evaluated client-side deliberately.

## Composition and reuse

| Technique | When |
|-----------|------|
| **Base spec class** | Shared rules (`ActiveOrderSpec` extended by `ActiveOrdersForUserSpec`) |
| **Constructor parameters** | `CustomerByIdSpec(int id)`, date ranges, tenant IDs |
| **Nested specs** | Ardalis supports combining specs — prefer explicit named composites for clarity |
| **AND / OR (domain)** | `AndSpecification<T>` for in-memory; for EF prefer single spec with clear `Where` or documented Ardalis combinators |

Avoid ten nearly identical spec classes — extract shared query fragments into protected helpers or base specs.

## Specification vs alternatives

| Approach | Use when | Avoid when |
|----------|----------|------------|
| **Specification class** | Named, reused, testable rules; repository queries | One-off trivial `GetById` |
| **`Expression<Func<T,bool>>` param** | Quick generic repo; no domain naming needed | Rules proliferate unnamed across callers |
| **`IQueryable` return** | Never as public API | Always for maintainable layered apps |
| **Raw LINQ in service** | Throwaway prototype | Production domain with multiple consumers |
| **CQRS read model / SQL** | Heavy reporting, denormalized reads | Simple entity filters — spec is enough |

## Project layout

| Piece | Location |
|-------|----------|
| Specification classes | `Domain/Specifications/` or `Application/Specifications/` |
| `ISpecification<T>` (custom) | Domain — only if not using Ardalis base types in domain |
| Repository interfaces | Application or Domain (interface); implementation in Infrastructure |
| `SpecificationEvaluator` usage | Infrastructure only |

Keep specifications **discoverable** — one folder per bounded context, names aligned with Ubiquitous Language.

## Testing

```csharp
[Fact]
public void ActiveOrderSpec_excludes_canceled()
{
    var spec = new ActiveOrderSpec();
    var order = new Order { IsCanceled = true };

    // In-memory: use spec.IsSatisfiedBy if exposed, or evaluate via evaluator + in-memory provider
    Assert.False(/* spec excludes order */);
}
```

| Layer | Test |
|-------|------|
| Spec logic | Unit test with entity instances or in-memory EF |
| Repository + spec | Integration test with SQLite / Testcontainers |
| API | Use spec indirectly — assert HTTP result, not SQL |

## Practical workflow for agents

1. **Find leaked queries** — `IQueryable` return types, `DbContext` outside Infrastructure, `.Where()` in controllers.
2. **Name the intent** — what domain question is being asked?
3. **Create `*Spec` class** — all `Where` / `Include` / `OrderBy` inside `Query` configurator.
4. **Slim repository** — `ListAsync`, `FirstOrDefaultAsync`, `CountAsync` accepting `ISpecification<T>`.
5. **Update callers** — `new XxxSpec(args)` then repository method.
6. **Delete** custom `GetActiveOrdersByUser` / `ListOrdersFor` repository methods replaced by specs.
7. **Verify SQL** — log or profile once; ensure no client evaluation surprises.

## Red flags

- `Task<IQueryable<T>>` on repository or service interfaces
- `DbContext` injected into controllers or domain services
- Different `.Where()` chains for the same business rule in multiple files
- Specification named `FilterSpec` with no domain meaning
- Custom instance methods inside `Where` that EF cannot translate
- Specification used only once with no reuse — a private repository method may suffice
- Domain layer referencing `Ardalis.Specification.EntityFrameworkCore` — keep EF types in Infrastructure

## When Specification is overkill

- Single `GetByIdAsync(Guid id)` with no extra rules
- Prototype with one query and no reuse expected
- Read side fully handled by raw SQL / Dapper / materialized view with no shared domain rule

Still avoid returning `IQueryable` — return `List<T>`, `Task<T?>`, or a paged result DTO.

## Related skills

- **`domain-driven-design`** — Specification as explicit domain concept; Ubiquitous Language
- **`implementing-domain-driven-design`** — repositories, application services, CQRS read models
- **`dotnet-ef-core`** — DbContext lifetime, `AsNoTracking`, migrations, anti-patterns
- **`domain-specific-languages`** — specifications as declarative rule objects (different from DSL, but similar “named intent” mindset)

## Further reading

- Eric Evans — *Domain-Driven Design* (Specification pattern)
- Steve Smith — [Avoid DbContext / IQueryable proliferation](https://ardalis.com/avoid-dbcontext-iqueryable-proliferation/)
- [Ardalis.Specification docs](https://ardalis.github.io/Specification/)
- [eShopOnWeb](https://github.com/dotnet-architecture/eShopOnWeb) — reference usage
- [Clean Architecture template](https://github.com/ardalis/CleanArchitecture)
