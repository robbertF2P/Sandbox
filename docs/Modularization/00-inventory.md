# Floor2Plan.Core — Repository Inventory (Phase 0)

**Generated:** 2026-06-24
**Target Framework:** .NET 10.0
**Codebase Size:** 100+ production projects + 20+ test projects

---

## 1. Solutions and Projects

### Solution Files

| Solution | Location | Purpose |
|----------|----------|---------|
| Floor2Plan.sln | `Floor2Plan.sln` | Main production solution, 100+ projects |
| Floor2Plan.DatabaseDeployer.sln | `Floor2Plan.DatabaseDeployer.sln` | Database migration & deployment solution |
| Floor2Plan.Selenium.sln | `Test/EndToEnd/Floor2Plan.Selenium.sln` | End-to-end UI testing (Selenium) |

### Target Framework

- **Primary:** .NET 10.0 (net10.0)
- **Architecture:** 64-bit (x64) preferred, AnyCPU support
- **Runtime ID:** win-x64
- **Language Version:** Latest C#

---

## 2. Folder Structure / Logical Layers

### Root-Level Organization

```
Floor2Plan.Core/
├── Src/                    # Source code
│   ├── UI/                 # User Interface Layer
│   ├── Application/        # Application/Business Logic
│   ├── Domain/             # Domain Model & Services
│   ├── Data/               # Data Access & Repositories
│   ├── Infrastructure/     # Cross-cutting Concerns
│   ├── Common/             # Shared Utilities & Base Classes
│   ├── Connectors/         # External System Integrations
│   ├── Contracts/          # DTOs, API Contracts, Domain Contracts
│   ├── DatabaseDeployer/   # Database Migration/Deployment Tool
│   ├── Distributions/      # Report Distribution Logic
│   └── Processors/         # Background Processing Jobs
├── Test/                   # Test Projects
│   ├── UnitTest/           # Unit test projects
│   ├── IntegrationTest/    # Integration tests
│   ├── DataTest/           # Database-specific tests
│   ├── EndToEnd/           # Selenium UI automation
│   └── Utility/            # Test utilities & test bases
├── Build/                  # Build scripts & props files
├── Pipelines/              # CI/CD pipeline definitions
├── DatabaseScripts/        # Raw SQL migration scripts
└── Artifacts/              # Build output directory
```

### UI Layer (Src/UI)

| Project | Type | Purpose |
|---------|------|---------|
| Floor2Plan.Startup | Library (Web SDK) | ASP.NET Core application bootstrap, DI configuration |
| UI.Floor2Plan | Web App | Primary MVC web application (Razor views) |
| Floor2Plan.Api | Library (Web SDK) | RESTful API layer with OpenAPI/Swagger, OData support |
| UI.Web.Common | Library | Shared web utilities, base controllers, middleware |
| UI.Floor2PlanClient | esproj | Angular/TypeScript frontend client (compiled to wwwroot) |

### Application Layer (Src/Application)

| Project | Type | Purpose |
|---------|------|---------|
| Application.Service | Library | Core business logic, domain operations |
| Application.Sync | Library | Data import/sync pipelines (Excel, connectors) |
| Application.SystemActions | Library | System-wide background operations, cleanup jobs |

### Domain Layer (Src/Domain)

| Project | Type | Purpose |
|---------|------|---------|
| Domain.Model | Library | Core domain entities, value objects |
| Domain.Service | Library | Domain-specific services, business rules |
| Domain.Model.Extensions | Library | Extension methods, helpers for domain model |
| Domain.Model.Mapping | Library | AutoMapper profiles, model-to-model mappings |

### Data/Persistence Layer (Src/Data)

| Project | Purpose |
|---------|---------|
| Data | Core repository interfaces, query services |
| Data.Base | Abstract base DbContext, common EF Core setup |
| Data.Base.Contracts | IRepository, IUnitOfWork interfaces |
| Data.Model | EF Core entity models, mappings |
| Data.EntityFramework | Floor2PlanDbContext, main EF implementation |
| Data.ChangeHandlers | EF Core change tracking handlers, auditing |
| Data.ChangeHandlers.Contracts | IChangeHandler interfaces |
| Data.OData.EntityFramework | OData query provider, custom OData operations |
| Data.OData.DbMigrations | OData-specific migrations |
| Data.Reporting | Reporting-specific data aggregations |
| Data.Reporting.EntityFramework | ReportingDbContext for BI/analytics |
| Data.DbMigrations | EF Core Migrations (main schema versioning) |
| Data.Reporting.DbMigrations | Reporting schema migrations |

