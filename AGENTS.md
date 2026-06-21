# Agent instructions

Shared guidance for AI coding agents (Cursor, GitHub Copilot, Claude Code, and others).

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

## Available skills

- `domain-driven-design` — Evans DDD foundations
- `implementing-domain-driven-design` — Vernon DDD implementation
- `domain-specific-languages` — Fowler DSL design and implementation
- `specification-pattern` — Specification + Repository; Ardalis.Specification; avoid IQueryable leakage
- `dotnet-core-csharp-development` — C# / ASP.NET Core conventions
- `dotnet-ef-core` — Entity Framework Core
- `akka-net` — Akka.NET (repo patterns)
- `reactive-applications-akka-net` — Anthony Brown reactive systems / Akka.NET book

Copilot-specific always-on notes: `.github/copilot-instructions.md`

## Platform 2.0 module composition (F2P refactor)

When working on **monolith modularization** or **new bounded-context modules** (in the **external F2P repo** with Copilot or Claude Code):

- **No ABP** in new extracted modules — no `Volo.Abp.*`, `AbpModule`, or `AbpDbContext`.
- Register modules with **`IServiceCollection` extension methods** (`Add<Context>Module`, layer-specific `Add*` helpers).
- Map endpoints with **`WebApplication` extensions** (`Map<Context>Endpoints`).
- Host (`Program.cs`) is the **only composition root** — explicit, grep-able service registration.
- Bridge legacy ABP via **`[StranglerAdapter]`** in Infrastructure; do not extend `AbpModule` for new code.

Details: `docs/monolith-modularization/module-composition-di.md` · Plan: `docs/monolith-modularization/foundation-and-pilot-plan.md` · Agent rules: `docs/monolith-modularization/agent-instructions-snippet.md` (copy to monolith as `agent-rules.md` + `.github/copilot-instructions.md`).

**SandBox** uses Cursor for brainstorming and skill authoring (`.cursor/skills/`). The monolith does not need Cursor — use the agent snippet for Copilot and Claude.
