# Floorganise coding standards

Team guidelines for **new work** in SandBox and Platform 2.0 (V2 monolith). Prefer **tool-enforced** conventions over long style debates.

## Documents

| Guide | Audience | Baseline |
|-------|----------|----------|
| [C# coding standards](./csharp-coding-standards.md) | Backend, actors, APIs, tests | ReSharper/Rider defaults + Microsoft .NET conventions |
| [Angular coding standards](./angular-coding-standards.md) | Nx / V2 frontend modules | Angular 2025 style guide + Google TypeScript guide |

## Enforcement

| Layer | C# | Angular / TypeScript |
|-------|----|----------------------|
| Editor | Rider / ReSharper / VS with `.editorconfig` | VS Code / WebStorm with `.editorconfig` |
| Format on save | `dotnet format` or ReSharper cleanup | Prettier (if configured) + ESLint |
| CI (recommended) | `dotnet format --verify-no-changes` | `ng lint` / `eslint` |

Root [`.editorconfig`](../../.editorconfig) applies repo-wide. Solution-specific overrides live beside `.sln` files when needed (e.g. `AkkaSignalRVuePoc/.editorconfig`).

## AI / agent skills

| Skill | Path |
|-------|------|
| C# / ASP.NET Core | `.cursor/skills/dotnet-core-csharp-development/` |
| Angular frontend | `.cursor/skills/angular-frontend-development/` |
| Tailwind / `@floorganise/css` | `.cursor/skills/tailwind-ui-styling/` |

After editing skills: `./scripts/sync-agent-skills.sh`.

## Adopted external references

**C#**

- [ReSharper code style & naming](https://www.jetbrains.com/help/resharper/Code_Syntax_Style.html) — default baseline; encode in `.editorconfig`
- [Microsoft C# coding conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [dotnet/docs .editorconfig](https://github.com/dotnet/docs/blob/main/.editorconfig) — starting point for analyzer severities
- [Framework Design Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/) — public API design (libraries, NuGet packages)

**Angular / TypeScript**

- [Angular style guide (2025)](https://angular.dev/style-guide) — primary Angular rules
- [Google TypeScript style guide](https://google.github.io/styleguide/tsguide.html) — TypeScript outside Angular-specific rules
- [angular-eslint](https://github.com/angular-eslint/angular-eslint) — lint presets for standalone + signals

**Platform 2.0 (Floorganise-specific)**

- `docs/monolith-modularization/module-composition-di.md` — no ABP in new modules
- `docs/monolith-modularization/platform-frontend-standard.md` — `@floorganise/css` + `@floorganise/ui`
