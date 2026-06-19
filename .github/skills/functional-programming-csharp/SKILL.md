---
name: functional-programming-csharp
description: |
  Guides functional programming in C# following Enrico Buonanno's "Functional Programming in C#"
  (Manning): purity, function signatures, Option/Either, Map/Bind/Where, composition, validation,
  immutability, and effectful types. Use when:
  - Refactoring imperative C# toward a functional style
  - Designing function signatures, DTOs, or validation/error-handling flows
  - Choosing between null, Option, Either, exceptions, or Result types
  - Applying Map/Bind/composition patterns with LINQ or custom types
  - Reviewing code for side effects, mutability, or testability issues
  - Explaining or implementing FP patterns in C# (including C# 10+ records, Func/Action, LINQ)
paths:
  - "**/*.cs"
  - "**/*.csproj"
metadata:
  version: 1.0.0
  source: "Enrico Buonanno — Functional Programming in C# (Manning)"
---

# Functional Programming in C#

Apply this skill when writing or refactoring C# using functional techniques. The mental model comes from **Enrico Buonanno, *Functional Programming in C#*** (Manning): functions first, minimize mutation, make effects explicit in types.

Match existing project conventions (see `dotnet-core-csharp-development` skill for layout and style). This repo does not require the `LaYumba.Functional` NuGet package — patterns below use idiomatic C# unless a project already references that library.

## Core principles (apply in order)

1. **Functions as values** — pass, return, and store `Func<>` / `Action<>`; prefer small composable functions over large methods.
2. **Avoid state mutation** — prefer immutable records, `init` properties, and returning new values instead of updating shared state.
3. **Purity where it matters** — isolate I/O, logging, DB, clocks, and randomness at the edges; keep domain logic referentially transparent.
4. **Honest signatures** — types should express valid inputs, optional data, and possible failure; avoid `void`, `null`, and exceptions for expected outcomes.
5. **Composition over statements** — build workflows by chaining `Map` / `Bind` / `Where` / LINQ rather than nested `if` and early `return` soup.

## Function purity

A **pure** function:
- Returns a value determined **only** by its inputs.
- Causes **no side effects** (no mutation of external/global state, no I/O, no logging inside domain logic).

| Pure | Impure |
|------|--------|
| `int Triple(int x) => x * 3` | Writes to DB, console, or static field |
| Computes from arguments | Reads `DateTime.Now`, config, or instance fields |
| Returns new data | Mutates input arguments or shared collections |

**Why purity matters:** pure code parallelizes safely, tests without mocks, and refactors predictably.

### Reducing impure footprint

```
HTTP / DB / File / Clock / Random  →  thin impure shell (adapters)
                                              ↓
                              pure domain functions (validation, mapping, rules)
```

- Push side effects to the **boundary** (controllers, repositories, `Main`/host).
- Pass dependencies as **arguments** instead of reading globals or hidden state.
- Prefer **parameterized tests** on pure functions over heavy mocking.

## Signature and type design

Use **arrow notation** when reasoning: `Validate : string → Option<Email>` means input `string`, output `Option<Email>`.

| Arrow | C# type |
|-------|---------|
| `A → B` | `Func<A, B>` |
| `() → B` | `Func<B>` |
| `A → ()` | `Action<A>` |
| `(A, B) → C` | `Func<A, B, C>` |

### Design rules

- Replace **primitive obsession** with small types (`Email`, `PersonName`, `Hours`, `WbsCode`) — the type documents valid inputs.
- Write **honest functions**: if input can be invalid, the return type must say so (`Option`, `Either`, `Validation`).
- Avoid **`null` for business meaning** — use `Option`/nullable only at boundaries; map to domain types quickly.
- Prefer **`Func` over `Action`** when a computation produces a value; use a dedicated `Unit` or `bool` only when truly no result exists.

### Option (absence / partial success)

Use when failure carries **no error detail** (missing data, invalid parse, not found):

```csharp
public readonly record struct Option<T>(T? Value, bool IsSome)
{
    public static Option<T> Some(T value) => new(value, true);
    public static Option<T> None() => new(default, false);

    public Option<R> Map<R>(Func<T, R> f) =>
        IsSome ? Option<R>.Some(f(Value!)) : Option<R>.None();

    public Option<R> Bind<R>(Func<T, Option<R>> f) =>
        IsSome ? f(Value!) : Option<R>.None();
}
```

Prefer `Option` over exceptions for **expected** failures (parse, lookup, validation without messages).

### Either / Validation (failure with detail)

Use when callers need **why** something failed:

- **`Either<L, R>`** — left = error, right = success (railway-oriented flow).
- **Validation** — accumulate **all** field errors (applicative); use **Bind** chain when failing fast on first error.

| Scenario | Type |
|----------|------|
| Parse/lookup, no error message | `Option<T>` |
| Single error, stop on first failure | `Either<Error, T>` + `Bind` |
| Form/API validation, collect all errors | `Validation<Error, T>` / `Either<IEnumerable<Error>, T>` |

Do **not** use exceptions for business validation. Reserve exceptions for **exceptional** infrastructure faults.

## Core FP patterns (Map, Bind, Where, ForEach)

