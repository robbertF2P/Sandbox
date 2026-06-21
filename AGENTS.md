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
- `akka-net` — Akka.NET (repo patterns)
- `reactive-applications-akka-net` — Anthony Brown reactive systems / Akka.NET book
- `functional-programming-csharp` — functional patterns in C#

Copilot-specific always-on notes: `.github/copilot-instructions.md`
