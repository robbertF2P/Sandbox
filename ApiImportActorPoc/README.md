# API Import Actor POC — Shipbuilding

Proof of concept modeled on `AkkaSignalRVuePoc`: Akka.NET actors import a **shipbuilding** work breakdown entirely in memory, expose it as JSON, and optionally persist it with EF Core.

## Domain model (shipbuilding)

- **Project** — a vessel new-build or refit (e.g. hull number, ship name)
- **Component** — hull blocks, sections, zones, or modules; components can nest (block → section → outfitting zone)
- **Activity** — construction or outfitting work (erection, welding, piping, painting, trials prep)
- **Assignment** — trade or role performing the work (welder, pipefitter, electrician, surveyor)
- **Activity relations** — scheduling links: child (sub-task), predecessor, successor (e.g. block erection before welding)
- **External ids** — flexible key/value pairs per entity (`"PLM": "BLOCK-204"`, `"SAP": "..."`) for cross-system identity

### External id rules

- Every entity (project, component, activity, assignment) may carry `externalIds: { "SystemName": "value" }`.
- Each `system:value` pair must be **unique across the entire model** (validated on import).
- After the first persist, entities keep an internal `int` identity (SQL Server `IDENTITY`); external ids are stored separately.
- Import **sessions** still use `Guid` (ephemeral, in-memory only).
- **Subsequent imports update** existing rows when any external id matches (upsert, not duplicate insert).
- Activity relations may reference `system:value` (e.g. `"PLM:ACT-WELD"`).

## Stack

- .NET 10, Akka.NET, ASP.NET Core minimal API, SignalR
- EF Core (SQLite by default)
- Vue 3 + Vite + TypeScript client

## Run the API

Local development uses SQLite (`import.db`) by default. Serilog writes to the console and, when configured, to [Seq](https://datalust.co/seq).

```bash
cd ApiImportActorPoc/server/ApiImportActorPoc.Api
dotnet run
```

API: `http://localhost:5001` — Swagger at `/swagger`, SignalR hub at `/hubs/import`.

### Serilog + Seq (local)

Start only Seq from compose (or full stack below):

```bash
cd ApiImportActorPoc
docker compose up seq -d
```

Seq UI: `http://localhost:8082` — log ingest: `http://localhost:5342` (set in `appsettings.Development.json`).

### SQL Server (local)

```bash
docker compose up sqlserver -d
```

Then set in `appsettings.Development.json` (or user secrets):

```json
"ConnectionStrings": {
  "Import": "Server=localhost,1401;Database=ApiImportPoc;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True"
},
"Database": { "Provider": "SqlServer" }
```

## Run with Docker Compose

Starts Seq, SQL Server 2022 (Linux), API, and Vue client:

```bash
cd ApiImportActorPoc
docker compose up --build
```

| Service | URL |
|---------|-----|
| Vue client | http://localhost:5174 |
| API | http://localhost:5001 |
| Seq UI | http://localhost:8082 |
| SQL Server | `localhost,1401` (sa / `Your_strong_password123`) |

The API logs to Seq at `http://seq:5341` inside the compose network. Migrations run automatically on startup.

## Run the Vue client

```bash
cd ApiImportActorPoc/client
npm install
npm run dev
```

Open `http://localhost:5174`. Copy `.env.example` to `.env.local` to override API URLs.

## API

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/projects` | List persisted vessel projects |
| GET | `/api/projects/{id}` | Full project tree (components, activities, assignments) |
| GET | `/api/projects/{id}/export` | Export as import JSON (round-trip test) |
| GET | `/api/projects/{id}/progress` | Budgeted vs worked hours rolled up to project |
| GET | `/api/assignments` | List assignments with project context |
| POST | `/api/assignments/{id}/hours` | Book worked hours on an assignment |
| POST | `/api/import` | Start import (JSON body = project payload) |
| GET | `/api/import/{sessionId}/model` | Get built in-memory model as JSON |
| POST | `/api/import/{sessionId}/persist` | Save model to EF Core database |

SignalR event: `importEvent` — `ImportStarted`, `ImportProgressUpdated`, `ImportCompleted`, `ImportFailed`, `ImportPersisted`.

## Vue client pages

| Route | Purpose |
|-------|---------|
| `/` | Import JSON, watch actor progress, persist |
| `/projects` | List persisted projects |
| `/projects/new` | Create/edit a project structure in memory |
| `/projects/{id}` | Edit lists + **Export JSON** for import testing |
| `/projects/{id}/progress` | Progress bars: budgeted vs worked hours at every level |
| `/book-hours` | Book hours on any assignment |
| `/projects/{id}/book-hours` | Book hours filtered to one project |

Round-trip: edit on **Projects** → Export → **Import** → Persist → view on **Projects** again.

## Thunder Client (VS Code)

Install the [Thunder Client](https://marketplace.visualstudio.com/items?itemName=rangav.vscode-thunder-client) extension.

This repo includes a ready-made collection in `thunder-tests/`:

1. Open the `ApiImportActorPoc` folder in VS Code.
2. Thunder Client should pick up `thunder-tests/thunderCollection.json` when workspace save is enabled (see `.vscode/settings.json`).
3. Or use **Collections → Menu → Import** and select `thunder-tests/thunderCollection.json`.
4. Select the **Local API** environment (`baseUrl` = `http://localhost:5001`).
5. Run **Start import** → copy `sessionId` from the response into the `sessionId` env var → **Get import model** / **Persist import**.

Replace `projectId` in the environment after listing or persisting a project.

## Tests

```bash
cd ApiImportActorPoc
dotnet test ApiImportActorPoc.slnx
```
