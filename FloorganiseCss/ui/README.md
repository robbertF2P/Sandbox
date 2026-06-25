# @floorganise/ui

Shared Angular presentational components built on `@floorganise/css`.

## v0.1 components

| Export | Purpose |
|--------|---------|
| `F2pHomeTilesComponent` | Production home tile grid (`f2ps-tile`, `f2p-tile-grid`) |
| `F2pButtonComponent` | `f2ps-btn-*` wrapper (`<f2p-btn>`) |
| `F2pAppNavbarComponent` | Dark production navbar shell |
| `F2pPageHeaderComponent` | Light module page header with optional Home link |

## Local development (SandBox)

Consuming apps resolve source via `tsconfig` paths and `file:` dependency:

```json
"@floorganise/ui": "file:../../FloorganiseCss/ui"
```

```bash
cd FloorganiseCss/ui && npm install && npm run build
```

## Standard

See `docs/monolith-modularization/platform-frontend-standard.md`.