### Infrastructure Layer (Src/Infrastructure)

| Subsystem | Projects | Purpose |
|-----------|----------|---------|
| **Authorization** | Infrastructure.Authorisation, Infrastructure.Authorisation.Contracts, Infrastructure.Authorisation.EntityFramework, Infrastructure.Authorisation.DbMigrations | Identity, RBAC, permissions |
| **Hangfire Jobs** | Infrastructure.Hangfire, Infrastructure.Hangfire.Contracts, Infrastructure.Hangfire.DbMigrations | Background job scheduling, recurring tasks, job state tracking |
| **Elsa Workflow** | Infrastructure.Elsa, Infrastructure.Elsa.EntityFramework, Infrastructure.Elsa.DbMigrations | Workflow engine, process automation, Hangfire integration |
| **Health Checks** | Infrastructure.Health, Infrastructure.Health.Contracts, Infrastructure.Health.EntityFramework, Infrastructure.Health.DbMigrations, Infrastructure.Health.Utility | Database health, application readiness probes |
| **File Management** | Infrastructure.Files, Infrastructure.Files.Contracts, Infrastructure.Files.EntityFramework, Infrastructure.Files.DbMigrations | File upload/download, storage abstraction |
| **Audit Logging** | Infrastructure.Audit, Infrastructure.Audit.EntityFramework, Infrastructure.Audit.DbMigrations | Entity change auditing, compliance tracking |
| **Configuration** | Infrastructure.Configuration.Startup, Infrastructure.Configuration.Startup.Contracts, Infrastructure.Configuration.Runtime, Infrastructure.Configuration.Runtime.Contracts, Infrastructure.Configuration.Runtime.EntityFramework, Infrastructure.Configuration.Runtime.DbMigrations | Startup & runtime app settings |
| **Process Execution** | Infrastructure.Process, Infrastructure.Process.Contracts, Infrastructure.Process.EntityFramework, Infrastructure.Process.DbMigrations | Long-running process management |
| **Logging** | Infrastructure.Logging | Serilog integration, structured logging, middleware |
| **State Management** | Infrastructure.State | In-memory state service |
| **Fire-and-Forget** | Infrastructure.FireAndForget, Infrastructure.FireAndForget.Contracts | Async fire-and-forget job execution |
| **Processors** | Infrastructure.Processors | Processor orchestration, dependency resolution |
| **Webservice** | Infrastructure.Webservice | HTTP client factory, external API calls |
| **Workflow Activities** | Infrastructure.Workflow, Infrastructure.Workflow.Contracts | Elsa workflow custom activities, task definitions |

### Common/Shared Layer (Src/Common)

| Project | Purpose |
|---------|---------|
| Common.Utility | Helpers, extensions, utilities (F2PModule base class) |
| Common.Domain | Shared domain abstractions, entity bases |
| Common.Data | Shared data utilities, connection factories |
| Common.Database | Database utilities, schema helpers, connection strings |
| Common.Security | Cryptography, permission definitions, security helpers |
| Common.Localization | Multi-language resource management |
| Common.Mail.Templates | Email template models, localized mail content |

### Connectors/Integrations (Src/Connectors & Src/Distributions)

| Project | Purpose |
|---------|---------|
| DocumentStore | External document storage abstraction, connector pattern |
| DocumentStore.EntityFramework | Document metadata persistence |
| DocumentStore.DbMigrations | Document store schema migrations |
| Distributions.EntityFramework | Report distribution tracking |
| Distributions.DbMigrations | Distribution schema migrations |

### Contracts/DTOs (Src/Contracts)

| Project | Purpose |
|---------|---------|
| Contracts.Application | Application service DTOs, service contracts |
| Contracts.Domain | Domain models exposed externally |
| Contracts.Infrastructure | Infrastructure service interfaces (auth, config, etc.) |
| Contracts.Infrastructure.Connectors | External connector interfaces |
| Contracts.UI | UI-specific models, view models |
| Contracts.Model | Shared contract models |

### Processors (Src/Processors)

| Project | Purpose |
|---------|---------|
| Processors.Domain | Background processors (refresh caches, generate reports, cleanup) |
| Processors.Contracts | Processor interfaces, execution contracts |

### Database Deployer (Src/DatabaseDeployer)

| Project | Purpose |
|---------|---------|
| DatabaseDeployer | Console app that runs database migrations |
| DatabaseDeployer.Domain | Deployment logic, migration orchestration |
| DatabaseDeployer.Data | Deployment-specific data queries |
| DatabaseDeployer.Contracts | Deployment interfaces |

