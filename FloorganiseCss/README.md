# @floorganise/css

Floorganise design system built on **Tailwind CSS v4** with semantic component aliases.

## Why aliasing?

Tailwind utility classes are powerful but verbose in templates. This package maps Floorganise and POC component class names to Tailwind via `@apply`, so you get:

- **Fewer classes in markup** — use `.panel`, `.f2ps-btn-primary`, `.app-nav` instead of long utility strings
- **Full Tailwind power** — add utilities alongside semantic classes: `class="panel flex gap-4"`
- **Design tokens** — `bg-f2p-brand`, `text-f2p-ink`, `border-f2p-border` from production Floor2Plan CSS

## Design sources

| Source | What was extracted |
|--------|-------------------|
| [Floor2Plan production CSS](https://2025-14-patch.floor2plan.com/dist/f2p-style.bundle.css) | Brand `#00aeef`, tiles, buttons, notifications, `f2ps-*` patterns |
| [Floor2PlanSmokeTests](../Floor2PlanSmokeTests) | DOM contract: `.f2ps-tile`, `.f2ps-btn-primary`, `form.login` |
| Vue POC clients | Dark glass theme: `.panel`, `.app-nav`, gantt, progress components |

## Usage

```bash
npm install tailwindcss @tailwindcss/vite
npm install file:../../FloorganiseCss   # from a client app
```

**vite.config.ts**

```ts
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [vue(), tailwindcss()],
})
```

**main.ts**

```ts
import '@floorganise/css'
```

**App root (dark POC theme)**

```html
<div class="f2p-app-dark app">…</div>
```

Use `shell--wide` / `app-nav--wide` for the wider Import POC layout (1100px). SignalR POC uses default `shell` (960px).

## Token reference

| Token | Value | Tailwind utility |
|-------|-------|------------------|
| Brand | `#00aeef` | `bg-f2p-brand`, `text-f2p-brand` |
| Brand hover | `#0084c1` | `hover:bg-f2p-brand-hover` |
| Ink | `#101821` | `text-f2p-ink` |
| Surface | `#ffffff` | `bg-f2p-surface` |
| Border | `#d9d9d9` | `border-f2p-border` |
| Danger | `#d7263d` | `text-f2p-danger`, `bg-f2p-danger` |
| Success | `#7ac74f` | `bg-f2p-success` |
| Warning | `#ffa630` | `bg-f2p-warning` |

Dark POC tokens: `f2p-dark-*` prefix (e.g. `bg-f2p-dark-surface`, `text-f2p-dark-ink`).

## Component aliases

### Production (`f2ps-*`)

- `.f2ps-btn`, `.f2ps-btn-primary`, `.f2ps-btn-secondary`, `.f2ps-btn-tertiary`
- `.f2ps-btn-decline`, `.f2ps-btn-approve`, `.f2ps-btn-sm` … `.f2ps-btn-xl`
- `.f2ps-tile`, `.f2ps-tile-fluid`, `.f2ps-icon`
- `.login-notification`, `.login-notification-danger|warning|success`
- `form.login`, `.client-background`

### Vue POC (dark)

- Layout: `.app`, `.app-nav`, `.shell`, `.shell--wide`, `.grid`, `.panel`
- SignalR: `.hero`, `.status-card`, `.catalog-list`, `.btn`, `.toast`, `.messages`
- Forms: `.field`, `input`, `textarea`, `button`, `.btn-secondary`, `.btn-danger`
- Data viz: `.gantt*`, `.progress-bar*`, `.progress-tree*`
- Feedback: `.error`, `.success`, `.muted`

## Showcase

Sample Vue components demonstrating the styling strategy:

```bash
cd FloorganiseCss/showcase
npm install
npm run dev   # http://localhost:5175
```

| Component | Pattern |
|-----------|---------|
| `F2pHomeTiles` | Production home grid — `f2ps-tile`, `f2p-tile-grid` |
| `F2pLoginPanel` | Login form — `form.login`, `f2ps-btn-primary`, notifications |
| `F2pButtonShowcase` | All `f2ps-btn-*` variants and sizes |
| `F2pCatalogPanel` | Dark catalog — `catalog-list`, `btn--*` |
| `F2pStatusPanel` | Live hub — `hero`, `status-card`, `state.*` |
| `F2pHybridPanel` | Semantic + Tailwind utilities together |
| `F2pToastSample` | Toast feedback — `toast.success` |

## Extending

Add tokens in `src/theme.css` (`@theme` block). Add aliases in `src/components/`. Use Tailwind utilities directly in Vue templates for one-off layout.
