# Platform frontend standard

Design system and shared UI requirements for **all V2 frontend modules** in the external F2P monolith strangler program.

**Packages (developed in SandBox, consumed by monolith Nx workspace):**

| Package | Role |
|---------|------|
| `@floorganise/css` | Tailwind v4 theme tokens + semantic component aliases (`f2ps-*`, shell, panels) |
| `@floorganise/ui` | Reusable Angular presentational components built on `@floorganise/css` |

**Related:** `FloorganiseCss/README.md`, `docs/floor2plan-v2-read-model-playbook.md`, `foundation-and-pilot-plan.md` (Phase A8).

---

## Non-negotiable rules (V2 modules)

1. **Every V2 frontend module MUST depend on `@floorganise/css`.** No parallel design tokens, no ad-hoc global CSS that duplicates production Floor2Plan styling.
2. **Shared chrome and cross-context widgets MUST come from `@floorganise/ui`.** Do not copy-paste shell, tile, button, or form markup into context libraries.
3. **Context `ui` libraries** (`libs/<context>/ui`) contain **only** presentational components specific to that bounded context. Generic controls belong in `@floorganise/ui`.
4. **Tailwind utilities are allowed** alongside semantic classes (`class="panel flex gap-4"`). Prefer semantic aliases from `@floorganise/css` for brand, buttons, tiles, and shell layout.
5. **No new UI stack per module** — no Bootstrap, Angular Material, or third-party component kits as the primary styling layer unless explicitly approved and wrapped in `@floorganise/ui`.
6. **DOM contract parity** — class names used in V2 screens must match production patterns exercised by `Floor2PlanSmokeTests` where applicable (`f2ps-tile`, `f2ps-btn-primary`, `form.login`, etc.).

Legacy Razor/Vue screens are untouched during backend-first extraction. **Any new or strangler-replaced screen** in the Nx SPA follows this standard.

---

## `@floorganise/css`

Floorganise design system on **Tailwind CSS v4** with semantic aliases mapped via `@apply`.

| Source (SandBox) | Purpose |
|------------------|---------|
| `FloorganiseCss/` | Package root — publish or `file:` link |
| `FloorganiseCss/showcase/` | Vue reference — full token + component gallery |
| `FloorganiseCss/showcase-angular/` | Angular reference — Platform 2.0 home mock |

### Adoption (Angular / Nx)

**package.json** (monolith `floor2plan-web` workspace root or shared lib):

```json
{
  "dependencies": {
    "@floorganise/css": "^0.1.0",
    "tailwindcss": "^4.3.0"
  }
}
```

Until an internal npm feed is wired, SandBox POCs use `"@floorganise/css": "file:../../FloorganiseCss"`. Monolith should consume the **published** package from your feed — not project-reference SandBox.

**styles entry** (`apps/f2p-shell/src/styles.css` or `libs/shared/ui/src/styles.css`):

```css
@import '@floorganise/css';
```

**PostCSS / build** — follow `FloorganiseCss/showcase-angular/.postcssrc.json` and `angular.json` styles pipeline. Tailwind v4 via `@tailwindcss/postcss` (Angular) or `@tailwindcss/vite` (Vite/Vue POCs).

**App shell themes:**

| Theme class | Use |
|-------------|-----|
| `f2p-app-light` | Production-aligned shell (home, module grid) |
| `f2p-app-dark` | Dark glass POC theme (tools, live status panels) |

### Tokens (do not redefine locally)

Use theme utilities from `@floorganise/css` — e.g. `bg-f2p-brand`, `text-f2p-ink`, `border-f2p-border`, `bg-f2p-dark-surface`. Extend tokens in SandBox `FloorganiseCss/src/theme.css` and release a new package version; do not fork tokens in context libs.

---

## `@floorganise/ui` (shared component library)

**Status:** Scaffold in SandBox Phase A8; required for all V2 Nx context modules before UI strangler slices scale.

Purpose: one **publishable Angular library** so every bounded-context feature reuses the same shell, navigation, buttons, tiles, forms, tables, toasts, and loading states — all styled via `@floorganise/css`.

### Target layout (SandBox → monolith)

