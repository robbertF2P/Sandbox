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
- `akka-net` — Akka.NET

Copilot-specific always-on notes: `.github/copilot-instructions.md`
