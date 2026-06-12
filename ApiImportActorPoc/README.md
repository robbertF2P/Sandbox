# API Import Actor POC — Shipbuilding

Proof of concept modeled on `AkkaSignalRVuePoc`: Akka.NET actors import a **shipbuilding** work breakdown entirely in memory, expose it as JSON, and optionally persist it with EF Core.

## Domain model (shipbuilding)

- **Project** — a vessel new-build or refit (e.g. hull number, ship name)
- **Component** — hull blocks, sections, zones, or modules; components can nest (block → section → outfitting zone)
- **Activity** — construction or outfitting work (erection, welding, piping, painting, trials prep)
- **Assignment** — trade or role performing the work (welder, pipefitter, electrician, surveyor)
- **Activity relations** — scheduling links: child (sub-task), predecessor, successor (e.g. block erection before welding)

## Stack

- .NET 10, Akka.NET, ASP.NET Core minimal API, SignalR
- EF Core (SQLite by default)
- Vue 3 + Vite + TypeScript client

## Run the API

```bash
cd ApiImportActorPoc/server/ApiImportActorPoc.Api
dotnet run
```

API: `http://localhost:5001` — Swagger at `/swagger`, SignalR hub at `/hubs/import`.

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
| POST | `/api/import` | Start import (JSON body = project payload) |
| GET | `/api/import/{sessionId}/model` | Get built in-memory model as JSON |
| POST | `/api/import/{sessionId}/persist` | Save model to EF Core database |

SignalR event: `importEvent` — `ImportStarted`, `ImportProgressUpdated`, `ImportCompleted`, `ImportFailed`, `ImportPersisted`.

## Tests

```bash
cd ApiImportActorPoc
dotnet test ApiImportActorPoc.slnx
```
