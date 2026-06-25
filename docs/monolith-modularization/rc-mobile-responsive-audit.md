# RC mobile responsive audit — legacy vs V2 Tailwind

**Purpose:** Evidence for the V2 frontend refactor (`@floorganise/css` + utility-first responsive layout).  
**Target:** `https://2025-14-patch.floor2plan.com` (RC acceptance, `testrd` service login)  
**Captured:** 2026-06-25  
**Reproduce:** `cd Floor2PlanSmokeTests && npm run audit:mobile`

---

## Summary

Legacy Floor2Plan login and home screens **look acceptable on desktop** but **break on phone-sized viewports**. The layout relies on bundled CSS (`login-style.bundle.css`, `home-style.bundle.css`) with fixed widths and desktop-first composition. There is no practical way to patch this screen-by-screen in legacy Razor without duplicating media queries across many bundles.

V2 modules should use **`@floorganise/css` semantic chrome** plus **Tailwind responsive utilities in templates** — the same DOM contract (`form.login`, `f2ps-tile`, `f2ps-btn-primary`) with layout that adapts by breakpoint.

---

## Login (`/Account/Login`)

### What we saw

| Viewport | Form width | Viewport width | Problem |
|----------|-------------|----------------|---------|
| iPhone SE (375×667) | **150px** | 375px | Form is 40% of screen; inputs unusably narrow |
| iPhone 14 (390×844) | **156px** | 390px | Same squeeze |
| iPad mini (768×1024) | **307px** | 768px | Still narrow; illustration dominates |
| Desktop (1280×720) | 329px | 1280px | Side-by-side layout works |

Screenshots: `assets/rc-mobile-audit/login-*.png`

### Specific failures (375px)

1. **Desktop two-column layout collapses without reflow** — RFID/illustration column and credentials column stay in a row; the form shrinks to ~150px instead of stacking full-width.
2. **Login button overlaps “Stay logged in?”** — checkbox row has no wrap/stack; button sits on top of label text.
3. **Wasted vertical space** — large tile padding and illustration consume the viewport; credentials are pushed to the bottom edge.
4. **Fixed bundle CSS** — `login-style.bundle.css` is minified site-wide rules; no component-level responsive variants like Tailwind `sm:` / `md:`.

### V2 contrast (`FloorganiseCss/showcase`)

Reference: `F2pLoginPanel.vue` — `f2ps-tile-fluid max-w-2xl w-full` + `form.login` + responsive action row:

```html
<div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
  <label>… Stay logged in?</label>
  <button class="f2ps-btn f2ps-btn-primary">Login</button>
</div>
```

Mobile-first: unprefixed `flex-col` stacks controls; `sm:flex-row` restores horizontal layout from 640px up ([Tailwind responsive design](https://tailwindcss.com/docs/responsive-design)).

---

## Home (`/`)

### What we saw

| Viewport | Visible tiles (first screen) | Layout |
|----------|------------------------------|--------|
| iPhone SE | ~1–2 of **14** | Single column, 327×152px tiles |
| iPhone 14 | ~2 of 14 | Same |
| iPad mini | ~4–6 visible with scroll | 2-column grid from ~768px |
| Desktop | 3-column grid | Works as designed |

Screenshots: `assets/rc-mobile-audit/home-*.png`

### Specific failures (375px)

1. **Extreme scroll depth** — 14 full-width tiles stacked; last tile starts at y≈2410px (~3.6 viewports of scrolling).
2. **Oversized tiles on mobile** — 152px height with small centred icon; low information density.
3. **Duplicate module entries** — both “Planboard” and “Planboard mobile” exist in DOM; one hidden via zero size (layout hack, not responsive design).
4. **Tiny chrome touch targets** — module menu button measures **20×20px** (below common 44px minimum).
5. **Header consumes ~45% of first screen** — logo block before any module tile.

### V2 target pattern

```html
<div class="f2p-tile-grid grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
  <a class="f2ps-tile …">…</a>
</div>
```

Use one tile component in `@floorganise/ui`; swap grid columns by breakpoint instead of separate “mobile” tiles.

---

## Argument for V2 refactor

| Legacy | V2 (`@floorganise/css` + Tailwind) |
|--------|-------------------------------------|
| Separate CSS bundles per area (`login-style`, `home-style`, Kendo) | One scanned utility pipeline; responsive variants co-located in templates |
| Desktop layout assumed; mobile = shrunk desktop | Mobile-first: unprefixed base, `sm:`/`md:`/`lg:` overrides |
| Duplicate “mobile” variants in markup | Single component; `grid-cols-*` / `flex-col` by breakpoint |
| Fixing RC login = edit compiled bundle + redeploy MVC | Strangler slice: new Angular route, same DOM contract, responsive utilities |
| No container queries for embedded widgets | `@container` variants for portable `@floorganise/ui` components |

**Smoke tests already pass at 1280×720** (`CYPRESS_VIEWPORT_WIDTH`). They do not catch mobile regressions. Add viewport checks to UI strangler acceptance criteria (see `platform-frontend-standard.md`).

---

## Reproduce

```sh
cd Floor2PlanSmokeTests
cp .env.smoke.example .env.smoke.local   # testrd / test
npm run audit:mobile
```

Output: `artifacts/rc-mobile-audit/report.json` and PNG screenshots (gitignored). Committed reference screenshots live under `docs/monolith-modularization/assets/rc-mobile-audit/`.

---

## Related

- `docs/monolith-modularization/platform-frontend-standard.md`
- `.cursor/skills/tailwind-ui-styling/SKILL.md` — Responsive design section
- `FloorganiseCss/showcase/src/components/F2pLoginPanel.vue`
- `Floor2PlanSmokeTests/README.md`