```text
FloorganiseCss/
├── package.json                    # @floorganise/css (existing)
└── ui/                             # @floorganise/ui (new)
    ├── package.json
    ├── ng-package.json
    └── src/lib/
        ├── shell/                  # app chrome, module header, nav
        ├── tiles/                  # F2pHomeTiles, f2ps-tile grid
        ├── buttons/                # f2ps-btn-* wrappers
        ├── forms/                  # field, login panel patterns
        ├── feedback/               # toast, notifications, empty states
        └── data-display/           # table chrome, status cards, badges
```

**Seed from:** `FloorganiseCss/showcase-angular/src/app/components/` — promote stable components into `@floorganise/ui`; keep showcases as documentation only.

### Monolith Nx mapping

```text
floor2plan-web/
├── libs/
│   ├── shared/
│   │   ├── ui/                     # thin re-export / workspace alias → @floorganise/ui
│   │   ├── api-core/
│   │   └── util/
│   └── planning/
│       ├── feature-activity-list/  # imports @floorganise/ui + @floorganise/css
│       └── ui/                     # planning-only presentational components
```

| Library | May import | Must not |
|---------|------------|----------|
| `@floorganise/ui` | `@floorganise/css`, Angular common | Context APIs, `HttpClient`, generated OpenAPI clients |
| `libs/<context>/ui` | `@floorganise/ui`, `@floorganise/css` | Other contexts' `data-access` |
| `libs/<context>/feature-*` | context `ui`, `data-access`, `@floorganise/ui` | Raw duplicate of shared button/tile markup |

Enforce with `@nx/enforce-module-boundaries`: `scope:shared` ← `scope:planning` ← `scope:shell`.

### Component promotion criteria

Move a component from a context `ui` lib into `@floorganise/ui` when:

- Used (or clearly needed) by **two or more** bounded contexts, or
- Implements **shell chrome** (header, module dropdown, home tiles), or
- Maps to a **production `f2ps-*` DOM contract** shared across the product.

Keep in context `ui` when the component encodes **ubiquitous language** for one context only (e.g. Gantt row chrome for Planning).

---

## Per-module checklist (V2 frontend)

Copy into PR template for UI strangler slices.

- [ ] `@floorganise/css` in workspace dependencies; global styles import present
- [ ] No duplicate global theme / reset CSS in context libs
- [ ] Shell, tiles, buttons, forms use `@floorganise/ui` (not local copies)
- [ ] Context-specific markup only in `libs/<context>/ui`
- [ ] Tailwind utilities used for layout; semantic classes for brand/components
- [ ] Visual parity spot-checked against `FloorganiseCss/showcase-angular` or SmokeTests DOM selectors
- [ ] Responsive layout verified at 375px and 768px (see `rc-mobile-responsive-audit.md`; run `Floor2PlanSmokeTests` `npm run audit:mobile` for legacy baseline)
- [ ] Nx boundary tags: `type:ui` has no `data-access` imports

---

## SandBox development workflow

1. **Tokens / aliases** — edit `FloorganiseCss/src/`; verify in `showcase` and `showcase-angular`.
2. **Shared components** — add to `FloorganiseCss/ui/` (or dedicated package folder); export via `public-api.ts`.
3. **Version & publish** — bump `@floorganise/css` and `@floorganise/ui`; push to internal npm feed.
4. **Monolith** — bump lockfile in `floor2plan-web`; no SandBox project references in production CI.

```bash
# SandBox — local verification
cd FloorganiseCss/showcase-angular && npm install && npm start
# After ui package exists:
cd FloorganiseCss/ui && npm run build && npm test
```

---

## Anti-patterns

| Anti-pattern | Instead |
|--------------|---------|
| Copy `f2ps-btn-primary` markup into every feature | Import `F2pButton` (or equivalent) from `@floorganise/ui` |
| Per-module `styles.scss` with brand colours | `@floorganise/css` tokens |
| Material/Bootstrap as default for new V2 screens | `@floorganise/ui` + semantic aliases |
| Context A imports Context B's `ui` lib for a button | Promote widget to `@floorganise/ui` |
| Inline `<style>` with design tokens | Tailwind utilities or package aliases |

---

## Versioning

| Version | Date | Notes |
|---------|------|-------|
| 1.0 | 2026-06-21 | Initial standard — `@floorganise/css` required; `@floorganise/ui` shared lib defined |