---

## 3. Integration Points

### ASP.NET Controllers & MVC

**Traditional MVC Controllers:**
- `Src/UI/UI.Web.Common/BaseController.cs` — Base controller with common methods
- `Src/UI/UI.Floor2Plan/Controllers/ErrorController.cs` — Error handling
- `Src/UI/UI.Floor2Plan/Areas/Devices/Controllers/ShopFloorTerminalController.cs` — Devices management
- `Src/UI/UI.Floor2Plan/Areas/Devices/Controllers/ClockingTerminalController.cs` — Terminal operations
- `Src/UI/UI.Floor2Plan/Areas/Plan/Controllers/HolidayController.cs` — Holiday management

**API Controllers:**
- `Src/UI/Floor2Plan.Api/Controllers/TimesheetController.cs` — `[ApiController]` — RESTful timesheet endpoints
- `Src/UI/Floor2Plan.Api/Controllers/ImportController.cs` — `[ApiController]` — Data import endpoints

### Minimal APIs

**Not used.** Architecture uses traditional MVC controllers and OData.

### Message Bus / Event Handlers

**No NServiceBus or MediatR.** Elsa Workflow + Hangfire are the async/messaging mechanisms.

**Elsa Workflow custom activities** (`Src/Infrastructure/Infrastructure.Workflow/Activities/`):
- `TicketSendMailActivity`, `TicketActivityCreateActivity`, `TicketActivityCloseActivity`
- `SyncLogSendMailActivity`, `RunConnectorActivity`
- `SetActivityStatusActivity`, `ActivityStatusGateActivity`
- `RunProcessorActivity`

### Hangfire Background Jobs

**Infrastructure:** `Src/Infrastructure/Infrastructure.Hangfire/Floor2PlanInfrastructureHangfireModule.cs`

| Component | Purpose |
|-----------|---------|
| JobStateEventHub | Real-time job state tracking via SignalR |
| JobStateEventHostedService | IHostedService implementation for Hangfire server lifecycle |
| StateAppliedForwardFilter | Custom Hangfire filter |

**Queue setup:** `default` queue (1 worker) + `sync` queue (1 worker)
**Storage:** SQL Server (production) / In-Memory (testing)

**Job areas:**
- Recurring jobs: `Application.Sync` (Excel import jobs)
- System-wide background: `Application.SystemActions`
- Processor jobs: `Processors.Domain` (cache refresh, reporting, cleanup)

### IHostedService / BackgroundService

| Service | Purpose | Location |
|---------|---------|----------|
| JobStateEventHostedService | Hangfire server lifecycle | Infrastructure.Hangfire |
| DelayedHealthCheckPublisherHostedService | Delayed health check publishing | Infrastructure.Health |
| HostedServiceLogScopeDecorator | Adds logging scope to hosted services | Infrastructure.Logging |

### SignalR Hubs

| Hub | Purpose | Location |
|-----|---------|----------|
| JobStateEventHub | Real-time job status updates | `Src/Infrastructure/Infrastructure.Hangfire/JobStateEventHub.cs` |
| WorkflowInstanceProxyHub | Workflow execution status updates | `Src/Infrastructure/Infrastructure.Elsa/UI/WorkflowInstanceProxyHub.cs` |

[NEEDS REVIEW] Full SignalR endpoint mapping configuration — check Elsa + Hangfire module startup.

### File Import/Export Pipelines

**Excel Import System** (`Src/Application/Application.Sync/`):
- Base class: `ImportProvider/Base/BaseImportProvider.cs`
- Providers for: UnitType, ProjectStatus, ProjectMaterial, Person, Organisation, Material, HourType, Discipline, ComponentType, ActivityStatus, ActivityPhase
- Jobs: `ImportJobs/` (Hangfire-scheduled recurring)
- Dynamic resolver: `ImportProviderResolver` selects importer by model type

**File Storage:**
- Abstract connector pattern in `Src/Connectors/DocumentStore`
- Metadata tracked in `Src/Connectors/DocumentStore.EntityFramework`

---

## 4. DbContext / DataContext Implementations

### Primary DbContexts

