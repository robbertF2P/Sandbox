---
name: csharp-enum-best-practices
description: |
  Guides C# enum design and evolution: when to use enums vs bool vs result types, decoupling
  consumers from enum values via behavior, extension methods, and promoting to smart classes.
  Based on Zoran Horvat (Coding Helmet). Use when:
  - Choosing between bool, enum, record, or class for method return values or status codes
  - Refactoring comparisons like `if (status == SomeEnum.Success)` in consuming code
  - Adding error codes, messages, or retry semantics to operation results
  - Designing domain enums (filters, lifecycle, approval state) or API serialization of enums
  - Reviewing whether an enum should stay closed or evolve into a richer type
paths:
  - "**/*.cs"
metadata:
  version: 1.0.0
  source: "Zoran Horvat — Best Practices Using Enums in .NET (Coding Helmet)"
  source_url: "https://codinghelmet.com/articles/enum-best-practices"
---

# C# enum best practices

Apply when modeling **status**, **outcomes**, or **closed categorical values** in C#. Source: [Best Practices Using Enums in .NET](https://codinghelmet.com/articles/enum-best-practices) (Zoran Horvat, Coding Helmet).

Pair with `dotnet-core-csharp-development` for style and `domain-driven-design` for where enums belong in the domain layer.

## Core rule

| Return type | Use when |
|-------------|----------|
| `bool` | Method answers a **yes/no question** (`IsRunning`, `Contains`, `CanExecute`). |
| `enum` / result type | Method **performs work** and reports outcome (success, failure kind, retry hint). |
| Rich class / record | Outcome carries **metadata** (message, code, correlation) beyond a single integer. |

**Avoid** `bool` for operation results — callers cannot distinguish *why* something failed or whether to retry.

## Evolution path (Horvat)

Design for change in this order:

```text
bool  →  enum (+ extension behavior)  →  smart class (static instances)
```

1. **Enum** — typed error/status codes with meaningful names (`Success`, `AccessDenied`, `DatabaseNotAvailable`).
2. **Extension methods** — expose *behavior* (`IsSuccess()`, `GetErrorCode()`) so consumers do not depend on specific members.
3. **Promote to class** — when you need non-integer data (user-facing `ErrorMessage`, structured details). Replace enum with a class using `public static readonly` instances; keep the same method names so callers stay stable.

## Do / don't for consuming code

### Don't bind to enum values (open-ended sets)

```csharp
// Bad — breaks when new members are added or semantics change
if (status == OperationResult.Success)
    break;
```

### Do bind to behavior

```csharp
// Good — semantics live with the type
if (status.IsSuccess())
    break;

var code = status.GetErrorCode();
```

Centralize meaning in **one place** (extensions today, class methods after promotion):

```csharp
public static class OperationResultExtensions
{
    public static bool IsSuccess(this OperationResult res) =>
        res is OperationResult.Success or OperationResult.Warning;

    public static int GetErrorCode(this OperationResult res) => (int)res;
}
```

When `Warning` becomes a success, only `IsSuccess` changes — **client code stays untouched**.

### When comparing values is OK (closed, exhaustive sets)

Bind directly only when the set is **fixed by domain** and unlikely to grow:

- `Read` / `Write` / `Bidirectional`
- `TenantDeploymentMode` with explicit migration mapping
- Internal `switch` with `_ => throw` for exhaustiveness (see `HourApprovalRules.MatchesFilter` in HourApprovals)

```csharp
public static bool MatchesFilter(TaskApprovalState state, ApprovalFilterStatus filter) =>
    filter switch
    {
        ApprovalFilterStatus.All => true,
        ApprovalFilterStatus.Approved => state == TaskApprovalState.Approved,
        ApprovalFilterStatus.NotApproved => state == TaskApprovalState.NotApproved,
        _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, "Unsupported filter."),
    };
```

## SandBox / F2P conventions

| Pattern | Example in repo | Guidance |
|---------|-----------------|----------|
| Domain enum | `TaskApprovalState`, `ApprovalFilterStatus` | Keep in `*.Domain/Enums/`; behavior in `Rules/` static classes. |
| Contract enum | `ApprovalState`, `SubmissionCategory` | Host/module message contracts; map from domain at boundaries. |
| Shared preset enum | `TimeRangePreset` | Prefer enum over magic strings (`"since_last_submission"`). |
| API exposure | `approvalState.ToString()` in minimal APIs | Serialize stable names; parse defensively at the HTTP edge. |

**Boundary rule:** domain enums must not leak vendor/host types inward; map `TaskApprovalState` → `ApprovalState` in facades/mappers (see `HourSubmissionSnapshotMapper`).

## Implementation checklist

When introducing a new status/outcome type:

1. **Name for intent** — `OperationResult`, `SubmissionCategory`, not `int` or `bool`.
2. **Explicit values** — assign underlying integers when codes matter for logging, legacy interop, or DB storage.
3. **Behavior next to type** — extensions or static rules class; never scatter `== SomeEnum.X` across modules.
4. **Exhaustive switches** — use `switch` expressions; `throw` on `_` for internal enums you own.
5. **Promote when needed** — add `ErrorMessage` or structured payload → static class with readonly instances.
6. **Tests** — cover behavior methods (`IsSuccess`, filter matching), not only enum member existence.

## Modern C# notes (beyond the 2013 article)

- **Records** — use for immutable outcome *data* (`SubmitTaskFailure`); keep enums for fixed member sets.
- **`Result<T>` / `OneOf`** — consider for composable error pipelines when behavior grows beyond one enum.
- **JSON** — `JsonStringEnumConverter` for public APIs if string names are the contract; document allowed values.
- **`[Flags]`** — only for true bitwise combinations; do not use for mutually exclusive statuses.
- **Do not** encode business rules only in enum member names — encode in domain rules (`HourApprovalRules.ResolveState`).

## Anti-patterns (reject in review)

- `bool` return from a command/service that can fail in multiple ways.
- `string` status in domain (`"Approved"`) when an enum or value object exists.
- Duplicated `isApproved` bool **and** `approvalState` enum without a single resolver.
- Switching on enums in UI, API, *and* domain with different semantics — pick one authority.
- Adding enum members without updating behavior helpers or exhaustiveness tests.

## Quick decision tree

```text
Is it a yes/no property of current state?
  yes → bool
  no ↓
Is the set fixed and exhaustive forever?
  yes → enum; direct compare OK inside one module + exhaustive switch
  no ↓
Will consumers need success/failure/retry without knowing codes?
  yes → enum + IsSuccess()/similar extensions
  no ↓
Need messages, metadata, or variable structure?
  yes → promote to class/record with static well-known instances
```

## References

- [Best Practices Using Enums in .NET](https://codinghelmet.com/articles/enum-best-practices) — Zoran Horvat, Coding Helmet
- Repo: `HourApprovals.Domain/Rules/HourApprovalRules.cs` — filter matching with exhaustive switch
- Repo: `F2pPlatform.Host.Contracts/ApprovalQueue/ApprovalState.cs` — contract-level approval enum
- Skill: `dotnet-core-csharp-development` — file layout and C# style
