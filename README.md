# Floorganise SandBox

Reference implementations, platform packages, and AI-assisted modularization assets for the **Floor2Plan (F2P) Platform 2.0** strangler refactor.

This repo is the Floorganise-owned home for F2P refactor work. Personal experiments and unrelated sandboxes live elsewhere.

## Start here

| Audience | Entry |
|----------|--------|
| **AI agents** | `AGENTS.md` → `sandbox-starter-kit` skill |
| **Humans** | [docs/ai-starter-kit.md](docs/ai-starter-kit.md) |
| **Monolith program** | [docs/monolith-modularization/foundation-and-pilot-plan.md](docs/monolith-modularization/foundation-and-pilot-plan.md) |

## Repo map

| Area | Path |
|------|------|
| Modularization program | `docs/monolith-modularization/` |
| **V2 host + module template** | `F2pPlatform/` — backend host, Reference module, Angular `web/` shell |
| Floorganise design system | `FloorganiseCss/` (`@floorganise/css`, showcase apps) |
| Platform logging + correlation | `Platform.Serilog.Logging/`, `build/Platform.Logging.*.props` |
| Import domain kernel | `ImportPipeline/` |
| Reference POCs | `ApiImportActorPoc/`, `AkkaSignalRVuePoc/`, `PrimaveraExcelReader/` |
| Smoke / DOM contract tests | `Floor2PlanSmokeTests/` |
| Agent skills + rules | `.cursor/skills/`, `.cursor/rules/` — sync with `./scripts/sync-agent-skills.sh` |

## Two-repo model

| Repo | Role |
|------|------|
| **This SandBox** | POCs, templates, standards, shared NuGet packages |
| **F2P monolith** (external) | Production code, characterization tests, extractions |

Copy artifacts from here into the monolith per `foundation-and-pilot-plan.md` — do not project-reference SandBox from production.

## Quick verify

```bash
dotnet build F2pPlatform
dotnet test F2pPlatform
dotnet build Platform.Serilog.Logging
dotnet test ImportPipeline
```
