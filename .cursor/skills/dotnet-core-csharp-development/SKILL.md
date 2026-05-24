---
name: dotnet-core-csharp-development
description: |
  Guides .NET / ASP.NET Core C# development: project layout, SDK-style projects, minimal APIs,
  dependency injection, configuration, testing, and build commands. Use when:
  - Writing or refactoring C# or .csproj files
  - Working on ASP.NET Core APIs, services, or hosts
  - Running dotnet build, test, or run
  - Adding endpoints, DI registrations, or configuration
  - Debugging compile errors, analyzers, or test failures in .NET solutions
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/*.sln"
  - "**/*.slnx"
  - "**/Directory.Build.props"
  - "**/global.json"
metadata:
  version: 1.0.0
---

# .NET Core C# Development

Apply this skill for C# and ASP.NET Core work in this repository. Prefer existing project conventions over generic templates.

## Before changing code

1. Identify the solution and project (e.g. `AkkaSignalRVuePoc/AkkaSignalRVuePoc.slnx`).
2. Read nearby types and `.csproj` references before adding dependencies.
3. Match style from `AkkaSignalRVuePoc/.cursor/rules/csharp-resharper-style.mdc` when editing files under that solution.

## Project and solution layout

- Use SDK-style projects (`<Project Sdk="Microsoft.NET.Sdk">` or Web SDK).
- Shared MSBuild settings belong in `Directory.Build.props` at the solution root.
- One public type per file; file name matches the type name.
- Use file-scoped namespaces.
- Prefer `sealed` on classes not designed for inheritance.

## C# style (ReSharper-aligned)

| Area | Convention |
|------|------------|
| Types, members | `PascalCase` |
| Private fields | `_camelCase` |
| Async methods | Suffix `Async` |
| Control flow | Always use braces |
| `var` | Use when the type is apparent |
| Usings | `System*` first, then third-party, then project; remove unused |

## ASP.NET Core patterns

### Hosting and startup

- Use `WebApplication.CreateBuilder` / `WebApplication` (minimal hosting model).
- Configure Serilog on the host when the project already does (see `Program.cs` in `AkkaSignalRVuePoc.Api`).
- Register services in `builder.Services`; map endpoints after `var app = builder.Build()`.

### API surface

- Prefer minimal APIs grouped in static `*Endpoints` classes (e.g. `MessageEndpoints`, `ProjectEndpoints`).
- Keep request/response DTOs in a `Models` folder as simple records or classes.
- Use `IResult` / `TypedResults` for HTTP responses; return appropriate status codes.

### Dependency injection

- Register interfaces with explicit lifetimes: `Singleton` for caches and facades, `Scoped` for per-request state.
- Avoid service locator; inject dependencies via constructors.
- Extension methods like `AddAkkaActors` keep registration cohesive.

### Configuration

- Bind options from `IConfiguration` sections (`GetSection`, `GetValue`, or `IOptions<T>`).
- Respect environment-specific `appsettings.{Environment}.json`.
- Do not hardcode secrets; use user secrets, environment variables, or secret stores.

## Build and test commands

From the solution directory (example: `AkkaSignalRVuePoc`):

```bash
dotnet build AkkaSignalRVuePoc.slnx
dotnet test AkkaSignalRVuePoc.slnx --logger "console;verbosity=detailed"
dotnet run --project server/AkkaSignalRVuePoc.Api/AkkaSignalRVuePoc.Api.csproj
```

For a single project:

```bash
dotnet build path/to/Project.csproj
dotnet test path/to/Tests.csproj
```

## Testing

- xUnit is the test framework in `AkkaSignalRVuePoc` (xUnit v3).
- Integration tests: `WebApplicationFactory` pattern under `tests/.../Integration`.
- Unit tests: mock external IO; assert behavior, not implementation details.
- Test method names: descriptive `PascalCase_with_underscores`.

## Analyzers and quality

- `Directory.Build.props` may set `EnforceCodeStyleInBuild` and `AnalysisLevel`; fix analyzer warnings in touched code.
- Run `dotnet build` after substantive edits; run `dotnet test` when behavior changes.

## Common pitfalls

- Mixing sync-over-async (`Task.Result`, `.Wait()`) in ASP.NET request paths.
- Capturing `DbContext` or scoped services in singletons.
- Forgetting CORS or SignalR hub paths when adding browser clients.
- Adding package references without checking `NuGet.config` or central package management if present.

## Related workspace rules

- `AkkaSignalRVuePoc/.cursor/rules/csharp-resharper-style.mdc` — detailed C# and test conventions.
- `.cursor/rules/terse-agent-communication.mdc` — keep agent responses concise.

For actor systems and Akka.NET-specific work, use the `akka-net` skill instead.
