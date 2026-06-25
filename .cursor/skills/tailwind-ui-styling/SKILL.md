---
name: tailwind-ui-styling
description: |
  Guides Tailwind CSS v4 utility-first styling with @floorganise/css in SandBox and V2 frontend modules.
  Use when:
  - Building or styling Vue, Angular, or HTML UI
  - Adding layout, spacing, colours, or responsive behaviour
  - Choosing between semantic classes (f2ps-*, panel) and Tailwind utilities
  - Wiring Tailwind into a new frontend app or showcase
  - Extending Floorganise design tokens or component aliases
paths:
  - "**/*.{vue,html,css,scss}"
  - "**/FloorganiseCss/**"
  - "**/angular.json"
  - "**/vite.config.ts"
  - "**/.postcssrc.json"
metadata:
  version: 1.1.0
---

# Tailwind UI styling

Apply this skill for all frontend styling in SandBox and when advising on V2 monolith Nx modules. **Do not** introduce Bootstrap, Angular Material, or ad-hoc global CSS as the primary styling layer.

**Platform standard:** `docs/monolith-modularization/platform-frontend-standard.md`  
**Package source:** `FloorganiseCss/` (`@floorganise/css`)  
**Official Tailwind docs:** [utility classes](https://tailwindcss.com/docs/styling-with-utility-classes) · [responsive design](https://tailwindcss.com/docs/responsive-design) · [framework guides](https://tailwindcss.com/docs/installation/framework-guides)  
**RC mobile audit (legacy vs V2):** `docs/monolith-modularization/rc-mobile-responsive-audit.md`

## Non-negotiable rules

1. **Depend on `@floorganise/css`** — import it in global styles; do not fork brand tokens or `f2ps-*` patterns in app code.
2. **Utility-first in templates** — compose layout and spacing with Tailwind utilities (`flex`, `gap-4`, `mt-3`, `lg:flex-row`).
3. **Semantic aliases for stable chrome** — buttons, tiles, shell, login, notifications use `@floorganise/css` classes (`f2ps-btn-primary`, `f2ps-tile`, `panel`, `form.login`).
4. **Hybrid is the default** — semantic surface + utilities for one-off layout: `class="panel flex flex-col gap-4 lg:flex-row"`.
5. **Extend tokens in SandBox only** — add colours/spacing in `FloorganiseCss/src/theme.css` (`@theme`); release a new package version; never redefine `bg-f2p-brand` locally.
6. **Shared widgets from `@floorganise/ui`** (monolith) — do not copy-paste shell/tile/button markup into context libs.
7. **DOM contract parity** — class names for production-aligned screens must match `Floor2PlanSmokeTests` selectors where applicable.

## Mental model (utility-first)

Style elements by stacking **single-purpose classes** in markup. Tailwind scans source files, generates CSS only for classes you use, and outputs one compiled stylesheet.

| Approach | Use for |
|----------|---------|
| **Utilities in markup** | Layout, spacing, responsive breakpoints, one-off tweaks |
| **Semantic aliases** (`@apply` in `@layer components`) | Repeated Floorganise patterns — buttons, tiles, panels, nav |
| **Theme tokens** (`@theme` in `theme.css`) | Brand colours, radii, shadows → utilities like `bg-f2p-brand` |
| **Framework components** | Reusable UI with structure + classes encapsulated (`@floorganise/ui`) |
| **Inline `style`** | Dynamic values from API/state only (e.g. progress bar width) |

Utilities beat inline styles because they support **variants** (`hover:`, `focus:`, `sm:`, `dark:`) and **design constraints** (token scale, not magic numbers).

## Choosing semantic vs utility

```
Is it Floorganise chrome (button, tile, shell, login, toast)?
  yes → semantic class from @floorganise/css (f2ps-*, panel, btn, form.login)
  no  → Is the same utility chain used 3+ times across files?
          yes → add @apply alias in FloorganiseCss/src/components/ OR extract @floorganise/ui component
          no  → Tailwind utilities directly in template
```

**Reference:** `FloorganiseCss/showcase/src/components/F2pHybridPanel.vue` — `.panel` + `flex flex-col gap-4 lg:flex-row`.

## Installation (Tailwind v4)

No `tailwind.config.js` by default. Pick the build integration for your stack:

### Vue / Vite

```ts
// vite.config.ts
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [vue(), tailwindcss()],
})
```

```ts
// main.ts
import '@floorganise/css'
```

Packages: `tailwindcss`, `@tailwindcss/vite`, `@floorganise/css`.

### Angular

```json
// .postcssrc.json
{ "plugins": { "@tailwindcss/postcss": {} } }
```

```css
/* styles.css */
@import '@floorganise/css';
```

Packages: `tailwindcss`, `@tailwindcss/postcss`, `@floorganise/css`. Wire PostCSS in `angular.json` styles pipeline — see `FloorganiseCss/showcase-angular/`.

## App shell themes

| Root class | Use |
|------------|-----|
| `f2p-app-light` | Production-aligned shell (home, module grid) |
| `f2p-app-dark` | Dark glass POC theme (tools, live status panels) |

Wrap app root: `<div class="f2p-app-light app">` or `f2p-app-dark`.

## Tokens (do not redefine)

Defined in `FloorganiseCss/src/theme.css` via `@theme`. Use as utilities:

| Intent | Utilities |
|--------|-----------|
| Brand | `bg-f2p-brand`, `text-f2p-brand`, `hover:bg-f2p-brand-hover` |
| Ink / surfaces | `text-f2p-ink`, `bg-f2p-surface`, `border-f2p-border` |
| Semantic | `bg-f2p-success`, `bg-f2p-warning`, `text-f2p-danger` |
| Dark POC | `bg-f2p-dark-surface`, `text-f2p-dark-ink`, `border-f2p-dark-border` |

## Responsive design

Tailwind is **mobile-first**: unprefixed utilities apply at all sizes; prefixed utilities apply at that breakpoint **and up**.

| Prefix | Min width |
|--------|-----------|
| `sm` | 640px (40rem) |
| `md` | 768px (48rem) |
| `lg` | 1024px (64rem) |
| `xl` | 1280px (80rem) |
| `2xl` | 1536px (96rem) |

### Rules

1. **Unprefixed = mobile** — do not use `sm:` to target phones; `sm:` means “from 640px up”.
2. **Layout in templates** — pair semantic chrome with responsive utilities: `class="f2ps-tile-fluid w-full max-w-2xl flex flex-col gap-4 lg:flex-row"`.
3. **Stack then spread** — `flex-col sm:flex-row`, `grid-cols-1 sm:grid-cols-2 lg:grid-cols-3`, `w-full lg:w-72`.
4. **Breakpoint ranges** — `md:max-xl:flex` targets only md–xl; `max-md:hidden` hides below md.
5. **Container queries** — for `@floorganise/ui` widgets in sidebars/modals: `@container` on parent, `@md:flex-row` on children (sizes by parent, not viewport).
6. **Custom breakpoints** — add `--breakpoint-*` in `FloorganiseCss/src/theme.css` `@theme` (use `rem`); no `tailwind.config.js`.

### Login and home (V2)

| Screen | Semantic chrome | Responsive utilities |
|--------|-----------------|-------------------|
| Login | `f2ps-tile-fluid`, `form.login`, `f2ps-btn-primary` | `w-full max-w-2xl mx-auto px-4`; action row `flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between` |
| Home grid | `f2ps-tile`, `f2p-tile-grid` | `grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3` |

**Reference:** `FloorganiseCss/showcase/src/components/F2pLoginPanel.vue`, `F2pHybridPanel.vue`.

### Legacy gap (why V2 matters)

RC audit (`2025-14-patch.floor2plan.com`, 375px): login form shrinks to **150px** wide; Login button overlaps “Stay logged in?”; home shows **14** stacked 152px tiles (~3.6 screens of scroll); module menu is **20×20px**. Legacy bundles (`login-style.bundle.css`) are desktop-first with fixed widths. Details: `docs/monolith-modularization/rc-mobile-responsive-audit.md`.

## Variants (common patterns)

| Pattern | Example |
|---------|---------|
| Hover / focus | `hover:bg-f2p-brand-hover`, `focus:ring-2` |
| Responsive | `flex-col sm:flex-row`, `w-full lg:w-72`, `md:max-xl:block` |
| Container | `@container` + `@md:flex-row` on children |
| Dark (theme class) | Prefer `f2p-app-dark` shell; use `dark:` only when matching `prefers-color-scheme` |
| Arbitrary value | `w-[72px]`, `min-[320px]:text-center` — sparingly |
| Dynamic width | `:style="{ width: `${pct}%` }"` on progress fills; static chrome stays in classes |

Stack variants left-to-right: `lg:hover:bg-f2p-brand-hover`.

## Managing duplication

When the same utility chain repeats:

1. **Loop / component param** — lists, grids, repeated cards
2. **Vue/Angular component** — structure + classes in one file (`@floorganise/ui` for shared chrome)
3. **`@apply` alias** — stable cross-screen pattern → `FloorganiseCss/src/components/*.css`
4. **Multi-cursor edit** — acceptable for local, temporary duplication in one file

Do **not** create long utility chains for buttons/tiles that already have `f2ps-*` aliases.

## Semantic alias inventory

### Production (`f2ps-*`)

- Buttons: `f2ps-btn-primary`, `f2ps-btn-secondary`, `f2ps-btn-decline`, `f2ps-btn-approve`, sizes `f2ps-btn-sm` … `f2ps-btn-xl`
- Tiles: `f2ps-tile`, `f2ps-tile-fluid`, `f2p-tile-grid`
- Login: `form.login`, `.login-notification`, `.client-background`

### POC dark theme

- Layout: `app`, `app-nav`, `shell`, `shell--wide`, `panel`, `grid`
- Actions: `btn`, `btn--primary`, `btn--secondary`, `btn--danger`
- Feedback: `toast`, `toast.success`, `error`, `success`, `muted`

Full list: `FloorganiseCss/README.md`.

## Extending the design system

1. **New token** — add to `@theme` in `FloorganiseCss/src/theme.css`
2. **New repeated pattern** — add `@layer components` rule in `FloorganiseCss/src/components/` (or `@utility` for atomic helpers like `f2ps-btn-base`)
3. **Verify** — run showcase: `FloorganiseCss/showcase` (Vue) or `showcase-angular` (Angular)
4. **Publish** — bump `@floorganise/css` version; monolith consumes from feed

## Anti-patterns (reject)

| Don't | Do instead |
|-------|------------|
| `<div style="display:flex; gap:16px">` for static layout | `class="flex gap-4"` |
| Custom `.my-primary-button { background:#00aeef }` in app CSS | `f2ps-btn-primary` or `bg-f2p-brand` |
| Bootstrap / Material as primary layer | `@floorganise/css` + `@floorganise/ui` |
| 15-class utility soup for a standard button | `f2ps-btn-primary` |
| `tailwind.config.js` with v3-style `theme.extend` | `@theme` in `theme.css` |
| Copy-paste `f2ps-tile` markup into every context lib | `@floorganise/ui` component |

## Before finishing UI work

1. Global styles import `@floorganise/css` (not raw `@import "tailwindcss"` alone in app code).
2. Semantic classes used for brand chrome; utilities for layout.
3. No hard-coded brand hex in templates (use `f2p-*` tokens).
4. Showcase or app builds: `npm run dev` / `npm start` in the relevant FloorganiseCss showcase.
5. If you edited `.cursor/skills/`, run `./scripts/sync-agent-skills.sh`.

## Repo references

| Path | Purpose |
|------|---------|
| `FloorganiseCss/README.md` | Package usage, token table, alias list |
| `FloorganiseCss/showcase/` | Vue gallery — all patterns |
| `FloorganiseCss/showcase-angular/` | Angular Platform 2.0 home mock |
| `AdminBackoffice/web/` | **Reference** operator UI — light shell, tables, forms |
| `FloorganiseCss/src/index.css` | Package entry (`tailwindcss` + theme + components) |
| `FloorganiseCss/src/theme.css` | `@theme` tokens |
| `FloorganiseCss/src/components/f2ps.css` | Production `f2ps-*` aliases |
| `FloorganiseCss/src/components/poc.css` | Dark POC aliases |
| `docs/monolith-modularization/platform-frontend-standard.md` | V2 monolith rules |
