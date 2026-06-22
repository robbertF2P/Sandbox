---
name: immutable-domain-ef-core
description: |
  Guides immutable domain aggregates persisted with Entity Framework Core — constructors,
  With* copy helpers, value objects, and graph reconciliation without anemic setters.
  Use when:
  - Modeling aggregates as immutable types (records, init-only properties, ImmutableList)
  - Persisting immutable copies through EF Core change tracking
  - Designing With* methods, copy constructors, and invariants on aggregates
  - Configuring EF for alternate keys, value conversions, and child collection diffing
  - Choosing between mutable entities, owned types, and full immutable aggregates
  - Reviewing Zoran Horvat-style pragmatic DDD with EF Core (not heavy repository layers)
  Reference: https://github.com/zoran-horvat/immutable-domain-tools
paths:
  - "**/*DbContext*.cs"
  - "**/Models/**/*.cs"
  - "**/Domain/**/*.cs"
  - "**/Entities/**/*.cs"
  - "**/Infrastructure/**/*.cs"
  - "**/Migrations/**/*.cs"
metadata:
  version: 1.0.0
---

# Immutable domain models with EF Core

Apply this skill when aggregates should stay **immutable in application code** while EF Core handles persistence and change tracking underneath.

**Reference implementation:** [zoran-horvat/immutable-domain-tools](https://github.com/zoran-horvat/immutable-domain-tools) — `ImmutableDomain.EntityFrameworkCore` library + `Demo/` invoice aggregate.

For general EF Core setup and migrations, use `dotnet-ef-core`. For aggregate boundaries and value objects conceptually, use `domain-driven-design` and `implementing-domain-driven-design`. For `Select`/`With*` functional style, see `functional-programming-csharp`.

## When to use

| Use immutable aggregates | Prefer simpler alternatives |
|--------------------------|----------------------------|
| Rich invariants enforced at construction / `With*` | CRUD tables with no domain rules |
| Copy-and-replace workflow (`loaded.WithX(...).WithY(...)`) | Hot paths that mutate many fields per request |
| Value objects + child collections as part of aggregate | Simple owned types with no collection diffing |
| Teaching / enforcing “no accidental mutation” | Greenfield where Ardalis Specification + mutable roots already work |

This is **not** a generic repository abstraction. Keep the EF extension thin — align with pragmatic DDD: EF Core is already capable; add only what immutability requires.

## Core idea

```text
Load (tracked) → domain With* produces new instance → UpdateImmutable reconciles graph → SaveChanges
```

Application code never mutates a loaded aggregate. It builds a **new** instance; the library aligns EF’s change tracker with that copy.

## Aggregate modeling

### Structure

- **Primary constructor** for required data; **private copy constructor** to preserve identity (`Id`, `PublicId`) and children when applying `With*` methods.
- **`init` properties** for scalar fields; validate in property initializers or constructor.
- **`With*` methods** return new instances — name them after the field (`WithCustomerName`, `WithLines`), not generic `Copy`.
- **Child collections:** `ImmutableList<T>` (or similar); replace the whole list in `WithLines`, or use `lines.Add(item)` on the immutable list before passing to `WithLines`.
- **Value objects:** `record` / `record struct` (`Money`, `Currency`, `InvoiceNumber`); keep behavior on the type (`ToString`, operators).

```csharp
public class Invoice(
    InvoiceNumber number, string customerName, DateOnly invoicedOn,
    InvoiceStatus status, Currency currency)
{
    private Invoice(Invoice other)
        : this(other.Number, other.CustomerName, other.InvoicedOn, other.Status, other.Currency)
    {
        Id = other.Id;
        PublicId = other.PublicId;
        Lines = other.Lines;
    }

    private int Id { get; init; }
    public Guid PublicId { get; private init; } = Guid.NewGuid();

    public ImmutableList<InvoiceLine> Lines
    {
        get;
        init => field = value.All(l => l.UnitPrice.Currency == Currency) ? value
            : throw new ArgumentException("Mismatched currencies in lines.");
    } = ImmutableList<InvoiceLine>.Empty;

    public Invoice WithCustomerName(string customerName) =>
        new(this) { CustomerName = customerName };

    public Invoice WithLines(ImmutableList<InvoiceLine> lines) =>
        new(this) { Lines = lines };
}
```

### Child entities inside the aggregate

- Hide surrogate `Id` from the public surface (`private int Id { get; init; }`).
- Add a **private EF materialization constructor** when value objects need flattening for columns.
- Ignore computed properties in EF (`LineTotal`, nested `UnitPrice`); persist flattened columns or conversions.

```csharp
public record InvoiceLine(string Description, int Quantity, Money UnitPrice)
{
    private int Id { get; init; }
    public Money LineTotal => Quantity * UnitPrice;

    private decimal UnitPriceAmount { get; init; } = UnitPrice.Amount;
    private string UnitPriceCurrency { get; init; } = UnitPrice.Currency.Code;

    private InvoiceLine(string description, int quantity, decimal amount, string currencyCode)
        : this(description, quantity, new Money(amount, new Currency(currencyCode))) { }
}
```

## EF Core configuration

| Concern | Pattern |
|---------|---------|
| Surrogate key | `HasKey("Id")` on private `Id`; generated by store |
| Natural / public lookup | `HasAlternateKey(e => e.PublicId)` + non-clustered unique index |
| Value object on scalar | `HasConversion` (e.g. `Currency` ↔ `Code`, `InvoiceNumber` ↔ `"YYYY/NNN"`) |
| Enum / status | `HasConversion(v => v.ToString(), v => Enum.Parse<T>(v))` |
| Computed / rich properties | `Ignore(e => e.LineTotal)` |
| Owned value object columns | Shadow or private init properties mapped explicitly |
| Aggregate children | `HasMany(...).WithOne().OnDelete(DeleteBehavior.Cascade)` |

Expose repositories from `DbContext` with declared includes so every load returns a **complete aggregate**:

```csharp
public IImmutableEntityRepository<Invoice> Invoices =>
    Set<Invoice>().ToImmutableEntityRepository(this, "Lines");
```

`"Lines"` (and nested includes if needed) must match the aggregate graph required for domain operations.

## Repository API

From `ImmutableDomain.EntityFrameworkCore`:

| Method | Role |
|--------|------|
| `AddImmutableAsync` | Insert new aggregate |
| `FindImmutableAsync(key...)` | Load with includes; prefers alternate key when configured |
| `UpdateImmutable` | Reconcile tracked graph with immutable copy |
| `RemoveImmutable` | Delete aggregate |

### Typical use-case flow

```csharp
var invoice = new Invoice(number, "Big Joe", issuedOn, InvoiceStatus.Issued, usd)
    .WithLines([
        new InvoiceLine("Hat", 2, new Money(19.99m, usd)),
    ]);

await dbContext.Invoices.AddImmutableAsync(invoice);
await dbContext.SaveChangesAsync();

var current = await dbContext.Invoices.FindImmutableAsync(invoice.PublicId)
    ?? throw new InvalidOperationException("Invoice not found.");

var updated = current
    .WithCustomerName("Sleepy Sam")
    .WithLines(current.Lines.Add(new InvoiceLine("Cloak", 1, new Money(99.99m, usd))));

dbContext.Invoices.UpdateImmutable(updated);
await dbContext.SaveChangesAsync();
```

### UpdateImmutable behaviour (know before relying on it)

1. Locates the **currently tracked** root by stable key; throws if not tracked.
2. Detaches the old root, attaches the new copy as `Modified`.
3. Walks navigations: **collections** diffed by child primary keys (add / update / remove); **references** updated recursively.
4. New children (default key values) become `Added`; removed children are `Deleted`.

**Implications:**

- Call `UpdateImmutable` only when the aggregate is already tracked (usual request-scoped `DbContext` flow).
- Child types need **stable, non-shadow** primary keys for collection diffing.
- Deep graphs recurse via reflection — keep aggregate depth bounded.

## DbContext and lifetime

- `DbContext` stays **scoped** (per HTTP request / unit of work) — same as `dotnet-ef-core`.
- Do not share one `IImmutableEntityRepository` instance across threads; `FindImmutableAsync` reuses internal key state per repository instance.
- For read-only API responses that never update, `AsNoTracking()` queries are still valid — but updates require a tracked load first.

## Testing (required for adoption)

Immutable graph reconciliation is subtle. Before production use, add characterization tests:

| Scenario | Assert |
|----------|--------|
| Insert aggregate with children | Rows persisted; reload matches |
| Update scalar via `With*` | Single `UPDATE` on root |
| Add child line | Child `INSERT`; siblings unchanged |
| Remove child | Child `DELETE` |
| Replace all lines | Correct add/remove set |
| Alternate key lookup | `FindImmutableAsync(publicId)` works |

Use SQLite or Testcontainers — in-memory provider may miss relational cascade behaviour.

## Anti-patterns

- Public setters on aggregate roots “for EF” — defeats the purpose; use init + private copy ctor.
- Skipping includes on the repository — partial aggregates break `With*` on children.
- Calling `UpdateImmutable` on a detached copy without prior tracked load.
- Shadow-only keys on child entities in collections — library rejects them.
- Wrapping this in a thick generic repository + specification stack “because DDD” — keep orchestration in use cases / endpoints when there is a single database.
- Using `EnsureCreated()` outside throwaway demos — use migrations (`dotnet-ef-core`).

## Checklist for a new immutable aggregate

1. Model root + children with constructors, invariants, and `With*` methods.
2. Configure keys, alternate keys, conversions, ignores in `OnModelCreating`.
3. Add `ToImmutableEntityRepository(this, includes...)` on `DbContext`.
4. Implement use case: `Add` → `Find` → `With*` → `UpdateImmutable` → `SaveChanges`.
5. Add integration tests for insert, scalar update, and collection add/remove.
6. Keep presentation mapping (`ToLabel`, DTOs) outside domain types.

## Related skills

- `dotnet-ef-core` — DbContext registration, migrations, query rules, lifetimes.
- `domain-driven-design` / `implementing-domain-driven-design` — aggregate boundaries, value objects, consistency.
- `specification-pattern` — named read filters; do not leak `IQueryable` above the data layer.
- `functional-programming-csharp` — immutability, `With*` chains, expression-oriented design.

## Upstream

- Repository: https://github.com/zoran-horvat/immutable-domain-tools
- Author: Zoran Horvat (Coding Helmet) — pragmatic DDD + EF Core training content.
- Package: reference the project directly until published to NuGet; verify version against your EF Core major version.