| DbContext | Location | Purpose |
|-----------|----------|---------|
| **Floor2PlanDbContext** | `Src/Data/Data.EntityFramework/Floor2PlanDbContext.cs` | Main application data — 100+ DbSets (Activities, Assignments, Components, Allocations, Projects, Personnel, Disciplines, Materials, WorkSchedules, Baselines, AssignmentSummaries, ActivitySummaries, ActivityRelations, and more) |
| **ReportingDbContext** | `Src/Data/Data.Reporting.EntityFramework/ReportingDbContext.cs` | BI/Analytics data warehouse — aggregated reporting views |
| **ODataDbContext** | `Src/Data/Data.OData.EntityFramework/ODataDbContext.cs` | OData query service — entity projections for OData endpoints |
| **HangfireDbContext** | `Src/Infrastructure/Infrastructure.Hangfire.DbMigrations/HangfireDbContext.cs` | Background job storage — job state, parameters, recurring jobs |

### Base Contexts & Factories

| Type | Location | Purpose |
|------|----------|---------|
| BaseDbContext | `Src/Data/Data.Base/BaseDbContext.cs` | Abstract base for all DbContexts; audit logging, soft deletes |
| BaseChangeHandlerDbContext | `Src/Data/Data.Base/BaseChangeHandlerDbContext.cs` | Change tracking, domain event publishing |
| DbContextFactory | `Src/Data/Data.Base/DbContextFactory.cs` | DbContext instantiation, connection management |
| BaseMigrationDbContext | `Src/DatabaseDeployer/DatabaseDeployer.Data/BaseMigrationDbContext.cs` | Base for migration runner contexts |

### DbContext Hierarchy

```
BaseDbContext
├── BaseChangeHandlerDbContext<Floor2PlanDbContext>
│   └── Floor2PlanDbContext  (main app — 100+ entities)
├── ReportingDbContext
├── ODataDbContext
├── HangfireDbContext
└── (per-feature infrastructure contexts via respective *.EntityFramework projects)
```

**Pattern:** Generic `BaseChangeHandlerDbContext<T>` triggers change handlers automatically on SaveChanges.

[NEEDS REVIEW] Full entity count per DbContext — enumerate DbSet<T> properties in Floor2PlanDbContext to get exact count.

---

## 5. Authentication & Authorization

### Authentication Packages

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.6 | JWT bearer token validation |
| Microsoft.AspNetCore.Authentication.OpenIdConnect | 10.0.6 | OpenID Connect / Azure AD |
| Microsoft.Identity.Web | 4.7.0 | Microsoft Identity platform integration |
| Volo.Abp.AspNetCore.Authentication.OpenIdConnect | 10.3.0 | ABP framework auth abstraction |

### Authorization & Permissions

- **Infrastructure:** `Src/Infrastructure/Infrastructure.Authorisation/` (RBAC, policy persistence)
- **Contracts:** `Src/Infrastructure/Infrastructure.Authorisation.Contracts/`
- **Migrations:** `Src/Infrastructure/Infrastructure.Authorisation.DbMigrations/`
- **Permission definitions:** `Src/Common/Common.Security/Permissions/ModulePermissions.cs`

[NEEDS REVIEW] Full permission matrix and [Authorize] policy names — examine Infrastructure.Authorisation for complete list.

---

## 6. Existing Test Projects

### Unit Tests

| Project | Tests Target | Framework |
|---------|-------------|-----------|
| Floor2Plan.UnitTest.Application.Service | Application.Service | xUnit + Moq |
| Floor2Plan.UnitTest.Application.SystemActions | Application.SystemActions | xUnit |
| Floor2Plan.UnitTest.Application.Sync | Application.Sync | xUnit |
| Floor2Plan.UnitTest.Common | Common.* | xUnit |
| Floor2Plan.UnitTest.Data | Data.* | xUnit |
| Floor2Plan.UnitTest.Domain.Service | Domain.Service | xUnit |
| Floor2Plan.UnitTest.Domain.Model.Mapping | Domain.Model.Mapping | xUnit |
| Floor2Plan.UnitTest.Infrastructure | Infrastructure.* | xUnit |
| Floor2Plan.UnitTest.Infrastructure.Health | Infrastructure.Health | xUnit |
| Floor2Plan.UnitTest.Infrastructure.Workflow | Infrastructure.Workflow | xUnit |
| Floor2Plan.UnitTest.Processors.Domain | Processors.Domain | xUnit |
| Floor2Plan.UnitTest.UI.Floor2Plan | UI.Floor2Plan (MVC) | xUnit |
| Floor2Plan.UnitTest.UI.Floor2Plan.Mvc | UI.Floor2Plan (MVC) | xUnit |
| Floor2Plan.UnitTest.UI.Web.Common | UI.Web.Common | xUnit |
| Floor2Plan.UnitTest.Ui.Api | Floor2Plan.Api | xUnit |
| UnitTest.DatabaseDeployer | DatabaseDeployer | xUnit |

