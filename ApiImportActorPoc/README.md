# API Import Actor POC

Proof of concept modeled on `AkkaSignalRVuePoc`: Akka.NET actors build a complex project structure entirely in memory, expose it as JSON, and optionally persist it with EF Core.

## Domain model

- **Project** — top-level container
- **Component** — one or more per project; components can nest (composition)
- **Activity** — one or more per component
- **Assignment** — work a person performs on an activity
- **Activity relations** — child, predecessor, or successor links between activities

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