These are the **same pattern** applied to different structures (`IEnumerable`, `Option`, `Task`, custom types):

| Function | LINQ name | Purpose |
|----------|-----------|---------|
| **Map** | `Select` | Transform inner value(s): `(F<T>, T→R) → F<R>` |
| **Bind** | `SelectMany` | Chain effectful steps: `(F<T>, T→F<R>) → F<R>` |
| **Where** | `Where` | Filter by predicate |
| **ForEach** | `ToList` + loop | Side effect per element (use sparingly at boundaries) |

**Functor** = type with `Map`. **Monad** = type with `Bind` (and `Return`/`Some`).

### When to use Map vs Bind

- **Map** — function returns a plain value: `T → R`
- **Bind** — function returns wrapped value: `T → Option<R>` / `T → Task<R>` / `T → Either<E,R>`

```csharp
// Map: parse one field
Option<int> age = ParseInt(rawAge).Map(n => n + 1);

// Bind: chain steps that may fail
Option<Customer> customer = ParseId(rawId)
    .Bind(id => FindCustomer(id))
    .Bind(c => ValidateActive(c));
```

### LINQ as monad syntax

For `IEnumerable`, `Option`-like types, or libraries supporting `SelectMany`, prefer query syntax for multi-step flows:

```csharp
from id in ParseId(rawId)
from customer in FindCustomer(id)
from order in LoadLatestOrder(customer)
select order.Total;
```

## Function composition and workflows

Compose small functions **right-to-left** (mathematical) or chain left-to-right (LINQ/method chaining):

```csharp
Func<string, Option<ActivityImportDto>> pipeline =
    StripAndNormalize
        .Then(ParseRow)
        .Then(ValidateBusinessRules)
        .Then(ToImportDto);
```

Workflow design checklist:
1. Parse raw input → typed values (`Option`/`Either`).
2. Validate with pure rules.
3. Map to domain/DTO.
4. Persist / respond at the **impure edge** only.

Prefer **expressions** over statements; prefer **declarative** data flow over nested imperative branches.

## Immutability

- Use **`record`** / **`record struct`** for DTOs and value objects.
- **`init`** or constructor-only properties; no public setters on domain types.
- Never mutate collections exposed to callers — return new lists/records.
- When change is needed, use **copy-and-update** (`with` for records).

## Error handling decision tree

```
Expected failure?
├─ No message needed → Option<T>
├─ One error, fail fast → Either<Error, T> + Bind
├─ Multiple errors to collect → Validation / applicative Traverse
└─ Infrastructure fault (network down) → exception at boundary only
```

Reporting pattern (aligns with collect-don't-throw readers): return **success values + structured issues**, don't abort entire batch on first bad row.

## Async and effects

- **`Task<T>`** is an effectful type — use `Bind`-style `await` chains or `SelectMany` patterns.
- **`Traverse`** — map async/validation over lists; choose monadic (fail fast) vs applicative (collect errors).
- Avoid **stacked monads** (`Task<Option<T>>`) in public APIs — normalize at boundaries.

## Concurrency

- Pure functions → safe parallel `AsParallel` / `Task.WhenAll`.
- Impure shared state → isolate behind agents/actors, channels, or immutable message passing — not locks scattered in domain code.

## Refactoring checklist

When touching imperative code, ask:

1. Can this function be **pure**? What context does it secretly read?
2. Does the signature **lie** about null, exceptions, or invalid inputs?
3. Can `if/throw` become **`Option`/`Either`**?
4. Is this a **Map** (transform) or **Bind** (chain possible failure)?
5. Are DTOs **immutable** records?
6. Is I/O pushed to the **outermost layer**?

## Code smells → FP fix

| Smell | Fix |
|-------|-----|
| `null` checks everywhere | `Option` / honest types at boundary |
| `try/catch` for validation | `Either` / `Validation` |
| Static mutable state | Pass state explicitly or isolate in agent |
| 200-line method | Compose pipeline of small functions |
| `out` parameters | Return `Option` or tuple |
| Boolean flags as parameters | Separate functions or discriminated union |

## Book chapter map (for deeper lookup)

| Topic | Chapters |
|-------|----------|
| FP basics, HOFs | 1 |
| Purity, testing | 2 |
| Signatures, Option, Unit | 3 |
| Map, Bind, functors, monads | 4–5 |
| Either, validation | 6–8 |
| Immutability, data structures | 9 |
| Event sourcing | 10 |
| Lazy, Try, middleware | 11 |
| Stateful computations | 12 |
| Async, Traverse | 13 |
| Reactive (IObservable) | 14 |
| Agents / message passing | 15 |

## Related skills

- `dotnet-core-csharp-development` — project layout, build, ASP.NET Core conventions.
- `dotnet-ef-core` — persistence (keep repositories impure, domain pure).
- `akka-net` — actor-based concurrency at the system boundary.

## Anti-patterns

- Using FP jargon without simplifying types/signatures.
- Making every function `Task<Either<List<Error>, Option<T>>>` — normalize effects.
- Pure functions that log, hit DB, or read `HttpContext`.
- Throwing `ValidationException` for expected user input errors.
- Mutating DTOs after validation/parsing.
