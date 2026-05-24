# Akka.NET + SignalR + Vue POC

This proof-of-concept contains:

- `server/AkkaSignalRVuePoc.Api`: an ASP.NET Core executable API host with a SignalR hub at `/hubs/live-messages`.
- `server/AkkaSignalRVuePoc.Core`: Akka.NET actors and publisher abstractions.
- `server/AkkaSignalRVuePoc.Contracts`: actor system messages and events that plugins can consume.
- `server/AkkaSignalRVuePoc.Data`: EF Core entity models and `CatalogDbContext`.
- `server/AkkaSignalRVuePoc.Data.Migrations`: EF Core migrations (SQL Server).
- `DataManagerActor` (child of the root actor) routes catalog commands to per-entity data actors that use `IDbContextFactory<CatalogDbContext>`.
- An Akka.NET actor system started by `AkkaActorHostedService`.
- `FrontendPushActor`, which publishes an `actorMessage` event to all SignalR clients immediately at startup and then every five seconds.
- `client`: a Vite + Vue + TypeScript frontend that connects to the hub and renders the live stream.

## Run the API

Local development expects SQL Server on `localhost:1433` (see Docker Compose below) with the connection string in `server/AkkaSignalRVuePoc.Api/appsettings.json`. Migrations run automatically at startup.

```bash
cd AkkaSignalRVuePoc/server/AkkaSignalRVuePoc.Api
dotnet run
```

The API listens on `http://localhost:5000` and allows the Vue dev origin `http://localhost:5173` by default. Seed data includes **Acme Corp** and **Driven IT** with sample projects.

### EF Core migrations

```bash
cd AkkaSignalRVuePoc
dotnet ef migrations add <Name> \
  --project server/AkkaSignalRVuePoc.Data.Migrations/AkkaSignalRVuePoc.Data.Migrations.csproj \
  --startup-project server/AkkaSignalRVuePoc.Api/AkkaSignalRVuePoc.Api.csproj
```

## Run the Vue client

```bash
cd AkkaSignalRVuePoc/client
npm install
npm run dev
```

Open the Vite URL, usually `http://localhost:5173`, to see actor messages arrive every five seconds.

To point the client at a different hub URL, copy `.env.example` to `.env.local` and edit `VITE_SIGNALR_HUB_URL`.


## Run with Docker Compose

From a machine with Docker Desktop running:

```bash
cd AkkaSignalRVuePoc
docker compose up --build
```

Then open `http://localhost:5173`. The compose file starts:

- `api`: ASP.NET Core, Akka.NET, Serilog, and SignalR on `http://localhost:5000`
- `client`: the built Vue app served by nginx on `http://localhost:5173`

The Vue image is built with `VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs/live-messages`, which is the URL your browser uses to connect back to the API on your laptop. Stop the stack with `Ctrl+C`, or run `docker compose down` from the same folder.

## Run the actor tests

```bash
cd AkkaSignalRVuePoc
dotnet test AkkaSignalRVuePoc.slnx --logger "console;verbosity=detailed"
```

The actor tests use xUnit v3 with `Akka.TestKit.Xunit`. Test loggers use Serilog and write to the xUnit output stream via `Serilog.Sinks.XUnit3`.
