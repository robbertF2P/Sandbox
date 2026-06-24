# AI starter kit

Quick entry for humans and agents working in the SandBox monorepo.

## Start here

| Audience | Entry |
|----------|--------|
| **AI agents** | Skill: `.cursor/skills/sandbox-starter-kit/SKILL.md` |
| **All tools** | `AGENTS.md` (skill index + sync instructions) |
| **Copilot** | `.github/copilot-instructions.md` |
| **Quality bar** | `docs/monolith-modularization/ai-assisted-delivery-quality-framework.md` |

After editing skills in `.cursor/skills/`:

```bash
./scripts/sync-agent-skills.sh
```

## Starter skill pack

### Always consider first

| Skill | When |
|-------|------|
| `sandbox-starter-kit` | Any task — repo map, workflow, skill router |
| `platform-logging` | Serilog, MSBuild props, Seq / App Insights |
| `platform-correlation` | Tracing use cases HTTP → actors → events → SignalR |

### Domain & architecture

| Skill | When |
|-------|------|
| `domain-driven-design` | Strategic/tactical DDD (Evans) |
| `implementing-domain-driven-design` | Aggregates, events, CQRS (Vernon) |
| `specification-pattern` | Repositories, specifications, no `IQueryable` leakage |
| `domain-specific-languages` | DSLs, fluent APIs |

### .NET implementation

| Skill | When |
|-------|------|
| `dotnet-core-csharp-development` | C#, ASP.NET Core, DI, tests |
| `dotnet-ef-core` | EF Core, migrations |
| `immutable-domain-ef-core` | Immutable aggregates + EF Core persistence |
| `akka-net` | Actors, messages, hosting (see POCs) |
| `reactive-applications-akka-net` | Reactive systems patterns |
| `functional-programming-csharp` | FP style in C# |

## Reference implementations

- **Akka + SignalR + Vue:** `AkkaSignalRVuePoc/`
- **Import + hours progress:** `ApiImportActorPoc/`
- **Planning adjustment approvals (foreman):** `PlanningApprovalsPoc/` — see [floor2plan-planning-approval-data-model.md](floor2plan-planning-approval-data-model.md)
- **Platform packages:** `Platform.Serilog.Logging/`, `build/Platform.Logging.*.props`

## Platform standards (docs)

- [platform-logging-standard.md](monolith-modularization/platform-logging-standard.md)
- [platform-correlation-standard.md](monolith-modularization/platform-correlation-standard.md)
- [03-modularization-roadmap.md](monolith-modularization/03-modularization-roadmap.md)

## Minimal agent workflow

1. Read `sandbox-starter-kit` + task-specific skills.
2. Find the nearest existing pattern in the target module.
3. Implement the smallest correct diff.
4. Run `dotnet build` / `dotnet test` on touched projects.
5. Sync skills if you changed `.cursor/skills/`.

## Extending the kit

Add skills when a pattern repeats **three or more times** or when mistakes are costly (security, actor lifecycle, EF leakage).

Keep each skill:

- **One concern** per folder
- **Actionable** checklists and do/don't tables
- **Paths** in frontmatter so tools auto-attach in the right files
- **Short** — link to `docs/` for long-form standards