### Integration Tests

| Project | Purpose |
|---------|---------|
| Floor2Plan.IntegrationTest.Application | Application services + real database |
| Floor2Plan.IntegrationTest.Data | Data access, migrations, change handlers |
| Floor2Plan.IntegrationTest.Domain | Domain services + database |
| Floor2Plan.IntegrationTest.Infrastructure | Infrastructure services + database |
| Floor2Plan.IntegrationTest.Infrastructure.Workflow | Elsa workflow execution + database |
| Floor2Plan.IntegrationTest.UI | MVC application integration |

### End-to-End Tests

| Project | Framework |
|---------|-----------|
| Floor2Plan.Selenium | Selenium WebDriver |
| Floor2Plan.Selenium.Base | Selenium base classes |
| Floor2Plan.Selenium.Utilities | Selenium helpers |

### Test Utilities

| Project | Purpose |
|---------|---------|
| Floor2Plan.TestUtility.TestBase | Base test class, mocking utilities |
| Floor2Plan.TestUtility.WebTestBase | Web/MVC test base, TestHost |
| Floor2Plan.TestUtility.Common | Shared test utilities |
| Floor2Plan.TestUtility.Data | Database test helpers, seeding |
| Floor2Plan.UnitTest.TestUtility | Additional unit test utilities |

### Data Tests

| Project | Purpose |
|---------|---------|
| Floor2Plan.DataTest.Data | Database health checks, schema validation |

### Test Framework Stack

| Tool | Package | Version |
|------|---------|---------|
| Test runner | xUnit v3 + xUnit.v3.extensibility.core | Latest |
| Mocking | Moq | 4.20.72 |
| EF mocking | MockQueryable.Moq, Moq.EntityFrameworkCore | — |
| Assertions | AwesomeAssertions | 9.4.0 |
| Test host | Microsoft.AspNetCore.TestHost | — |
| Coverage | Microsoft.Testing.Extensions.CodeCoverage | — |

---

## 7. Build Tooling & Project Configuration

### Central Build Files

| File | Purpose |
|------|---------|
| `Directory.Build.props` | Global assembly info, target framework, compiler settings, NuGet audit |
| `Directory.Build.targets` | Custom build targets, artifact paths |
| `Directory.Packages.props` | Centralized NuGet package version management |
| `Build/Runnable.Build.props` | Configuration for runnable projects |
| `Build/Variables.Build.props` | Build variable definitions |
| `Build/Version.Build.props` | Assembly versioning |
| `BannedSymbols.txt` | Banned API definitions (Roslyn analyzer) |
| `Test/UnitTest/UnitTest.Build.props` | Unit test project config |
| `Test/IntegrationTest/IntegrationTest.Build.props` | Integration test config |
| `Test/DataTest/DataTest.Build.props` | Data test config |
| `global.json` | .NET SDK version pin |

### Build Settings

- **Target Framework:** net10.0
- **Language Version:** Latest C#
- **Platforms:** AnyCPU, x64
- **Build Output:** `Artifacts/` (centralized non-standard output path)
- **Deterministic Builds:** Enabled
- **Treat Warnings as Errors:** True
- **NuGet Audit:** Enabled (all, low+)

### CI/CD Pipelines (`Pipelines/`)

| Pipeline | Trigger | Purpose |
|----------|---------|---------|
| ci-feature-build.yml | Feature branches | Build validation |
| f2p-build-pull-request-core.yml | Pull requests | PR checks |
| f2p-build-continuous-integration-core.yml | Merges to main | Release builds |
| f2p-build-continuous-integration-core-data-tests.yml | Main/develop | Data test suite |

**Platform:** Azure Pipelines (YAML)

### Database Migrations

- **Tool:** EF Core Migrations per module (`*.DbMigrations` projects)
- **Provider:** Microsoft.EntityFrameworkCore.SqlServer 10.0.6
- **Deployer:** `DatabaseDeployer` console app orchestrates all migrations

---

## 8. Deprecated Patterns

| Pattern | Status |
|---------|--------|
| Web.config | Not present — fully .NET Core |
| packages.config | Not present — uses .csproj package references |
| app.config | Not present — uses appsettings.json |
| Castle Windsor | Not present — uses MS DI + Autofac (via ABP) |
| SOAP / WCF | Not present |
| Static dependency injection / service locator | Not present |

