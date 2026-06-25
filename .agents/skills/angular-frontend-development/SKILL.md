---
name: angular-frontend-development
description: |
  Guides Angular frontend development for Platform 2.0 Nx modules and SandBox Angular apps.
  Use when:
  - Building or refactoring Angular components, services, routes, or libs
  - Choosing standalone vs NgModule, inject vs constructor, signals vs mutable state
  - Structuring Nx feature/data-access/ui libraries
  - Wiring auth guards, HTTP clients, or lazy routes
  - Reviewing Angular PRs for style and architecture
paths:
  - "**/*.component.ts"
  - "**/*.component.html"
  - "**/*.routes.ts"
  - "**/angular.json"
  - "**/tsconfig*.json"
  - "**/F2pPlatform/web/**"
  - "**/AdminBackoffice/web/**"
  - "**/FloorganiseCss/showcase-angular/**"
metadata:
  version: 1.0.0
---

# Angular frontend development

Apply for **V2 frontend** and SandBox Angular work. **Team standards:** `docs/coding-standards/angular-coding-standards.md`.

**Styling:** always pair with `tailwind-ui-styling` (`@floorganise/css`).  
**Platform rules:** `docs/monolith-modularization/platform-frontend-standard.md`.

## Adopted baselines

| Source | Use for |
|--------|---------|
| [Angular style guide (2025)](https://angular.dev/style-guide) | Structure, naming, components, DI |
| [Google TypeScript style guide](https://google.github.io/styleguide/tsguide.html) | TS syntax, imports, types |
| [angular-eslint](https://github.com/angular-eslint/angular-eslint) | Lint presets |

## Non-negotiable (new code)

1. **Standalone components** — `imports: [...]` in `@Component`; no new `NgModule` unless bridging legacy.
2. **`inject()` over constructor** — for dependency injection.
3. **Signals for UI state** — `signal`, `computed`; avoid mutable fields for template-driven state.
4. **Built-in control flow** — `@if`, `@for`, `@switch` in templates.
5. **Feature-based Nx layout** — `libs/<context>/feature-*`, `data-access`, `ui`; not `libs/components`.
6. **`@floorganise/css`** — global import only; Tailwind utilities + semantic `f2ps-*` classes.
7. **DOM contract parity** — `form.login`, `f2ps-tile`, `f2ps-btn-primary` where production-aligned.

## Nx library layers

| Layer | Contains | May import |
|-------|----------|------------|
| `apps/f2p-shell` | Bootstrap, shell routes, global styles | feature libs, shared |
| `libs/<ctx>/feature-*` | Pages, smart components, routes | same context `data-access`, `ui`, shared |
| `libs/<ctx>/data-access` | API clients, DTOs, guards | shared `api-core` |
| `libs/<ctx>/ui` | Presentational components | shared `ui`, `@floorganise/css` |
| `libs/shared/ui` | Cross-context chrome | `@floorganise/css` |

`ui` libraries must **not** import another context's `data-access`.

## Component template

```typescript
@Component({
  selector: 'f2p-example-page',
  imports: [RouterLink, FormsModule],
  templateUrl: './example-page.component.html',
})
export class ExamplePageComponent implements OnInit {
  private readonly api = inject(ExampleApi);

  readonly loading = signal(true);
  readonly items = signal<ExampleDto[]>([]);
  readonly selected = computed(() => /* ... */);

  ngOnInit(): void {
    this.load();
  }

  protected onSave(): void { /* template handler */ }

  private load(): void { /* ... */ }
}
```

## File naming

- kebab-case: `hour-approvals-page.component.ts`
- Colocate: `.ts`, `.html`, `.css`, `.spec.ts` share base name
- Tests: `*.spec.ts` beside source

## Routing

- Export `Routes` from `feature-*/src/lib/*.routes.ts`
- Lazy load from shell: `loadChildren: () => import('@f2p/...').then(m => m.routes)`
- Guards in `data-access` when auth/API-dependent

## Styling (summary)

- Layout: Tailwind utilities (`flex`, `gap-4`, `grid`, responsive variants)
- Chrome: `f2ps-btn-primary`, `f2ps-tile`, `form.login`, `panel`
- Responsive: mobile-first — see `tailwind-ui-styling` skill

## Before finishing

1. `ng build` / `ng test` for touched projects
2. No `any` on public APIs without justification
3. Responsive spot-check at 375px for user-facing screens
4. If you edited `.cursor/skills/`, run `./scripts/sync-agent-skills.sh`

## Repo references

| Path | Purpose |
|------|---------|
| `F2pPlatform/web/` | Platform 2.0 reference |
| `AdminBackoffice/web/` | Operator UI reference |
| `FloorganiseCss/showcase-angular/` | Design system |
| `docs/coding-standards/angular-coding-standards.md` | Full team guide |
| `.cursor/skills/tailwind-ui-styling/SKILL.md` | Styling |
