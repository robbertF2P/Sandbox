# Agent instructions

Shared guidance for AI coding agents (Cursor, GitHub Copilot, Claude Code, and others).

## Start here

**New to this repo?** Read the **`sandbox-starter-kit`** skill first (`.cursor/skills/sandbox-starter-kit/SKILL.md`) or [docs/ai-starter-kit.md](../docs/ai-starter-kit.md).

## Skills layout

| Path | Used by |
|------|---------|
| `.cursor/skills/` | Cursor (source of truth) |
| `.github/skills/` | GitHub Copilot |
| `.agents/skills/` | Cross-tool Agent Skills convention |

Each skill is a folder with a `SKILL.md` file (YAML frontmatter + markdown body).

After editing skills in `.cursor/skills/`, run:

```bash
./scripts/sync-agent-skills.sh
```

## Starter kit (read first)

| Skill | Purpose |
|-------|---------|
| `sandbox-starter-kit` | Repo map, skill router, delivery workflow, quality gates |
| `platform-logging` | `Platform.Serilog.Logging` NuGet + MSBuild props |
| `platform-correlation` | CorrelationId / UseCase across HTTP, Akka, events, SignalR |

## Available skills

### SandBox platform

- `sandbox-starter-kit` — entry point for AI-assisted work in this monorepo
- `platform-logging` — central Serilog (Seq, App Insights, xUnit sink)
- `platform-correlation` — end-to-end use-case tracing

### Domain & architecture

- `domain-driven-design` — Evans DDD foundations
- `implementing-domain-driven-design` — Vernon DDD implementation
- `domain-specific-languages` — Fowler DSL design and implementation
- `specification-pattern` — Specification + Repository; Ardalis.Specification; avoid IQueryable leakage

### .NET

- `dotnet-core-csharp-development` — C# / ASP.NET Core conventions
- `dotnet-ef-core` — Entity Framework Core
- `immutable-domain-ef-core` — immutable aggregates with EF Core (With* helpers, graph reconciliation)
- `akka-net` — Akka.NET (repo patterns)
- `reactive-applications-akka-net` — Anthony Brown reactive systems / Akka.NET book
- `functional-programming-csharp` — functional patterns in C#

Copilot-specific always-on notes: `.github/copilot-instructions.md`

## Platform 2.0 module composition (F2P refactor)

When working on **monolith modularization** or **new bounded-context modules** (in the **external F2P repo** with Copilot or Claude Code):

- **No ABP** in new extracted modules — no `Volo.Abp.*`, `AbpModule`, or `AbpDbContext`.
- Register modules with **`IServiceCollection` extension methods** (`Add<Context>Module`, layer-specific `Add*` helpers).
- Map endpoints with **`WebApplication` extensions** (`Map<Context>Endpoints`).
- Host (`Program.cs`) is the **only composition root** — explicit, grep-able service registration.
- Bridge legacy ABP via **`[StranglerAdapter]`** in Infrastructure; do not extend `AbpModule` for new code.
- **V2 frontend:** **`@floorganise/css`** (Tailwind + Floorganise tokens) on every frontend module; shared widgets from **`@floorganise/ui`** — no per-module design systems.
- **Workflow orchestration:** Akka.NET actor pipelines for integrations, tenant customization packs, and legacy strangler bridges — see `platform-actor-standard.md`.

Details: `docs/monolith-modularization/module-composition-di.md` · Plan: `docs/monolith-modularization/foundation-and-pilot-plan.md` · Frontend: `docs/monolith-modularization/platform-frontend-standard.md` · Actors: `docs/monolith-modularization/platform-actor-standard.md` · Auth: `docs/monolith-modularization/platform-authentication-standard.md` · Agent rules: `docs/monolith-modularization/agent-instructions-snippet.md` (copy to monolith as `agent-rules.md` + `.github/copilot-instructions.md`).

**SandBox** uses Cursor for brainstorming and skill authoring (`.cursor/skills/`). The monolith does not need Cursor — use the agent snippet for Copilot and Claude.
