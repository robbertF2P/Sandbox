# Angular coding standards

Standards for **V2 frontend** work (Nx monolith, SandBox Angular apps). Baseline: **[Angular 2025 style guide](https://angular.dev/style-guide)** + **[Google TypeScript style guide](https://google.github.io/styleguide/tsguide.html)**, plus Floorganise platform rules.

> **Principle:** Use modern Angular (standalone, `inject`, signals, built-in control flow). Organize by **feature**, not by file type. Styling via **`@floorganise/css`** — see `tailwind-ui-styling` skill.

## Adopted sources

| Source | Role |
|--------|------|
| [Angular style guide (2025)](https://angular.dev/style-guide) | File naming, project structure, components, DI |
| [Google TypeScript style guide](https://google.github.io/styleguide/tsguide.html) | TypeScript syntax, imports, types (Angular guide defers here) |
| [angular-eslint](https://github.com/angular-eslint/angular-eslint) | Recommended lint rules for templates and TS |
| `docs/monolith-modularization/platform-frontend-standard.md` | `@floorganise/css`, `@floorganise/ui`, DOM contract |
| `docs/coding-standards/../monolith-modularization/rc-mobile-responsive-audit.md` | Why responsive utilities matter |

## Modern Angular defaults (2025+)

Use these for **all new code**:

| Topic | Do | Avoid |
|-------|-----|-------|
| Modules | Standalone components, `imports: [...]` in `@Component` | New `NgModule` unless bridging legacy |
| DI | `inject()` function | Constructor injection in new components |
| State | `signal`, `computed`, `linkedSignal` | Mutable class fields for UI state |
| Templates | `@if`, `@for`, `@switch` | `*ngIf`, `*ngFor`, `*ngSwitch` |
| Bindings | `[class.x]`, `[style.prop]` | `ngClass`, `ngStyle` |
| Inputs/outputs | `input()`, `output()`, `model()` | `@Input` / `@Output` in new code |
| Host bindings | `host: { ... }` in decorator | `@HostBinding`, `@HostListener` |
| File suffixes | Optional (Angular CLI v20+) | Mandatory `.component.ts` boilerplate — use when it aids clarity |

**When in doubt, prefer consistency within the file** (Angular style guide).

## Project structure (Nx / Platform 2.0)

```
web/
├── apps/
│   └── f2p-shell/          # Composition root — routes, shell, bootstrap
└── libs/
    ├── shared/
    │   ├── ui/               # Cross-context presentational components
    │   └── api-core/         # HTTP interceptors, API config
    └── <context>/
        ├── feature-*/        # Smart components, routes, pages
        ├── ui/                 # Context-only presentational components
        └── data-access/        # Services, DTOs, guards, API clients
```

| Rule | Detail |
|------|--------|
| Feature folders | `libs/planning/feature-gantt/`, not `libs/components/` |
| One concept per file | One component/service per file when practical |
| Colocate tests | `user-profile.spec.ts` next to `user-profile.ts` |
| Barrel exports | `index.ts` per lib; public API only |
| Path aliases | `@f2p/<context>/<layer>` — match `tsconfig.base.json` |

Reference implementations: `F2pPlatform/web/`, `AdminBackoffice/web/`.

## File naming

| Rule | Example |
|------|---------|
| kebab-case file names | `hour-approvals-page.component.ts` |
| Match primary type | `UserProfile` → `user-profile.ts` |
| Tests | `user-profile.spec.ts` |
| Group component files | `user-profile.ts`, `user-profile.html`, `user-profile.css` (same base name) |

Class names remain **PascalCase** (`HourApprovalsPageComponent`). Selector prefix: app-specific (e.g. `f2p-hour-approvals-page`).

## TypeScript style (Google guide summary)

| Topic | Convention |
|-------|------------|
| Quotes | Single quotes for TS strings (`.editorconfig`: `quote_type = single`) |
| Semicolons | Required |
| `const` / `let` | `const` by default; `let` when reassigned; never `var` |
| Types | Explicit parameter and return types on public methods; avoid `any` |
| Imports | Prefer single quotes; group: Angular → third-party → `@f2p/*` → relative |
| Equality | `===` / `!==` |
| Unused | No unused imports or variables (ESLint) |

## Components

### Class layout

Order inside `@Component` classes:

1. Injected dependencies (`private readonly x = inject(X)`)
2. Inputs, outputs, queries (`readonly x = input()`)
3. Signals / computed state
4. Constructor (only if needed — prefer `inject`)
5. Lifecycle hooks (thin — delegate to named methods)
6. Public methods
7. Protected template helpers
8. Private methods

### Template rules

- Keep templates **presentation-focused**; move non-trivial logic to `computed()` or the component class.
- Name event handlers for **what they do**: `saveUser()` not `handleClick()`.
- Use `protected` for members only referenced from the template.
- Mark `input()` / `output()` / `model()` properties `readonly`.
- Implement lifecycle interfaces (`implements OnInit`).

### Example (repo pattern)

```typescript
@Component({
  selector: 'f2p-hour-approvals-page',
  imports: [RouterLink, FormsModule, DatePipe],
  templateUrl: './hour-approvals-page.component.html',
})
export class HourApprovalsPageComponent implements OnInit {
  private readonly api = inject(HourApprovalsApi);

  readonly loading = signal(true);
  readonly tasks = signal<HourApprovalTaskDto[]>([]);
  readonly canApprove = computed(() => this.capabilities()?.canApprove ?? false);

  ngOnInit(): void {
    this.load();
  }

  protected selectTask(id: string): void {
    this.selectedTaskId.set(id);
  }

  private load(): void { /* ... */ }
}
```

## Styling

| Rule | Detail |
|------|--------|
| Global entry | `@import '@floorganise/css';` in `styles.css` only |
| Layout | Tailwind utilities in templates (`flex`, `gap-4`, `sm:grid-cols-2`) |
| Brand chrome | Semantic classes (`f2ps-btn-primary`, `f2ps-tile`, `form.login`) |
| No parallel design system | No Bootstrap/Material as primary layer |
| Responsive | Mobile-first breakpoints — see `tailwind-ui-styling` skill |

## Services and data access

- **Smart** data access lives in `data-access` libs (`HourApprovalsApi`, guards, interceptors).
- Components call services; services call `HttpClient` — no HTTP in templates.
- Use `Observable` or `async` patterns consistently within a lib; prefer signals at the component boundary for UI state.
- DTOs: plain types/interfaces in `*.dto.ts`; map API shapes explicitly.

## Routing

- Feature routes in `feature-*/src/lib/*.routes.ts`; lazy-loaded from shell.
- Guards/resolvers in `data-access` when they depend on auth or API.
- Route names and paths: kebab-case (`hour-approvals`, not `hourApprovals`).

## Testing

| Topic | Convention |
|-------|------------|
| Unit tests | Jasmine/Karma or Jest — match project config |
| File naming | `*.spec.ts` colocated |
| TestBed | `imports: [ComponentUnderTest]` for standalone |
| DOM | Prefer semantic selectors (`form.login`, `f2ps-tile`) for parity with smoke tests |

## ESLint (recommended)

Use `angular-eslint` with TypeScript recommended rules. Minimum for new projects:

- `@angular-eslint/prefer-standalone`
- `@angular-eslint/prefer-on-push-component-change-detection` (for presentational components)
- `@typescript-eslint/no-unused-vars`
- `@typescript-eslint/explicit-function-return-type` (warn on public API)

## Pull request checklist

- [ ] Standalone component; `inject()` for dependencies
- [ ] Signals / built-in control flow for new UI state and templates
- [ ] `@floorganise/css` for styling; no hard-coded brand hex
- [ ] Feature lib boundaries respected (`ui` does not import `data-access` from other contexts)
- [ ] Responsive layout checked at 375px and 768px for user-facing screens
- [ ] `ng build` / `ng test` pass for touched projects

## Repo references

| Path | Purpose |
|------|---------|
| `F2pPlatform/web/` | Platform 2.0 Angular reference |
| `AdminBackoffice/web/` | Operator UI reference |
| `FloorganiseCss/showcase-angular/` | Design system gallery |
| `.cursor/skills/angular-frontend-development/SKILL.md` | Agent skill |
| `.cursor/skills/tailwind-ui-styling/SKILL.md` | Styling skill |
