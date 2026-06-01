# Floor2Plan smoke test harness

This harness runs a Cypress smoke test against one or more Floor2Plan login URLs. The default target is:

```text
https://2025-14-patch.floor2plan.com/Account/Login
```

## Run with npm

```sh
npm ci
npm run test:smoke
```

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

## Environment variables

- `TARGET_URLS`: comma-separated login URLs. Takes precedence over `TARGET_URL`.
- `TARGET_URL`: single login URL.
- `SMOKE_EMAIL`: email address typed into the login form. Defaults to `smoke@example.com`.
- `SMOKE_VISUAL_SETTLE_MS`: optional wait after the smoke assertion for video capture. Defaults to `0`.
- `CYPRESS_VIEWPORT_WIDTH`: viewport width. Defaults to `1280`.
- `CYPRESS_VIEWPORT_HEIGHT`: viewport height. Defaults to `720`.
