# Admin backoffice — control plane

Platform operator API for tenant provisioning. Persists tenant registry in a **control-plane SQL database** (EF Core) and pushes configuration to the **F2pPlatform** runtime via REST.

## Layout

```text
host/AdminBackoffice.Host/           composition root (port 5090)
src/ControlPlane.Application/        provision + sync use cases
src/ControlPlane.Infrastructure/     EF Core + platform HTTP client
src/ControlPlane.Api/                POST /admin/tenants, sync endpoints
src/ControlPlane.Data.Migrations/    EF migrations
```

Shared contracts: `../Platform.ControlPlane.Contracts/`

## Quick start

```bash
cd AdminBackoffice
docker compose up -d
dotnet ef database update --project src/ControlPlane.Data.Migrations
dotnet run --project host/AdminBackoffice.Host
```

- Backoffice API: `http://localhost:5090`
- Swagger: `/swagger`
- Control-plane SQL: `localhost:1403` (database `ControlPlane`)

Start **F2pPlatform** on `:5080` so provisioning can push tenant config:

```bash
cd ../F2pPlatform
docker compose up -d
dotnet run --project host/F2pPlatform.Host
```

## Flow

1. Operator `POST /admin/tenants` with slug, mode, packs, connection refs.
2. Backoffice writes tenant row (`status = provisioning`) to control-plane DB.
3. Backoffice `PUT` tenant config to F2pPlatform `http://localhost:5080/api/v1/platform/tenant-config`.
4. On success, tenant `status = active`, `lastSyncedToPlatformAt` set.

Re-sync: `POST /admin/tenants/{tenantId}/sync`

## Configuration

`appsettings.Development.json`:

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:ControlPlane` | EF Core SQL Server |
| `Platform:BaseUrl` | F2pPlatform host (default `http://localhost:5080`) |
| `Platform:ConfigurationApiKey` | Shared secret — must match F2pPlatform `Platform:ConfigurationApiKey` |
