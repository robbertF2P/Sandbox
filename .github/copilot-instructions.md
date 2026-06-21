# SandBox — Copilot instructions

This repo uses **Agent Skills** for domain-specific guidance. Copilot loads them from `.github/skills/` when relevant.

## Domain-driven design

- **`domain-driven-design`** — Eric Evans: Ubiquitous Language, bounded contexts, tactical building blocks, strategic design.
- **`implementing-domain-driven-design`** — Vaughn Vernon: aggregate rules, domain events, CQRS, application services, integration.

Use both for modeling and implementation work on C# domain code.

## Domain-specific languages

- **`domain-specific-languages`** — Martin Fowler: Semantic Model, internal vs external DSL, fluent APIs, parsing, code generation.

Use when designing configuration languages, rule engines, fluent builders, or parsers.

## Specification pattern

- **`specification-pattern`** — Named query/business rules, Ardalis.Specification, Repository encapsulation; avoid leaking `IQueryable` / `DbContext`.

Use when designing repositories, EF queries, or reusable domain criteria.

## Floor2Plan V2

- **`docs/floor2plan-v2-read-model-playbook.md`** — Nx Angular module layout + Specification-backed read/filter flow per bounded context.
- **`docs/floor2plan-legacy-connector-submodule-antipattern.md`** — why submodule-based connectors referencing core are an anti-pattern; integration pack target.
- **`docs/floor2plan-v2-connector-architecture.md`** — V2 connector dependency model, diagrams, ground rules.
- **`docs/floor2plan-v2-connector-migration-prompt.md`** — copy-paste Claude prompt: legacy connector → concrete V2 pack proposal.

## Floor2Plan / Platform 2.0 modularization

- **`docs/monolith-modularization/foundation-and-pilot-plan.md`** — foundation-first strangler plan for the external F2P monolith.
- **`docs/monolith-modularization/agent-instructions-snippet.md`** — agent-agnostic rules (copy to monolith `agent-rules.md` + `.github/copilot-instructions.md`).
- **`docs/monolith-modularization/module-composition-di.md`** — no ABP in new modules; `IServiceCollection` / `WebApplication` extension methods.
- **`docs/monolith-modularization/starter-kit/README.md`** — module scaffold kit (copy into monolith).

## .NET / C#

- **`dotnet-core-csharp-development`** — SDK-style projects, ASP.NET Core, DI, testing, C# style.
- **`dotnet-ef-core`** — EF Core DbContext, migrations, queries.
- **`akka-net`** — Akka.NET actors, messages, hosting (see `AkkaSignalRVuePoc`).
- **`reactive-applications-akka-net`** — Anthony Brown: Reactive Manifesto, supervision, scaling, clustering, persistence.

## Source of truth

Skills are authored in `.cursor/skills/`. Run `./scripts/sync-agent-skills.sh` after editing to refresh `.github/skills/` and `.agents/skills/`.
