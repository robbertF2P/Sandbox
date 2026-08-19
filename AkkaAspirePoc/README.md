# Akka Aspire POC

Proof-of-concept showing **Akka.NET** integrated with **.NET Aspire** via the official community plugin [`Aaron.Akka.Aspire`](https://www.nuget.org/packages/Aaron.Akka.Aspire.Hosting), plus an Angular frontend, SQL Server on Linux, EF Core migrations, Sentry performance monitoring, and **TUnit** tests.

## Stack

| Layer | Technology |
|-------|------------|
| Orchestration | .NET Aspire 13 AppHost |
| Akka cluster | `Aaron.Akka.Aspire.Hosting` + `Aaron.Akka.Discovery.Redis` |
| API | ASP.NET Core minimal APIs + Akka.Hosting actors |
| Database | SQL Server 2022 (Linux container) + EF Core 10 |
| Frontend | Angular 19 |
| Observability | Sentry (`Sentry.AspNetCore`) + Aspire OpenTelemetry defaults |
| Tests | TUnit 1.7 (domain, EF Core, Akka actor) |

## Architecture

```
AppHost
├── sql (SQL Server 2022 Linux) → todosdb
├── redis (Akka cluster discovery)
├── api (Akka.NET + EF Core + Sentry)
└── web (Angular, proxies to api)
```

Todo CRUD flows through a `TodoActor` that persists via EF Core. The AppHost wires Akka cluster bootstrap automatically — no manual HOCON.

## Prerequisites

- .NET 10 SDK
- Docker (for SQL Server + Redis containers)
- Node.js 20+ (for Angular dev server)
- Optional: [Aspire CLI](https://get.aspire.dev) for dashboard (or set `AspireUseCliBundle=false` as in this POC)

## Quick start

```bash
cd AkkaAspirePoc

# Run all services via Aspire AppHost
dotnet run --project AkkaAspirePoc.AppHost

# Or run tests (TUnit via Microsoft.Testing.Platform)
cd tests/AkkaAspirePoc.Tests && dotnet run
```

Open the Aspire dashboard URL printed in the **AppHost console** (default `https://localhost:17261` for the https profile).

**Docker is required.** The API expects SQL Server (`todosdb`) and Redis from AppHost — do not run the API standalone without those Aspire resources.

- **Portal (Angular)**: `http://localhost:4200` — links to dashboard, Sentry, API, and todos
- **API portal (HTML)**: `http://localhost:5080/` — same links served by the API
- **API**: `http://localhost:5080`
- **Todos**: `http://localhost:4200/todos`
- **Swagger/health**: `/health`, `/api/todos`, `/api/links`

## Sentry

Set your DSN and project URL in `AkkaAspirePoc.Api/appsettings.Development.json` or via user secrets / AppHost parameters:

```json
{
  "Sentry": {
    "Dsn": "https://<key>@o<org>.ingest.sentry.io/<project>",
    "ProjectUrl": "https://<org>.sentry.io/projects/<project>/",
    "TracesSampleRate": 1.0
  }
}
```

AppHost parameter: `sentry-project-url`. The Aspire dashboard URL is fixed in `AppHost.cs` (`https://localhost:17261`) and auto-detected when the API runs under AppHost.

Leave `Dsn` empty to disable Sentry locally. Sentry has no local UI — the portal links to your cloud project.

## EF Core migrations

```bash
dotnet ef migrations add <Name> \
  --project AkkaAspirePoc.Data.Migrations \
  --startup-project AkkaAspirePoc.Data.Migrations
```

Migrations are applied automatically on API startup via `TodoDatabaseInitializer`.

## Akka.NET Aspire plugin

This POC uses [Aaronontheweb/akka.net-aspire-plugin](https://github.com/Aaronontheweb/akka.net-aspire-plugin):

- **AppHost**: `AddAkka("todo-cluster").WithClustering(redis)` + `WithReference(akka)` on the API
- **Service**: `WithAspireClusterBootstrap()` + `WithRedisDiscovery()` — cluster ports, management, and bootstrap are injected by Aspire

## Tests (TUnit)

| Suite | What it covers |
|-------|----------------|
| `Domain/TodoRulesShould` | Validation rules |
| `Domain/TodoItemShould` | Aggregate behaviour |
| `Data/TodoDbContextShould` | EF Core persistence (SQLite in-memory) |
| `Actors/TodoActorShould` | Akka actor + EF integration |

```bash
cd tests/AkkaAspirePoc.Tests && dotnet run
```

> TUnit uses Microsoft.Testing.Platform. On .NET 10, prefer `dotnet run` in the test project over legacy `dotnet test` without MTP opt-in.
