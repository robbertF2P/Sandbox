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
- `tailwind-ui-styling` — Tailwind v4 utility-first styling with `@floorganise/css`

### Domain & architecture

- `domain-driven-design` — Evans DDD foundations
- `implementing-domain-driven-design` — Vernon DDD implementation
- `domain-specific-languages` — Fowler DSL design and implementation
- `specification-pattern` — Specification + Repository; Ardalis.Specification; avoid IQueryable leakage

### .NET

- `dotnet-core-csharp-development` — C# / ASP.NET Core conventions (`docs/coding-standards/csharp-coding-standards.md`)
- `angular-frontend-development` — Angular / Nx frontend conventions (`docs/coding-standards/angular-coding-standards.md`)
- `tailwind-ui-styling` — Tailwind v4 utility-first styling with `@floorganise/css`
- `dotnet-ef-core` — Entity Framework Core
- `immutable-domain-ef-core` — immutable aggregates with EF Core (With* helpers, graph reconciliation)
- `akka-net` — Akka.NET (repo patterns)
- `reactive-applications-akka-net` — Anthony Brown reactive systems / Akka.NET book
- `functional-programming-csharp` — functional patterns in C#
- `csharp-enum-best-practices` — enum design, status outcomes, smart-class evolution (Horvat / Coding Helmet)

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

Details: `docs/monolith-modularization/module-composition-di.md` · **Architecture overview:** `docs/monolith-modularization/platform-architecture-overview.md` · Plan: `docs/monolith-modularization/foundation-and-pilot-plan.md` · Frontend: `docs/monolith-modularization/platform-frontend-standard.md` · UI customization: `docs/monolith-modularization/platform-ui-customization-standard.md` · **Pack blueprint:** `docs/monolith-modularization/platform-pack-blueprint.md` · Legacy Text*/Bool* fields: `docs/monolith-modularization/tenant-workflow-fields-deepdive-instructions.md` · Actors: `docs/monolith-modularization/platform-actor-standard.md` · Auth: `docs/monolith-modularization/platform-authentication-standard.md` · Agent rules: `docs/monolith-modularization/agent-instructions-snippet.md` (copy to monolith as `agent-rules.md` + `.github/copilot-instructions.md`).

**SandBox** uses Cursor for brainstorming and skill authoring (`.cursor/skills/`). The monolith does not need Cursor — use the agent snippet for Copilot and Claude.

## Cursor Cloud specific instructions

Environment facts and non-obvious caveats for cloud agents (the startup update script already restores .NET + web deps).

**Toolchain**
- .NET 10 SDK is installed at `/usr/local/dotnet-root` and symlinked to `/usr/local/bin/dotnet` (on default PATH; no `global.json`, so the SDK is not pinned). Node 22 + npm are preinstalled.
- Docker/Podman are **not** installed, so the SQL Server / Seq compose stacks and the `docker-compose.platform.yml` full stack cannot run here.

**F2pPlatform is the flagship runnable product.** Standard build/test/run commands are in `F2pPlatform/README.md`. Cloud-specific notes:
- Run the API **without SQL Server** by enabling SQLite: `ASPNETCORE_ENVIRONMENT=Development HourApprovals__UseSqlite=true PlatformConfig__UseSqlite=true dotnet run --project host/F2pPlatform.Host` (API on `http://localhost:5080`, Swagger at `/swagger`). `EnsureCreated` builds the SQLite DBs on startup.
- Tests run against SQLite automatically (they set the `Testing` environment) — `dotnet test` needs no external services.
- Auth is a POC: `POST /api/identity/login` accepts **any** credentials. Module endpoints read the `X-User-Name` and `X-User-Permissions` headers (e.g. `ApproveHoursProgress` unlocks hour-approval submit/approve).
- `local-feed/*.nupkg` (Platform.Serilog.Logging) is committed, so builds work out of the box. The `scripts/pack-*.sh` scripts currently fail under the .NET 10 CLI (a package `Description` arg is mis-parsed) but are **not** needed.

**Known breakage (not an environment problem):** the Angular tenant SPA at `F2pPlatform/web` does **not** compile on `main` — `npm start` / `npm run build` fail with TypeScript errors in `libs/hour-approvals/feature-tasks` (a mistyped `submitSelected` RxJS pipeline plus a `vitest` spec pulled into the app build via `tsconfig.app.json`'s `libs/**/*.ts` include, though `vitest` is not a dependency). `npm install` succeeds; only compilation is broken. Demonstrate the platform via the API/Swagger until the source is fixed.