**No deprecated pre-.NET Core patterns detected.** Codebase is fully on .NET 10.

---

## 9. ABP Modular Architecture

The codebase uses **Volo.Abp Module pattern** with a custom `F2PModule` base class:

```
F2PModule (Common.Utility)
└── Floor2PlanStartupModule (Floor2Plan.Startup)
    ├── Data.EntityFramework (Floor2PlanDbContext)
    ├── Infrastructure.Hangfire (job scheduling)
    ├── Infrastructure.Elsa (workflow engine)
    ├── Application.Service (core business logic)
    ├── Application.Sync (import pipelines)
    ├── Application.SystemActions (system jobs)
    └── [50+ other feature modules...]
```

Each module has:
- `*Module.cs` extending `F2PModule`
- `[DependsOn(...)]` attributes declaring module dependencies
- `ConfigureServices()` for DI registration
- Separate `Contracts`, `EntityFramework`, and `DbMigrations` projects

### Key NuGet Package Versions

| Purpose | Package | Version |
|---------|---------|---------|
| DI & Modularity | Volo.Abp.Core, Volo.Abp.Autofac | 10.3.0 |
| Data Access | Microsoft.EntityFrameworkCore.SqlServer | 10.0.6 |
| Background Jobs | Volo.Abp.BackgroundJobs.HangFire, Hangfire | 10.3.0 / 1.8.23 |
| Workflow | Elsa, Elsa.Hangfire, Elsa.EntityFrameworkCore | 3.5.3 |
| Web Framework | Volo.Abp.AspNetCore.Mvc | 10.3.0 |
| Logging | Serilog, SerilogTracing | 4.3.1 / 2.3.1 |
| OData | Microsoft.AspNetCore.OData | 9.4.1 |
| Authentication | Microsoft.Identity.Web | 4.7.0 |
| Mapping | AutoMapper | 16.1.1 |
| Reporting | Telerik.Reporting | 18.2.24.806 |
| API Docs | Swashbuckle.AspNetCore | 10.1.5 |

---

## 10. Key File Locations

| File | Path | Purpose |
|------|------|---------|
| Startup Module | `Src/UI/Floor2Plan.Startup/Floor2PlanStartupModule.cs` | DI root configuration |
| Main DbContext | `Src/Data/Data.EntityFramework/Floor2PlanDbContext.cs` | 100+ entity sets |
| Hangfire Config | `Src/Infrastructure/Infrastructure.Hangfire/Floor2PlanInfrastructureHangfireModule.cs` | Job scheduler setup |
| Elsa Workflow | `Src/Infrastructure/Infrastructure.Elsa/Floor2PlanInfrastructureElsaModule.cs` | Workflow engine config |
| API Controllers | `Src/UI/Floor2Plan.Api/Controllers/` | REST endpoints, OData |
| MVC Controllers | `Src/UI/UI.Floor2Plan/Controllers/` | Web UI actions |
| Migrations | `Src/Data/Data.DbMigrations/Migrations/` | EF Core schema versions |
| Permission Defs | `Src/Common/Common.Security/Permissions/ModulePermissions.cs` | RBAC definitions |
| Database Deployer | `Src/DatabaseDeployer/DatabaseDeployer/Program.cs` | Migration runner |
| CI/CD Pipelines | `Pipelines/*.yml` | Azure DevOps definitions |

---

## 11. Items Requiring Human Review

| Item | Notes |
|------|-------|
| Full entity count in Floor2PlanDbContext | [NEEDS REVIEW] Enumerate all DbSet<T> properties for exact count |
| Full permission matrix | [NEEDS REVIEW] Enumerate all policy/permission names in Infrastructure.Authorisation + Common.Security |
| SignalR endpoint registration order | [NEEDS REVIEW] Full middleware + hub mapping chain in startup |
| OData custom function/action definitions | [NEEDS REVIEW] Enumerate all custom OData operations |
| ReportingDbContext full entity list | [NEEDS REVIEW] Telerik.Reporting 18.2.24.806 — full reporting architecture |
| Angular frontend build process | [NEEDS REVIEW] UI.Floor2PlanClient esproj — build integration with MSBuild |
| Custom middleware pipeline order | [NEEDS REVIEW] Full UseX() call order in startup |
| NuGet.config / custom feed configuration | [NEEDS REVIEW] Check for private feed config |

---

*Phase 0 complete. Proceed to Phase 1 (entry-point catalog) after tech lead review.*
