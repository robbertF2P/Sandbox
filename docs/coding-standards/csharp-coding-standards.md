# C# coding standards

Standards for **new and refactored** C# in SandBox and Platform 2.0 modules. Baseline: **ReSharper/Rider defaults**, aligned with **Microsoft .NET conventions**, enforced via **`.editorconfig`** and analyzers.

> **Principle:** Let the IDE and CI enforce formatting and naming. Reserve code review for design, correctness, and domain boundaries — not brace placement.

## Adopted sources

| Source | Role |
|--------|------|
| [ReSharper syntax & naming](https://www.jetbrains.com/help/resharper/Code_Syntax_Style.html) | Default style baseline (var, braces, qualification, modifier order) |
| [ReSharper EditorConfig](https://www.jetbrains.com/help/resharper/Using_EditorConfig.html) | Machine-readable rules shared across Rider, VS, VS Code |
| [Microsoft C# conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) | Language usage, async, exceptions, layout |
| [dotnet/docs .editorconfig](https://github.com/dotnet/docs/blob/main/.editorconfig) | Analyzer severity reference |
| [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/) | Public API surface for shared libraries |

## Tooling setup

### Rider / ReSharper

1. Open solution — root `.editorconfig` loads automatically.
2. **Settings → Editor → Code Style → C#** — confirm “Read settings from EditorConfig”.
3. Enable **Solution-wide analysis**; treat naming/style warnings seriously.
4. Use **Reformat Code** (Ctrl+Alt+L) and **Cleanup Code** before PR.

### Visual Studio

1. **Tools → Options → Text Editor → C# → Code Style** — EditorConfig overrides IDE settings.
2. Run **Analyze → Run Code Analysis** on build (see `Directory.Build.props` recommendation below).

### CI (recommended)

```bash
dotnet format --verify-no-changes --verbosity diagnostic
```

Fail the build on style violations; do not rely on reviewers to catch formatting.

### Directory.Build.props (team template)

Add to solution roots when ready to enforce at build time:

```xml
<PropertyGroup>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  <AnalysisLevel>latest</AnalysisLevel>
  <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
</PropertyGroup>
```

Start with `suggestion`/`warning` severities in `.editorconfig`; tighten to `error` once the codebase is clean.

---

## File and type structure

| Rule | Detail |
|------|--------|
| **One type per file** | Every `class`, `record`, `struct`, `enum`, and `interface` gets its own file. File name **must** match the type name (`OrderService.cs` → `OrderService`). |
| No type barrels | Do **not** group related types in `Identifiers.cs`, `Types.cs`, `Models.cs`, or similar catch-all files — even for small value objects or enums. |
| Exceptions (rare) | Nested private types used only by the parent type; generated code. Never stack multiple public types in one file. |
| File-scoped namespaces | `namespace Floorganise.Planning.Application;` |
| Member order | Constants → fields → constructors → public → protected → private |
| `sealed` by default | Unless the type is designed for inheritance |
| `readonly` fields | When assigned only in declaration or constructor |
| Primary constructors | Use when they simplify records/DTOs; avoid hiding validation logic |

### Anti-patterns

```csharp
// ❌ Bad — multiple public types in Identifiers.cs
public readonly record struct TaskId(Guid Value) { }
public readonly record struct AssignmentId(Guid Value) { }

// ✅ Good — TaskId.cs and AssignmentId.cs
```

## Naming (ReSharper-aligned)

| Element | Convention | Example |
|---------|------------|---------|
| Types, methods, properties, events | `PascalCase` | `OrderService`, `GetByIdAsync` |
| Interfaces | `I` + `PascalCase` | `IOrderRepository` |
| Private instance/static fields | `_camelCase` | `_logger`, `_cache` |
| Local variables, parameters | `camelCase` | `orderId`, `cancellationToken` |
| Async methods | suffix `Async` | `SaveChangesAsync` |
| Constants | `PascalCase` | `DefaultPageSize` |
| Test methods | descriptive `PascalCase_With_Underscores` | `SaveOrder_WhenValid_PersistsToDatabase` |

Do **not** qualify with `this.` or type name unless resolving ambiguity.

## Usings and types

- Sort usings: `System*` first, then third-party, then project namespaces.
- Remove unused usings (`IDE0005` = warning).
- Prefer **language keywords** for built-in types: `string`, `int`, not `String`, `Int32`.
- **`var`**: use when the type is apparent from the right-hand side; use explicit types when it improves clarity for readers (public APIs, non-obvious generics).

## Control flow and syntax

| Rule | Setting |
|------|---------|
| Braces | Always (`csharp_prefer_braces = true`) |
| Expression-bodied members | Single-line methods/properties/accessors only |
| `using` declarations | Prefer simple `using` statements for IDisposable |
| Pattern matching | Prefer `is` patterns over cast checks |
| Null checks | Prefer modern null patterns over legacy `== null` chains where clear |

## Async and exceptions

- Use `async`/`await` for I/O-bound work; pass `CancellationToken` through public APIs.
- Avoid `async void` except event handlers.
- Catch **specific** exceptions; never swallow `Exception` without logging and rethrow/filter.
- Do not use `.Result` or `.Wait()` on tasks in ASP.NET Core request paths.

## ASP.NET Core (Platform 2.0)

| Topic | Convention |
|-------|------------|
| Hosting | `WebApplication` minimal hosting model |
| Endpoints | Static `*Endpoints` classes; `MapGroup` for route prefixes |
| DI | Constructor injection; extension methods `Add<Context>Module` |
| Configuration | `IOptions<T>` / `IConfiguration`; no hardcoded secrets |
| Responses | `TypedResults` / `IResult`; correct status codes |
| Logging | `Platform.Serilog.Logging` MSBuild props; structured logging |
| Correlation | `Platform.Correlation` patterns for HTTP → actors → events |

**No ABP** in new extracted modules — see `docs/monolith-modularization/module-composition-di.md`.

## Domain and architecture

- **Domain layer**: no EF, HTTP, or UI references.
- **Repositories**: return domain types; no `IQueryable` leakage past the repository boundary (`specification-pattern` skill).
- **Immutable aggregates**: `With*` helpers + graph reconciliation (`immutable-domain-ef-core` skill).
- **Actors**: messages as `record` types in Contracts; `Tell` not `Ask` inside actors (`akka-net` skill).

## Testing

| Topic | Convention |
|-------|------------|
| Framework | xUnit |
| Naming | `MethodName_Scenario_Expected` |
| Async tests | `async Task` (suffix `Async` optional in tests) |
| Assertions | FluentAssertions or clear `Assert.*` — match neighbouring project |
| Test data | Builders/factories; no magic strings without named constants |
| Database tests | Fresh DB per test when state mutates; see `ActorDatabaseTestBase` pattern |

## Pull request checklist

- [ ] `dotnet build` and `dotnet test` pass for touched solutions
- [ ] No unused usings or obvious ReSharper/Rider warnings left
- [ ] Public APIs documented when behaviour is non-obvious
- [ ] No secrets or environment-specific URLs in source
- [ ] Platform logging/correlation props used for new hosts

## What we do not debate in review

- Spaces vs tabs (spaces, 4 for C#)
- `_field` prefix (yes, for private fields)
- File-scoped namespaces (yes)
- `Async` suffix (yes, for async methods)
- Brace style (always use braces)

Escalate to team lead only when a **project-wide** exception is needed; document it in this file or a local `.editorconfig` override.

## Repo references

| Path | Purpose |
|------|---------|
| `.editorconfig` | Enforced style rules |
| `AkkaSignalRVuePoc/.cursor/rules/csharp-resharper-style.mdc` | Akka-specific additions |
| `.cursor/skills/dotnet-core-csharp-development/SKILL.md` | Agent skill |
| `build/Platform.Logging.*.props` | Logging MSBuild integration |
