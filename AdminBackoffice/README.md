# Admin backoffice — control plane

Platform operator API and Angular UI for tenant provisioning. Persists tenant registry in a **control-plane SQL database** (EF Core) and pushes configuration to the **F2pPlatform** runtime via REST, orchestrated by **Akka.NET actors** (same pattern as `ApiImportActorPoc`).

## Layout

```text
host/AdminBackoffice.Host/           composition root (port 5090) + Dockerfile
src/ControlPlane.Application/        provision + sync use cases (domain services)
src/ControlPlane.Contracts/          actor messages + IControlPlaneActorFacade
src/ControlPlane.Core/               Akka actors (persist + platform sync)
src/ControlPlane.Infrastructure/     EF Core + platform HTTP client
src/ControlPlane.Api/                REST endpoints + Akka hosting
src/ControlPlane.Data.Migrations/    EF migrations
web/                                 Angular admin UI + Dockerfile (`@floorganise/css` reference implementation)
```

Shared contracts: `../Platform.ControlPlane.Contracts/`

## Quick start — full platform (Docker)

From repository root:

```bash
docker compose -f docker-compose.platform.yml up --build
```

- Admin UI: http://localhost:5190
- Admin API Swagger: http://localhost:5090/swagger

See `docs/platform-docker-stack.md` for the full stack.

## Quick start — local dev

```bash
cd AdminBackoffice
docker compose up -d
dotnet run --project host/AdminBackoffice.Host
cd web && npm install && npm start
```

- Backoffice API: `http://localhost:5090`
- Admin UI: `http://localhost:5190` (proxies `/admin` to API)
- Control-plane SQL: `localhost:1403` (database `ControlPlane`)

## Frontend styling

The admin UI is the **reference Angular app** for `@floorganise/css` on operator/backoffice screens:

- Global styles: `@import '@floorganise/css'` only — no parallel `admin-*` CSS
- Light shell: `f2p-app-light` + `bg-f2p-navbar` chrome
- Semantic chrome: `f2ps-btn-*`, `f2ps-group-box`, `login-notification-*`, `field`
- Layout/spacing: Tailwind utilities (`space-y-6`, `grid`, `flex`, responsive `sm:` variants)
- Skill: `.cursor/skills/tailwind-ui-styling/SKILL.md`

Start **F2pPlatform** on `:5080` so provisioning can push tenant config:

```bash
cd ../F2pPlatform
docker compose up -d
dotnet run --project host/F2pPlatform.Host
```

## Flow

1. Operator submits **New tenant** form in admin UI (or `POST /admin/tenants`).
2. `IControlPlaneActorFacade` asks `RootActor` → `TenantProvisioningManagerActor`.
3. Manager persists tenant (`status = provisioning`) via `TenantPersistActor`.
4. `PlatformSyncActor` `PUT`s tenant config to F2pPlatform `/api/v1/platform/tenant-config`.
5. On success, tenant `status = active`, `lastSyncedToPlatformAt` set.

Re-sync: **Sync** button in UI or `POST /admin/tenants/{tenantId}/sync`

## Configuration

`appsettings.Development.json`:

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:ControlPlane` | EF Core SQL Server |
| `Platform:BaseUrl` | F2pPlatform host (default `http://localhost:5080`) |
| `Platform:ConfigurationApiKey` | Shared secret — must match F2pPlatform `Platform:ConfigurationApiKey` |
| `Cors:AllowedOrigins` | Admin Angular dev server (default `http://localhost:5190`) |
