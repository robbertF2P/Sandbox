# Akka.NET + SignalR + Vue POC

This proof-of-concept contains:

- `server/AkkaSignalRVuePoc.Api`: an ASP.NET Core executable API host with a SignalR hub at `/hubs/live-messages`.
- An Akka.NET actor system started by `AkkaActorHostedService`.
- `FrontendPushActor`, which publishes an `actorMessage` event to all SignalR clients immediately at startup and then every five seconds.
- `client`: a Vite + Vue + TypeScript frontend that connects to the hub and renders the live stream.

## Run the API

```bash
cd AkkaSignalRVuePoc/server/AkkaSignalRVuePoc.Api
dotnet run
```

The API listens on `http://localhost:5000` and allows the Vue dev origin `http://localhost:5173` by default.

## Run the Vue client

```bash
cd AkkaSignalRVuePoc/client
npm install
npm run dev
```

Open the Vite URL, usually `http://localhost:5173`, to see actor messages arrive every five seconds.

To point the client at a different hub URL, copy `.env.example` to `.env.local` and edit `VITE_SIGNALR_HUB_URL`.

## Run the actor tests

```bash
cd AkkaSignalRVuePoc
dotnet test AkkaSignalRVuePoc.slnx --logger "console;verbosity=detailed"
```

The actor tests use xUnit with `Akka.TestKit.Xunit2`. Test loggers are created with Serilog and write to the xUnit output stream via `Serilog.Sinks.XUnit`, so actor logs appear with the same Serilog console-style output template used by the API.
