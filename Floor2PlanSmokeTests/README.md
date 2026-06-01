# Floor2Plan smoke test harness

This harness runs a Cypress smoke test against one or more Floor2Plan login URLs. The test enters the application by clicking the lower-right Floorganise logo and verifies the home page renders tiles. The default target is:

```text
https://2025-14-patch.floor2plan.com/Account/Login
```

## Run with npm

```sh
npm ci
npm run test:smoke
```

## Run with an existing Edge Microsoft session

Cypress normally launches browsers with a clean profile. To use transparent Microsoft login from Edge, close Edge first, then point Cypress at the Edge user-data directory and profile that already has access:

```sh
CYPRESS_EDGE_USER_DATA_DIR="$HOME/.config/microsoft-edge" \
CYPRESS_EDGE_PROFILE_DIRECTORY="Default" \
npm run test:smoke:edge
```

On Windows, run the same script from PowerShell with your Edge user-data directory:

```powershell
$env:CYPRESS_EDGE_USER_DATA_DIR="$env:LOCALAPPDATA\Microsoft\Edge\User Data"
$env:CYPRESS_EDGE_PROFILE_DIRECTORY="Default"
npm run test:smoke:edge
```

Use the profile name shown in `edge://version` if your Microsoft account is not in `Default`.

## Run with xUnit

```sh
npm ci
dotnet test Floor2PlanSmokeTests.csproj
```

The xUnit test launches the same Cypress spec and fails when Cypress fails.

## Run with Docker

Build the container:

```sh
docker build -t floor2plan-smoke-tests .
```

Run against the default target:

```sh
docker run --rm floor2plan-smoke-tests
```

Run against a specific target:

```sh
docker run --rm -e TARGET_URL=https://example.com/Account/Login floor2plan-smoke-tests
```

Run against multiple targets:

```sh
docker run --rm -e TARGET_URLS="https://one.example.com/Account/Login,https://two.example.com/Account/Login" floor2plan-smoke-tests
```

Run in Docker with a mounted Edge profile:

```sh
docker run --rm \
  -e CYPRESS_EDGE_USER_DATA_DIR=/edge-profile \
  -e CYPRESS_EDGE_PROFILE_DIRECTORY=Default \
  -v "$HOME/.config/microsoft-edge:/edge-profile" \
  floor2plan-smoke-tests npm run test:smoke:edge
```

Host Edge profile reuse in Docker can be limited by OS keychain encryption and profile locks. Close Edge before running, and prefer running `npm run test:smoke:edge` on the same desktop user session when possible.

## Environment variables

- `TARGET_URLS`: comma-separated login URLs. Takes precedence over `TARGET_URL`.
- `TARGET_URL`: single login URL.
- `CYPRESS_EDGE_USER_DATA_DIR`: Edge user-data directory to reuse for Microsoft SSO.
- `CYPRESS_EDGE_PROFILE_DIRECTORY`: Edge profile directory, for example `Default` or `Profile 1`.
- `SMOKE_HOME_TILE_SELECTOR`: optional CSS selector for home-page tiles. Defaults to common tile selectors.
- `SMOKE_MIN_HOME_TILES`: minimum visible tiles expected on the home page. Defaults to `2`.
- `SMOKE_VISUAL_SETTLE_MS`: optional wait after the smoke assertion for video capture. Defaults to `0`.
- `CYPRESS_VIEWPORT_WIDTH`: viewport width. Defaults to `1280`.
- `CYPRESS_VIEWPORT_HEIGHT`: viewport height. Defaults to `720`.

The logo login path uses Azure AD. The runner must have a valid Microsoft SSO session for the target application; otherwise the smoke test will stop before the home tiles can render.
