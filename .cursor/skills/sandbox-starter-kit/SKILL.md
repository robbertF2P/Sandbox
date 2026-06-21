---
name: sandbox-starter-kit
description: |
  Entry point for AI-assisted development in the SandBox monorepo. Use when:
  - Starting any task and unsure which conventions apply
  - Adding a new module, POC, or platform integration
  - Scoping work across HTTP, actors, EF Core, logging, or correlation
  - Reviewing whether AI output meets repo quality gates
paths:
  - "**/*"
metadata:
  version: 1.0.0
---

# SandBox AI starter kit

Use this skill first. It routes you to focused skills and repo standards — do not improvise patterns that already exist here.

## Repo map (where truth lives)

| Area | Path | Use for |
|------|------|---------|
| Actor + SignalR POC | `AkkaSignalRVuePoc/` | Akka.NET, SignalR, catalog API patterns |
| Import + progress POC | `ApiImportActorPoc/` | Import sessions, hours booking, multi-manager actors |
| Platform logging | `Platform.Serilog.Logging/`, `build/Platform.Logging.*.props` | Serilog, Seq, App Insights, test sink |
| Platform packages feed | `local-feed/` | Packed NuGet (`Platform.Serilog.Logging*`) |
| Import domain | `ImportPipeline/` | `ImportPipeline.Domain` NuGet |
| Modularization program | `docs/monolith-modularization/` | Roadmap, logging/correlation standards, quality framework |
| Agent skills (source) | `.cursor/skills/` | Author skills here; sync with `./scripts/sync-agent-skills.sh` |
| Agent entry | `AGENTS.md` | Skill index for all tools |

**POCs are reference implementations**, not production Floor2Plan behaviour. Do not cite POC code as legacy truth without labeling it `reference_only`.

## Skill router

| If you are… | Read this skill |
|-------------|-----------------|
| Modeling domains, aggregates, bounded contexts | `domain-driven-design`, `implementing-domain-driven-design` |
| Repositories, specifications, query rules | `specification-pattern` |
| C# / ASP.NET Core hosts, DI, tests | `dotnet-core-csharp-development` |
| EF Core, migrations, DbContext | `dotnet-ef-core` |
| Actors, messages, SignalR integration | `akka-net`, `reactive-applications-akka-net` |
| DSLs, fluent APIs, parsers | `domain-specific-languages` |
| Serilog, MSBuild logging props, test sink | `platform-logging` |
| CorrelationId, use-case tracing, Akka envelopes | `platform-correlation` |
| Functional style in C# | `functional-programming-csharp` |

When multiple apply, read **all** relevant skills before editing.

## Default delivery workflow

1. **Locate** — Find the nearest existing pattern in the same module (do not copy from unrelated folders).
2. **Scope** — Smallest diff that satisfies the request; one slice per PR when possible.
3. **Platform first** — Prefer `build/*.props` + NuGet over copying Serilog/bootstrap code.
4. **Implement** — Match naming, file layout, and test style of the target module.
5. **Verify** — `dotnet build` and `dotnet test` on touched projects; fix failures before finishing.
6. **Sync skills** — If you edited `.cursor/skills/`, run `./scripts/sync-agent-skills.sh`.

## Quality gates (reject slop)

From `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md`:

**Analysis / design**

- No uncited claims — use `path:line`, test names, or mark `[NEEDS REVIEW]`.
- No invented business rules or actors not evidenced in code/tests/experts.
- No generic filler that could apply to any ERP.

**Code PRs**

- Behaviour changes need tests (or explicit `known_quirk` preservation).
- No cross-context leakage (vendor types in domain, `IQueryable` past repository boundary).
- No style drift — match neighbours and workspace rules.
- Prefer focused PRs; avoid unrelated drive-by edits.

**AI role:** drafting accelerator — humans and green tests authorize merge.

## New .NET module (minimal checklist)

1. SDK-style `.csproj` + solution membership.
2. Import `build/Platform.Logging.Versions.props` in `Directory.Packages.props`.
3. Host: `Platform.Logging.Host.props` → `AddPlatformLogging` + `UsePlatformCorrelationPipeline` + `UsePlatformRequestLogging`.
4. Library: `Platform.Logging.Library.props`; tests: `Platform.Logging.Tests.props`.
5. Actor modules: also `Platform.Logging.Akka.props` on API + Core.
6. Scaffold helper: `./scripts/add-platform-logging-to-module.sh` (when applicable).
7. Pack platform packages after API changes: `./scripts/pack-platform-logging.sh [version]`.

## Common commands

```bash
./scripts/sync-agent-skills.sh
./scripts/pack-platform-logging.sh 1.1.0
dotnet build <path/to/project.csproj>
dotnet test <path/to/tests.csproj>
```

## When stuck

- Logging sinks wrong? → `platform-logging`
- Ask timeout / lost `Sender` in async actor? → `platform-correlation` (inline envelope dispatch)
- Repository leaking EF? → `specification-pattern`
- Unsure aggregate boundaries? → `implementing-domain-driven-design`
