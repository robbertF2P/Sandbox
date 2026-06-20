# Floor2Plan.Core2 — Repository Inventory (Phase 0)

> Generated: 2026-06-18
> Branch: `feature-main/release-candidate`
> Purpose: Pre-modularization baseline inventory. No recommendations included.

---

## Table of Contents

1. [Solutions, Entry Points, and Projects](#1-solutions-entry-points-and-projects)
2. [Folder Structure and Logical Layers](#2-folder-structure-and-logical-layers)
3. [Integration Points](#3-integration-points)
4. [DbContext / DataContext Inventory](#4-dbcontext--datacontext-inventory)
5. [Authentication and Authorization](#5-authentication-and-authorization)
6. [Test Projects](#6-test-projects)
7. [Build Tooling, Target Frameworks, and Deprecated Patterns](#7-build-tooling-target-frameworks-and-deprecated-patterns)

---

## 1. Solutions, Entry Points, and Projects

### Solution Files

| File | Purpose |
|---|---|
| `Floor2Plan.sln` | Main application solution — contains all Src and Test projects |
| `Floor2Plan.DatabaseDeployer.sln` | Standalone deployer solution — subset for DB migration tooling |
| `Test/EndToEnd/Floor2Plan.Selenium.sln` | End-to-end Selenium test solution |

### Web Entry Points (Microsoft.NET.Sdk.Web)

| Project (csproj) | SDK | OutputType | Description |
|---|---|---|---|
| `Src/UI/UI.Floor2Plan/UI.Floor2Plan.csproj` | `Microsoft.NET.Sdk.Web` | (default: web app) | Main ASP.NET Core MVC + Vue SPA host. Razor views + Vite front-end. The runnable application entry point. |
| `Src/UI/Floor2Plan.Api/Floor2Plan.Api.csproj` | `Microsoft.NET.Sdk.Web` | `Library` | API library (not standalone runnable). Contains OData and versioned REST controllers. Compiled as library and loaded by UI.Floor2Plan. |

### Console Applications

| Project (csproj) | OutputType | RuntimeIdentifiers | Description |
|---|---|---|---|
| `Src/DatabaseDeployer/DatabaseDeployer/DatabaseDeployer.csproj` | `Exe` | `win-x64;linux-x64` | Standalone database migration runner. Cross-platform. |

### Class Libraries (Src — all target `net10.0` via `Directory.Build.props`)

All projects below use `Microsoft.NET.Sdk` and compile as class libraries unless noted above.

#### Application Layer

| Project | Description |
|---|---|
| `Src/Application/Application.Service/Application.Service.csproj` | Core application services (224+ .cs files). Contains providers for all domain entities, import helpers, report generation, Hangfire job wiring. |
| `Src/Application/Application.Sync/Application.Sync.csproj` | Sync/import orchestration. Import providers for Excel/CSV/XER/XML/Sciforma/Aspose formats. Import jobs executed via Hangfire. |
| `Src/Application/Application.SystemActions/Application.SystemActions.csproj` | 40+ named system actions (cache refresh, reindex, recalculation) exposed as callable admin operations. |

#### Common Layer

| Project | Description |
|---|---|
| `Src/Common/Common.Data/Common.Data.csproj` | Data helpers, transaction helpers |
| `Src/Common/Common.Database/Common.Database.csproj` | Contains `EmptyDbContext` (plain `DbContext` subclass, used as placeholder) |
| `Src/Common/Common.Domain/Common.Domain.csproj` | Domain primitives and shared domain logic |
| `Src/Common/Common.Localization/Common.Localization.csproj` | Localization abstractions |
| `Src/Common/Common.Mail.Templates/Common.Mail.Templates.csproj` | Razor/HTML e-mail templates |
| `Src/Common/Common.Security/Common.Security.csproj` | Permission definitions (`ModulePermissions`), Argon2 password hashing |
| `Src/Common/Common.Utility/Common.Utility.csproj` | General utilities, framework base classes (`F2PModule`), extension methods |

#### Connectors (external document store integration)

| Project | Description |
|---|---|
| `Src/Connectors/DocumentStore/DocumentStore.csproj` | Document store abstraction |
| `Src/Connectors/DocumentStore.EntityFramework/DocumentStore.EntityFramework.csproj` | EF Core `DocumentStoreDbContext` — entity: `DsFileContent` |
| `Src/Connectors/DocumentStore.DbMigrations/DocumentStore.DbMigrations.csproj` | EF migrations for document store |

#### Contracts Layer (pure interfaces / DTOs)

| Project | Description |
|---|---|
| `Src/Contracts/Contracts.Application/Contracts.Application.csproj` | Application-layer interfaces |
| `Src/Contracts/Contracts.Domain/Contracts.Domain.csproj` | Domain-layer interfaces, unit-of-work contracts |
| `Src/Contracts/Contracts.Infrastructure/Contracts.Infrastructure.csproj` | Infrastructure interfaces |
| `Src/Contracts/Contracts.Infrastructure.Connectors/Contracts.Infrastructure.Connectors.csproj` | Connector interfaces |
| `Src/Contracts/Contracts.Model/Contracts.Model.csproj` | Shared model contracts (includes `IF2PEntity` — marker for DbContext entities) |
| `Src/Contracts/Contracts.UI/Contracts.UI.csproj` | UI contracts (import provider interfaces, sync sheet context, report interfaces) |

#### Data Layer

| Project | Description |
|---|---|
| `Src/Data/Data.Model/Data.Model.csproj` | EF entity model classes (POCO entities) |
| `Src/Data/Data.Base/Data.Base.csproj` | `BaseDbContext<T>`, `BaseChangeHandlerDbContext<T>`, `InvokableDbContext`, `DbContextFactory`, `ChangeHandlerRunnerAsync`, health checks |
| `Src/Data/Data.Base.Contracts/Data.Base.Contracts.csproj` | Contracts for `Data.Base` |
| `Src/Data/Data/Data.csproj` | Repositories for all main entities (60+ repository classes) |
| `Src/Data/Data.EntityFramework/Data.EntityFramework.csproj` | `Floor2PlanDbContext` — primary application DbContext (see Section 4) |
| `Src/Data/Data.DbMigrations/Data.DbMigrations.csproj` | EF migrations for `Floor2PlanDbContext` |
| `Src/Data/Data.ChangeHandlers/Data.ChangeHandlers.csproj` | 55+ domain change handlers triggered post-SaveChanges (cache refresh, recalculation) |
| `Src/Data/Data.ChangeHandlers.Contracts/Data.ChangeHandlers.Contracts.csproj` | Contracts for change handlers |
| `Src/Data/Data.OData.EntityFramework/Data.OData.EntityFramework.csproj` | `ODataDbContext` — dynamically builds entity set from `IODataDto<T>` implementors |
| `Src/Data/Data.OData.DbMigrations/Data.OData.DbMigrations.csproj` | EF migrations for OData DbContext |
| `Src/Data/Data.Reporting/Data.Reporting.csproj` | Reporting data services |
| `Src/Data/Data.Reporting.EntityFramework/Data.Reporting.EntityFramework.csproj` | `ReportingDbContext` — entities: `ReportingResource`, `ReportingFolder`, `ReportingResourcePageSetting` |
| `Src/Data/Data.Reporting.DbMigrations/Data.Reporting.DbMigrations.csproj` | EF migrations for reporting |

#### Distributions (separate denormalized distributions database)

| Project | Description |
|---|---|
| `Src/Distributions/Distributions.EntityFramework/Distributions.EntityFramework.csproj` | `DistributionsDbContext` — entities: `DailyPersonDistribution`, `DailyAssignmentDistribution`, `DateDistribution`, `BaselineAssignmentDistribution`, `DailyActivityGroupDistribution` |
| `Src/Distributions/Distributions.DbMigrations/Distributions.DbMigrations.csproj` | EF migrations for distributions |

#### Domain Layer

| Project | Description |
|---|---|
| `Src/Domain/Domain.Model/Domain.Model.csproj` | View models, DTOs, domain result models |
| `Src/Domain/Domain.Model.Extensions/Domain.Model.Extensions.csproj` | Extension methods on domain models |
| `Src/Domain/Domain.Model.Mapping/Domain.Model.Mapping.csproj` | AutoMapper profiles (GenericPropertyProfile — modified on current branch) |
| `Src/Domain/Domain.Service/Domain.Service.csproj` | Domain services: `ActivityService`, `ProjectService`, `CopyService`, `AllocationService`, `TaskService`, sync services, timesheet services, filter/view configuration helpers |

#### Infrastructure Layer

| Project | Description |
|---|---|
| `Src/Infrastructure/Infrastructure.Audit/Infrastructure.Audit.csproj` | ABP audit logging module wiring |
| `Src/Infrastructure/Infrastructure.Audit.EntityFramework/Infrastructure.Audit.EntityFramework.csproj` | `AuditDbContext` — entities: `AuditLog`, `AuditLogExcelFile` (via ABP `IAuditLoggingDbContext`) |
| `Src/Infrastructure/Infrastructure.Audit.DbMigrations/Infrastructure.Audit.DbMigrations.csproj` | EF migrations for audit |
| `Src/Infrastructure/Infrastructure.Authorisation/Infrastructure.Authorisation.csproj` | Permission grant management, local login, JWT provider |
| `Src/Infrastructure/Infrastructure.Authorisation.Contracts/Infrastructure.Authorisation.Contracts.csproj` | Auth contracts, `F2PAuthenticationPolicy`, `[PermissionAuthorise]` attribute |
| `Src/Infrastructure/Infrastructure.Authorisation.EntityFramework/Infrastructure.Authorisation.EntityFramework.csproj` | `AuthorisationDbContext` — entity: `PermissionGrant` |
| `Src/Infrastructure/Infrastructure.Authorisation.DbMigrations/Infrastructure.Authorisation.DbMigrations.csproj` | EF migrations for authorisation |
| `Src/Infrastructure/Infrastructure.Configuration.Runtime/Infrastructure.Configuration.Runtime.csproj` | `AppSettings` runtime config; SQL Server + Azure Key Vault configuration providers |
| `Src/Infrastructure/Infrastructure.Configuration.Runtime.Contracts/Infrastructure.Configuration.Runtime.Contracts.csproj` | `IAppSettings` contract |
| `Src/Infrastructure/Infrastructure.Configuration.Runtime.EntityFramework/Infrastructure.Configuration.Runtime.EntityFramework.csproj` | `ConfigurationDbContext` — entity: `AppSetting` |
| `Src/Infrastructure/Infrastructure.Configuration.Runtime.DbMigrations/Infrastructure.Configuration.Runtime.DbMigrations.csproj` | EF migrations for runtime configuration |
| `Src/Infrastructure/Infrastructure.Configuration.Startup/Infrastructure.Configuration.Startup.csproj` | Application startup / host builder helpers |
| `Src/Infrastructure/Infrastructure.Configuration.Startup.Contracts/Infrastructure.Configuration.Startup.Contracts.csproj` | Startup contracts |
| `Src/Infrastructure/Infrastructure.Elsa/Infrastructure.Elsa.csproj` | Elsa 3 workflow engine integration (backed by Hangfire). Configures Elsa EF persistence, JS scripting, scheduling. |
| `Src/Infrastructure/Infrastructure.Elsa.EntityFramework/Infrastructure.Elsa.EntityFramework.csproj` | Elsa EF context wrappers (Management + Runtime Elsa DbContexts) |
| `Src/Infrastructure/Infrastructure.Elsa.DbMigrations/Infrastructure.Elsa.DbMigrations.csproj` | EF migrations for Elsa (`RuntimeElsaMigrationDbContext`, `ManagementElsaMigrationDbContext`) |
| `Src/Infrastructure/Infrastructure.Files/Infrastructure.Files.csproj` | File storage service |
| `Src/Infrastructure/Infrastructure.Files.Contracts/Infrastructure.Files.Contracts.csproj` | File contracts |
| `Src/Infrastructure/Infrastructure.Files.EntityFramework/Infrastructure.Files.EntityFramework.csproj` | `FileDbContext` — entities: `ComponentFile`, `ProjectFile`, `AssetFile`, `MaterialFile`, `OrganisationFile`, `Floor2PlanBaseFileInfo`, `Floor2PlanFileInfo`, `ComponentUrl`, `ProjectUrl`, `Floor2PlanUrlInfo`, `Floor2PlanFileCategory`, `Floor2PlanFileThumbnail`, `Floor2PlanFileSource` |
| `Src/Infrastructure/Infrastructure.Files.DbMigrations/Infrastructure.Files.DbMigrations.csproj` | EF migrations for files |
| `Src/Infrastructure/Infrastructure.FireAndForget/Infrastructure.FireAndForget.csproj` | Fire-and-forget job service (wraps ABP `IBackgroundJobManager`, routes to `default` or `sync` Hangfire queues) |
| `Src/Infrastructure/Infrastructure.FireAndForget.Contracts/Infrastructure.FireAndForget.Contracts.csproj` | `IFireAndForget` contract |
| `Src/Infrastructure/Infrastructure.Hangfire/Infrastructure.Hangfire.csproj` | Hangfire wiring: two named servers (`DefaultServer` queue=`default`, `SyncServer` queue=`sync`), SQL Server or in-memory storage, dashboard at `/hangfire`, `JobStateEventHostedService` (`IHostedService`) |
| `Src/Infrastructure/Infrastructure.Hangfire.Contracts/Infrastructure.Hangfire.Contracts.csproj` | Hangfire contracts |
| `Src/Infrastructure/Infrastructure.Hangfire.DbMigrations/Infrastructure.Hangfire.DbMigrations.csproj` | `HangfireDbContext` (bare `DbContext`; schema managed by Hangfire's own schema installer) |
| `Src/Infrastructure/Infrastructure.Health/Infrastructure.Health.csproj` | Health check orchestration; `DelayedHealthCheckPublisherHostedService` (`IHostedService`) |
| `Src/Infrastructure/Infrastructure.Health.Contracts/Infrastructure.Health.Contracts.csproj` | Health contracts |
| `Src/Infrastructure/Infrastructure.Health.EntityFramework/Infrastructure.Health.EntityFramework.csproj` | `HealthDbContext` — entities: `HealthCheckLog`, `HealthCheckReport` |
| `Src/Infrastructure/Infrastructure.Health.DbMigrations/Infrastructure.Health.DbMigrations.csproj` | EF migrations for health |
| `Src/Infrastructure/Infrastructure.Health.Utility/Infrastructure.Health.Utility.csproj` | Health utility helpers |
| `Src/Infrastructure/Infrastructure.Logging/Infrastructure.Logging.csproj` | Serilog configuration; `HostedServiceLogScopeDecorator` (`IHostedService` decorator) |
| `Src/Infrastructure/Infrastructure.Mail/Infrastructure.Mail.csproj` | Email sending (SendGrid + SMTP) |
| `Src/Infrastructure/Infrastructure.Process/Infrastructure.Process.csproj` | Process log service |
| `Src/Infrastructure/Infrastructure.Process.Contracts/Infrastructure.Process.Contracts.csproj` | Process contracts |
| `Src/Infrastructure/Infrastructure.Process.EntityFramework/Infrastructure.Process.EntityFramework.csproj` | `ProcessDbContext` — entities: `ProcessLog`, `ProcessLogMessage` |
| `Src/Infrastructure/Infrastructure.Process.DbMigrations/Infrastructure.Process.DbMigrations.csproj` | EF migrations for process logs |
| `Src/Infrastructure/Infrastructure.Processors/Infrastructure.Processors.csproj` | Processor execution framework: `DataProcessorBase<TDbContext,…>`, `FilteredDataProcessorBase<…>` |
| `Src/Infrastructure/Infrastructure.State/Infrastructure.State.csproj` | In-memory state storage per request |
| `Src/Infrastructure/Infrastructure.Webservice/Infrastructure.Webservice.csproj` | WCF / SOAP web service client helpers (`System.ServiceModel.Http`) |
| `Src/Infrastructure/Infrastructure.Workflow/Infrastructure.Workflow.csproj` | Elsa activity implementations (see Section 3) |
| `Src/Infrastructure/Infrastructure.Workflow.Contracts/Infrastructure.Workflow.Contracts.csproj` | Workflow contracts |

#### Processors Layer

| Project | Description |
|---|---|
| `Src/Processors/Processors.Contracts/Processors.Contracts.csproj` | Processor interfaces (IRefreshActivitySummaryProcessor, etc.) |
| `Src/Processors/Processors.Domain/Processors.Domain.csproj` | 82 processor implementation classes; all extend `DataProcessorBase<Floor2PlanDbContext, …>` |

#### DatabaseDeployer

| Project | Description |
|---|---|
| `Src/DatabaseDeployer/DatabaseDeployer/DatabaseDeployer.csproj` | Executable entry point (Exe, win-x64/linux-x64) |
| `Src/DatabaseDeployer/DatabaseDeployer.Contracts/DatabaseDeployer.Contracts.csproj` | `IBaseMigrationDbContext` contract |
| `Src/DatabaseDeployer/DatabaseDeployer.Data/DatabaseDeployer.Data.csproj` | `DatabaseBuilderByDbContext`, `BaseMigrationDbContext<T,T>`, `BaseSqlDbContextFactory<T>` |
| `Src/DatabaseDeployer/DatabaseDeployer.Domain/DatabaseDeployer.Domain.csproj` | `ContextMigrator`, `DatabaseMigratorConfigurationHelper` |

#### UI Layer

| Project | Description |
|---|---|
| `Src/UI/UI.Floor2Plan/UI.Floor2Plan.csproj` | Web SDK — Razor MVC app. Main runnable host. Contains 100+ MVC controllers across 12 MVC Areas + root controllers. Wires Vite/Vue front-end client. |
| `Src/UI/Floor2Plan.Api/Floor2Plan.Api.csproj` | Web SDK (Library) — REST API and OData generic endpoint. 4 explicit controllers + generic OData controller factory. |
| `Src/UI/Floor2Plan.Startup/Floor2Plan.Startup.csproj` | Application startup bootstrap library (standard SDK) |
| `Src/UI/UI.Web.Common/UI.Web.Common.csproj` | Shared MVC base classes, middleware, authentication, Kendo helpers, security headers |
| `Src/UI/UI.Floor2PlanClient/` | Vue 3 + TypeScript front-end (`.esproj`) — compiled to `wwwroot/dist` by Vite. Not a C# project. |

---

## 2. Folder Structure and Logical Layers

### Top-Level Folder Map

| Folder | Status | Description |
|---|---|---|
| `Src/` | Source root | All C# source projects |
| `Test/` | Test root | All test projects (unit, integration, E2E, data) |
| `Build/` | Build infrastructure | `Runnable.Build.props`, `Version.Build.props`, `Variables.Build.props` |
| `Pipelines/` | CI/CD | Azure DevOps YAML pipeline definitions (`f2p-build-*.yml`) |
| `ThirdParty/` | Local NuGet feed | `ThirdParty/Nuget/` — locally hosted NuGet packages |
| `Tools/` | Developer tooling | [NEEDS REVIEW] — present but not surveyed in detail |
| `docs/` | Documentation | Modularization analysis docs (this document) |
| `Application/`, `Common/`, `Connectors/` etc. (top-level) | Artifact output | Build artifact output directories only (`bin/`, `obj/`). **Not source.** |
| `Artifacts/` | Build artifacts | Central output dir (set via `UseArtifactsOutput=true` in `Directory.Build.props`) |
| `DatabaseScripts/` | SQL scripts | [NEEDS REVIEW] — SQL scripts outside migration system |

### Source Layer Map (`Src/`)

| Folder in `Src/` | Logical Layer | Description |
|---|---|---|
| `Src/UI/` | **Presentation / API** | Web hosts, REST controllers, OData endpoint, MVC Areas, Vue SPA entry point, shared MVC helpers |
| `Src/Application/` | **Application** | Orchestration services, import/sync jobs, system actions (no domain logic, no EF references) |
| `Src/Domain/` | **Domain** | Domain models (view models, DTOs), AutoMapper profiles, domain services (business logic) |
| `Src/Contracts/` | **Abstractions** | Pure interface / DTO packages shared across all layers |
| `Src/Data/` | **Data / Persistence** | EF entities (`Data.Model`), `Floor2PlanDbContext`, repositories, change handlers, OData context, reporting context, migrations |
| `Src/Distributions/` | **Data / Persistence** | Separate denormalized distributions context and its migrations |
| `Src/Connectors/` | **External Integration** | Document store connector (EF + migrations) |
| `Src/Common/` | **Cross-cutting** | Utilities, security, localization, mail templates, domain primitives |
| `Src/Infrastructure/` | **Infrastructure** | Auth, audit, config, Hangfire, Elsa workflows, health, logging, mail, files, process log, processor framework, state storage, WCF helpers |
| `Src/Processors/` | **Background Processing** | 82 domain processor implementations (cache refresh, distributions, summary recalculation, balance updates) |
| `Src/DatabaseDeployer/` | **Tooling** | Standalone DB migration executable and supporting libraries |

### MVC Areas in `UI.Floor2Plan`

| Area | Controllers |
|---|---|
| `Pbs` (Project Breakdown Structure) | Project, Component, ComponentType, Organisation, OrganisationCapacity, Discipline, DisciplineCapacity, Person, PersonTimesheetApprover, Role, Asset, Schedule, Baseline, Planning, ProjectScenario, StructureType, Wor, Settings, Shipyard, Index, User, ActivityProperty, ProjectType |
| `Plan` | Activity, ActivityRelation, Assignment, Holiday |
| `Do` | PlanBoard, FloorBoard, WeeklyTimesheet, EmployeeTimesheet, CorrectionTimesheet, Reporter |
| `Check` | Reports, ReportsApi, ReportDesigner, KPI, KpiSpiCpi, ErpWeeklyTimesheet, ErpCorrectionTimesheet |
| `HR` | Balance, BalancePolicy, BalancePolicyRule, BalanceAccumulationRule, PersonBalance, OffTime, ScheduleManagement, ClockingTerminal, ClockingTerminalProfile |
| `Sync` | Index, ImportExport, PlanningHours, TransferHours, ProductModelLink, Pdm, ConnectorConfiguration, ConvertPlanningFile |
| `System` | Administration, AppSetting, LicenseTable, SystemAction, Shipyard (top-level) |
| `Ticket` | Ticket, Issue, StopWorkOrder, TransportRequest |
| `Material` | Material, ProjectMaterial |
| `FloorSpace` | FloorSpace, Location |
| `Devices` | ShopFloorTerminal, ClockingTerminal |
| `Prediction` | Prediction |

### Root (non-area) Controllers in `UI.Floor2Plan`

`Home`, `Account`, `Error`, `Version`, `ClientInit`, `State`, `Select`, `OptionList`, `GenericProperty`, `Event`, `NewsFeed`, `ExternalLink`, `File`, `PageSettings`, `GeneralSettings`, `PersonManagement`, `PersonInfo`, `PermissionManagement`, `Structure`, `Task Readiness`, `Team`, `Allocation`, `Scripts`, `Elsa`

---

## 3. Integration Points

### 3.1 HTTP APIs

#### REST API — `Floor2Plan.Api` (`Src/UI/Floor2Plan.Api/`)

All public API endpoints are versioned `v1.0` and require `[Authorize(AuthenticationSchemes = F2PAuthenticationPolicy.Api)]` (JWT Bearer).

| Controller | Route | Methods |
|---|---|---|
| `AuthController` | `api/Auth` | `POST /LoginAsync` — returns JWT token |
| `TimesheetController` | `api/Timesheet` | `POST /Status` — external timesheet approval callback |
| `ImportController` | `api/Import` | `PUT/POST /ProductBreakdown`, `PUT/POST /Allocation` — file-based imports |
| `GenericEntityController<TDto,T>` | OData route (auto-discovered) | `GET` — generic OData query endpoint for all `IODataDto<T>` implementations |

#### MVC Controllers (`UI.Floor2Plan`)

100+ MVC controllers (see Section 2). These serve HTML/JSON for the Razor MVC front-end. Not versioned REST endpoints — they use cookie/bearer session authentication.

### 3.2 Message Handlers

No `IHandleMessages`, `INotificationHandler` (MediatR), or `IConsumer` (MassTransit) implementations found. Message brokering is not used; internal communication uses direct service injection and the Hangfire fire-and-forget pattern.

`Infrastructure.Elsa.Utilities.WorkflowTransientVariablePopulator` implements `INotificationHandler` from Elsa's internal notification system (not a general-purpose message bus). This is internal to the Elsa workflow engine.

### 3.3 Hangfire Jobs

Hangfire is configured in `Src/Infrastructure/Infrastructure.Hangfire/Floor2PlanInfrastructureHangfireModule.cs` with two named servers:

| Server name | Queue | Worker count | Purpose |
|---|---|---|---|
| `DefaultServer` | `default` | 1 | General background jobs |
| `SyncServer` | `sync` | 1 | Import/sync jobs |

**Import jobs** (enqueued via `IFireAndForget` → `IBackgroundJobManager` → Hangfire):

| Job class | Location | Description |
|---|---|---|
| `ImportTimesheetJob` | `Application.Sync/ImportJobs/Timesheets` | Timesheet import |
| `ImportPlanningJob` | `Application.Sync/ImportJobs/Planning` | Planning import |
| `ImportHoursAndProgressJob` | `Application.Sync/ImportJobs/HoursAndProgress` | Hours and progress import |
| `ImportEmployeeJob` | `Application.Sync/ImportJobs/Employees` | Employee/person import |
| `ImportOrganisationJob` | `Application.Sync/ImportJobs/OrganisationImport` | Organisation import |
| `ImportMaterialJob` | `Application.Sync/ImportJobs/Materials` | Material import |
| `ImportDisciplineJob` | `Application.Sync/ImportJobs/DisciplineImport` | Discipline import |
| `ImportProjectMaterialJob` | `Application.Sync/ImportJobs/ProjectMaterials` | Project material import |
| `ImportTaskReadinessJob` | `Application.Sync/ImportJobs/TaskReadiness` | Task readiness import |
| `ImportPropertyTypeJob` | `Application.Sync/ImportJobs/PropertyTypes` | Property type import |
| `ImportAsposeJob` | `Application.Sync/ImportJobs/Aspose` | Aspose-based planning import (XER/P6) |
| `ImportAccessControlJob` | `Application.Sync/ImportJobs/AccessControl` | Access control import |
| `ImportUnitTypeDomainModelJob` | `Application.Sync/ImportJobs/UnitTypes` | Unit type import |
| `ImportPersonDomainModelJob` | `Application.Sync/ImportJobs/Persons` | Person domain model import |
| `ImportOrganisationDomainModelJob` | `Application.Sync/ImportJobs/Organisations` | Organisation domain model import |
| `ImportMaterialDomainModelJob` | `Application.Sync/ImportJobs/Materials` | Material domain model import |
| `ImportHourTypeDomainModelJob` | `Application.Sync/ImportJobs/HourTypes` | Hour type import |
| `ImportDisciplineDomainModelJob` | `Application.Sync/ImportJobs/Disciplines` | Discipline domain model import |
| `ImportComponentTypeDomainModelJob` | `Application.Sync/ImportJobs/ComponentTypes` | Component type import |
| `ImportProjectStatusDomainModelJob` | `Application.Sync/ImportJobs/ProjectStatusses` | Project status import |
| `ImportActivityStatusDomainModelJob` | `Application.Sync/ImportJobs/ActivityStatus` | Activity status import |
| `ImportActivityPhaseDomainModelJob` | `Application.Sync/ImportJobs/ActivityPhases` | Activity phase import |

**FireAndForget** (single-shot background execution, not recurring):
- `FireAndForgetJob` — general fire-and-forget via `default` queue
- `SyncFireAndForgetJob` — sync-specific via `sync` queue
- Both in `Src/Infrastructure/Infrastructure.FireAndForget/Jobs/`

**Recurring / scheduled jobs**: No direct `RecurringJob.AddOrUpdate` calls found. ABP's `Volo.Abp.BackgroundJobs.HangFire` module is referenced, which provides scheduling through ABP's background jobs abstraction. [NEEDS REVIEW] — recurring job registration may occur through ABP configuration or Elsa scheduling.

### 3.4 IHostedService / BackgroundService

| Class | Project | Description |
|---|---|---|
| `JobStateEventHostedService` | `Infrastructure.Hangfire` | Listens for Hangfire job state change events and broadcasts them |
| `DelayedHealthCheckPublisherHostedService` | `Infrastructure.Health` | Publishes deferred health check results |
| `HostedServiceLogScopeDecorator` | `Infrastructure.Logging` | Wraps other `IHostedService` implementations to add log scope enrichment |

### 3.5 File Import / Export

Import formats handled:

| Format | Provider class | Location |
|---|---|---|
| Excel (NPOI) | `ExcelImportReaderService`, `ImportPersonsProvider`, `ImportOrganisationProvider`, `ImportMaterialProvider`, `ImportDisciplineProvider`, plus multiple `*DomainModelExcelProvider` | `Application.Sync` |
| CSV | `CsvImportReaderService` | `Application.Sync/Services/Readers` |
| XER (Primavera P6) | `ImportXerProvider` | `Application.Sync/ImportProvider` |
| Aspose.Tasks (MS Project / P6) | `ImportAsposeProvider`, `ImportAsposeJob` | `Application.Sync/ImportProvider`, `Application.Sync/ImportJobs/Aspose` |
| Sciforma XML | `ImportSciformaProvider` | `Application.Sync/ImportProvider` |
| Timesheet (Excel) | `ImportTimesheetProvider` | `Application.Sync/ImportProvider` |
| Access control | `ImportAccessControlProvider` | `Application.Sync/ImportProvider` |
| Task readiness | `ImportTaskReadinessProvider` | `Application.Sync/ImportProvider` |
| Product breakdown | `ImportProductBreakdownProvider` | `Application.Sync/ImportProvider` |
| Allocation | `AllocationImportProvider` | `Floor2Plan.Api/Application` |

Export formats:
| Format | Class | Location |
|---|---|---|
| Excel (NPOI) | `NPoiExcelReportFile` | `Application.Service/Reports` |
| PDF (Spire.PDF) | `PdfConverter` | `Domain.Service/Helpers` |
| Aspose export | `AsposeExportHelper`, `AsposeProjectBuilder` | `Application.Service/Helpers/Aspose` |
| Telerik Reporting (TRDX/PDF) | Wired in `UI.Floor2Plan`, Report Designer at `Check/ReportDesigner` | `UI.Floor2Plan/Areas/Check/Controllers` |

### 3.6 Elsa Workflow Activities

Workflow activities registered in `Infrastructure.Workflow`:

| Category | Activities |
|---|---|
| Data | `ActivityStatusGateActivity`, `GetActivityByIdActivity`, `SetActivityStatusActivity` |
| Processor | `RunProcessorActivity` |
| Sync | `RunConnectorActivity`, `SyncLogSendMailActivity` |
| Ticket | `TicketActivityCloseActivity`, `TicketActivityCreateActivity`, `TicketMailActivity`, `TicketSendMailActivity`, `TicketStatusUpdateActivity`, `TransportRequestActivityUpdateActivity` |
| StopWorkOrder | `StopWorkOrderSendMailActivity` |

Elsa uses Hangfire as the scheduling backend (`Elsa.Hangfire` 3.5.3). Workflow persistence uses SQL Server via EF Core (`Elsa.EntityFrameworkCore.SqlServer`).

---

## 4. DbContext / DataContext Inventory

All DbContexts use SQL Server via ABP `AbpDbContext<T>` / `BaseDbContext<T>` base, with connection string `Default` (constant `DbConstants.DefaultConnectionStringName`), except `HangfireDbContext` (plain `DbContext`, schema managed externally) and Elsa contexts.

### Primary Application DbContext

**`Floor2PlanDbContext`** — `Src/Data/Data.EntityFramework/Floor2PlanDbContext.cs`
Base: `BaseChangeHandlerDbContext<Floor2PlanDbContext>` (which extends `AbpDbContext<T>`)

Mapped entities (DbSet properties):

| Group | Entities |
|---|---|
| Activities | `Activity`, `ActivitySummary`, `ActivityRelation`, `ActivityStatus`, `ActivityType`, `ActivityPhase`, `ActivityProgressTask`, `ActivityTicketRelation`, `ActivityProperty` |
| Assignments | `Assignment`, `AssignmentSummary`, `AssignmentOption`, `AssignmentProfile`, `AssignmentProgressHistory`, `AssignmentDurationPrediction`, `AssignmentShortTermPrediction`, `AssignmentProperty`, `AssignmentHourIncrement` |
| Allocations | `Allocation` |
| Baselines | `Baseline`, `BaselineActivity`, `BaselineAssignment`, `BaselineComponent`, `BaselineProject` |
| Components | `Component`, `ComponentType`, `ComponentProperty`, `ComponentTypeProperty`, `ComponentState`, `ComponentStateGroup`, `ComponentStateProperty`, `ComponentStateGroupProperty` |
| Disciplines | `Discipline`, `DisciplineProperty`, `DisciplinePerson` |
| Time / Schedules | `DayWeekYear`, `Schedule`, `ScheduleEntry`, `ScheduleEntryPeriod`, `PersonSchedule`, `PersonScheduleEntry`, `OrganisationSchedule`, `OrganisationScheduleEntry`, `ScheduleRoundingRuleSet`, `OvertimeEntry`, `CachePersonalSchedule`, `CachePersonalScheduleOvertime`, `CachePersonalSchedulePeriod`, `PersonalDailyClockTimes` |
| Timesheets | `TimeSheet`, `TimesheetEntry`, `TimeSheetRemark`, `TimeSheetStatus`, `TimeSheetWeeklyHourline`, `TimesheetEventMessage`, `CorrectionTimesheet`, `CorrectionTimesheetWeeklyCorrectionLine`, `ErpHourline`, `Hourline`, `HourlyRate`, `HourType`, `SplitActivityEntry` [NEEDS REVIEW — csproj ref found but DbSet not confirmed in read window] |
| Balances | `Balance`, `BalanceMutationHistory`, `BalanceMutationType`, `BalancePolicy`, `BalancePolicyRule`, `BalancePolicyInBalancePolicyRule`, `BalanceAccumulationRule`, `PersonBalancePolicy` |
| Organisations | `Organisation`, `OrganisationHoliday`, `OrganisationInRole`, `OrganisationProperty`, `OrganisationRole`, `OrganisationCapacityDashboardLine`, `OrganisationOffTimeInTimesheetHours` |
| Persons | `Person`, `PersonHoliday`, `PersonHolidayStatus`, `PersonHolidayInTimesheetHours`, `PersonInTeam`, `PersonProperty`, `PersonCapacityDashboardLineFilter` |
| Projects | `Project`, `ProjectDisciplineTariff`, `ProjectStatus`, `ProjectType`, `ProjectScenario`, `ProjectScenarioActivity`, `ProjectMaterial`, `ProjectMaterialUsageHistory`, `ProjectProperty`, `ProjectTypeProperty` |
| Roles & Users | `Role`, `User`, `UserInRole`, `OrganisationInRole` |
| Structures | `Structure`, `StructureRelation`, `StructureType` |
| Tasks | `F2PTask` |
| Teams | `Team` |
| Materials | `Material`, `MaterialProperty` |
| Assets | `Asset`, `AssetType` |
| Tickets | `TicketBase`, `TransportRequest`, `StopWorkOrder`, `TicketRemark`, `TicketProperty` |
| Properties | `PropertyTypeOption`, `PropertyType`, `GenericPropertyBase`, `PropertySet`, `PersonProperty`, `ProjectProperty`, `ActivityProperty`, `AssignmentProperty`, `ComponentProperty`, `MaterialProperty` |
| Devices / Clocking | `ClockingTerminal`, `ClockingTerminalPunch`, `ClockingTerminalProfile`, `ClockingTerminalSetting`, `ClockType`, `ShopFloorTerminal`, `ShopFloorTerminalSetting`, `AccessControlTime`, `AccessControlTimeClockingTerminalPunch`, `RfidSetting` |
| Capacity | `CapacityDashboardLine`, `OrganisationCapacityDashboardLine`, `PersonCapacityDashboardLineFilter`, `DisciplineCapacityDashboardLine` [NEEDS REVIEW — DisciplineCapacityDashboardLine confirmed in EF config only] |
| Task Readiness | `TaskReadinessStatusBase`, `TaskReadinessStatusAssignment`, `TaskReadinessStatusActivity`, `TaskReadinessStatusComponent`, `TaskReadinessStatusItemBase`, `TaskReadinessStatusItemEngineering`, `TaskReadinessStatusItemDesignChange`, `TaskReadinessStatusItemPredecessor`, `TaskReadinessStatusItemMaterial` |
| Misc | `ImportConfig`, `ImportConfigRule`, `LoginRequest`, `ManualHoursAndProgress`, `NewsFeedMessage`, `MilestoneEventMessage`, `SyncLog`, `Wor` (WeakObjectRelation), `WorType`, `PageSetting`, `DayWeekYear` |

### Secondary / Bounded DbContexts

| DbContext class | Project | Connection | Entities / Purpose |
|---|---|---|---|
| `ReportingDbContext` | `Data.Reporting.EntityFramework` | Default | `ReportingResource`, `ReportingFolder`, `ReportingResourcePageSetting` |
| `ODataDbContext` | `Data.OData.EntityFramework` | Default | Dynamically registered — all `IODataDto<T>` implementations. No static DbSet properties. |
| `DistributionsDbContext` | `Distributions.EntityFramework` | Default | `DailyPersonDistribution`, `DailyAssignmentDistribution`, `DateDistribution`, `BaselineAssignmentDistribution`, `DailyActivityGroupDistribution` |
| `DocumentStoreDbContext` | `Connectors/DocumentStore.EntityFramework` | Default | `DsFileContent` |
| `FileDbContext` | `Infrastructure.Files.EntityFramework` | Default | `ComponentFile`, `ProjectFile`, `AssetFile`, `MaterialFile`, `OrganisationFile`, `Floor2PlanBaseFileInfo`, `Floor2PlanFileInfo`, `ComponentUrl`, `ProjectUrl`, `Floor2PlanUrlInfo`, `Floor2PlanFileCategory`, `Floor2PlanFileThumbnail`, `Floor2PlanFileSource` |
| `AuthorisationDbContext` | `Infrastructure.Authorisation.EntityFramework` | Default | `PermissionGrant` |
| `AuditDbContext` | `Infrastructure.Audit.EntityFramework` | Default | `AuditLog`, `AuditLogExcelFile` (ABP `IAuditLoggingDbContext`) |
| `HealthDbContext` | `Infrastructure.Health.EntityFramework` | Default | `HealthCheckLog`, `HealthCheckReport` |
| `ConfigurationDbContext` | `Infrastructure.Configuration.Runtime.EntityFramework` | Default | `AppSetting` |
| `ProcessDbContext` | `Infrastructure.Process.EntityFramework` | Default | `ProcessLog`, `ProcessLogMessage` |
| `HangfireDbContext` | `Infrastructure.Hangfire.DbMigrations` | Default | Empty — Hangfire manages its own schema separately |

### Elsa DbContexts (external library wrappers)

| DbContext class | Project | Purpose |
|---|---|---|
| `RuntimeElsaMigrationDbContext` | `Infrastructure.Elsa.DbMigrations` | Elsa runtime persistence (workflow instances, triggers, bookmarks) |
| `ManagementElsaMigrationDbContext` | `Infrastructure.Elsa.DbMigrations` | Elsa management persistence (workflow definitions) |

### Migration DbContexts (tooling only)

Each production DbContext has a corresponding `*MigrationDbContext` that extends `BaseMigrationDbContext<TMigration, TTarget>` and is used only by the DatabaseDeployer and EF design-time tooling. These are not used at runtime.

---

## 5. Authentication and Authorization

### Authentication Schemes

Configured in `Src/UI/UI.Web.Common/Middleware/Authentication/AuthenticationMiddleware.cs`.

| Scheme | Type | Description |
|---|---|---|
| `DefaultAuthentication` (policy) | Policy scheme | Selector — routes incoming requests to the appropriate actual scheme based on cookie presence or `Authorization: Bearer` header |
| `ClientAuthScheme` | Cookie | Azure AD login for client users (via `Microsoft.Identity.Web` / `AddCookieAuthentication`) |
| `FloorganiseAuthScheme` | Cookie | Azure AD login for Floorganise internal users |
| Cookie (default) | Cookie | Local username/password login |
| `JwtBearerDefaults.AuthenticationScheme` | JWT Bearer | API bearer token authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`) |

### Authorization Policies

Registered in `AuthenticationMiddleware.Configure()`:

| Policy name | Requirement |
|---|---|
| `F2PAuthenticationPolicy.ShopFloorTerminal` | Requires claim `ShopFloorTerminal` |
| `F2PAuthenticationPolicy.ClockingTerminal` | Requires claim `ClockingTerminal` |
| `F2PAuthenticationPolicy.CiBuildAgent` | Custom `ApiKeyRequirement` (API key from `IAppSettings.CiBuildApiKey`) |
| `F2PAuthenticationPolicy.Api` | Requires claim `Api` — used on all API controllers |

### Controller-Level Authorization

- All API controllers in `Floor2Plan.Api` declare `[Authorize(AuthenticationSchemes = F2PAuthenticationPolicy.Api)]`.
- `GenericEntityController` additionally uses `[PermissionAuthorise(ModulePermissions.General.OData)]` (custom attribute from `Infrastructure.Authorisation.Contracts`).
- `HolidayController` in `Areas/Plan` has at least one `[Authorize]` attribute (additional MVC route-level auth).
- MVC controllers in `UI.Floor2Plan` rely on the default authentication + cookie scheme; explicit `[Authorize]` attributes are present on individual action methods [NEEDS REVIEW — full scan of MVC controllers not performed].

### JWT Configuration

- Packages: `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.6, `Microsoft.IdentityModel.Tokens` v8.17.0, `System.IdentityModel.Tokens.Jwt` v8.17.0.
- JWT bearer token issuance: `IApiAuthorizationProvider.GetBearerTokenAsync()` in `Infrastructure.Authorisation` (called from `AuthController.LoginAsync`).
- `BearerTokenAuthenticationMiddleware` in `UI.Web.Common/Middleware/Authentication/` handles bearer token extraction.

### Identity / User Management

- `System.DirectoryServices.AccountManagement` is referenced (Active Directory integration).
- `Microsoft.Identity.Web` v4.7.0 + `Microsoft.Identity.Client` v4.83.3 — Azure AD / Entra ID integration for both client and Floorganise auth schemes.
- `Konscious.Security.Cryptography.Argon2` — local password hashing in `Common.Security`.
- ABP permission system (`Volo.Abp.Authorization`) used for fine-grained permissions stored in `PermissionGrant` table.

### Health Check Authorization

- `/Health/plain` and `/Health/json` endpoints require `F2PAuthenticationPolicy.CiBuildAgent` policy when not in debug mode.
- `/Mitigate/json` similarly protected.
- Configured in `Floor2PlanUIWebModule.ConfigureHealthChecks()`.

---

## 6. Test Projects

### Test Framework

All test projects use:
- **xunit.v3** (via `xunit.v3.mtp-v2` v3.2.2) as the test framework
- **Microsoft.Testing.Platform** v2.2.1 as the test runner (all tests compile as `OutputType=Exe`)
- **AwesomeAssertions** v9.4.0 for assertions
- **Moq** v4.20.72 for mocking
- **MockQueryable.Moq** v10.0.5 for EF queryable mocking

Selenium end-to-end tests use their own `net8.0` target with `xunit.v3` v1.1.0 — **the only projects targeting net8.0 instead of net10.0**.

### Unit Tests

| Project | Source layer under test |
|---|---|
| `Test/UnitTest/Floor2Plan.UnitTest.Application.Service/` | `Application.Service` |
| `Test/UnitTest/Floor2Plan.UnitTest.Application.Sync/` | `Application.Sync` |
| `Test/UnitTest/Floor2Plan.UnitTest.Application.SystemActions/` | `Application.SystemActions` |
| `Test/UnitTest/Floor2Plan.UnitTest.Common/` | `Common.*` |
| `Test/UnitTest/Floor2Plan.UnitTest.Data/` | `Data.*` |
| `Test/UnitTest/Floor2Plan.UnitTest.Domain.Model.Mapping/` | `Domain.Model.Mapping` |
| `Test/UnitTest/Floor2Plan.UnitTest.Domain.Service/` | `Domain.Service` |
| `Test/UnitTest/Floor2Plan.UnitTest.Infrastructure/` | `Infrastructure.*` |
| `Test/UnitTest/Floor2Plan.UnitTest.Infrastructure.Health/` | `Infrastructure.Health` |
| `Test/UnitTest/Floor2Plan.UnitTest.Infrastructure.Workflow/` | `Infrastructure.Workflow` (Elsa) |
| `Test/UnitTest/Floor2Plan.UnitTest.Processors.Domain/` | `Processors.Domain` |
| `Test/UnitTest/Floor2Plan.UnitTest.TestUtility/` | Test utilities themselves |
| `Test/UnitTest/Floor2Plan.UnitTest.UI/` (`Floor2Plan.UnitTest.UI.Floor2Plan.csproj`) | `UI.Floor2Plan` |
| `Test/UnitTest/Floor2Plan.UnitTest.Ui.Api/` | `Floor2Plan.Api` |
| `Test/UnitTest/Floor2Plan.UnitTest.UI.Floor2Plan.Mvc/` | MVC-specific tests |
| `Test/UnitTest/Floor2Plan.UnitTest.UI.Web.Common/` | `UI.Web.Common` |
| `Test/UnitTest/UnitTest.DatabaseDeployer/` | `DatabaseDeployer.*` |
| `Test/UnitTest/Floor2Plan.UnitTest.Startup/` | `Floor2Plan.Startup` [NEEDS REVIEW — project directory only contains build artifacts, no .csproj found] |

### Integration Tests

| Project | Description |
|---|---|
| `Test/IntegrationTest/Floor2Plan.IntegrationTest.Application/` | Application-layer integration tests |
| `Test/IntegrationTest/Floor2Plan.IntegrationTest.Data/` | Data-layer integration tests |
| `Test/IntegrationTest/Floor2Plan.IntegrationTest.Domain/` | Domain-layer integration tests |
| `Test/IntegrationTest/Floor2Plan.IntegrationTest.Infrastructure/` | Infrastructure integration tests |
| `Test/IntegrationTest/Floor2Plan.IntegrationTest.Infrastructure.Workflow/` | Elsa workflow integration tests |
| `Test/IntegrationTest/Floor2Plan.IntegrationTest.UI/` | UI / controller integration tests |

Integration tests use `Microsoft.AspNetCore.TestHost` v10.0.3 for in-process HTTP testing [NEEDS REVIEW — confirm whether TestHost is used directly or through ABP test infra].

### Data Tests (EF / SQL)

| Project | Description |
|---|---|
| `Test/DataTest/Floor2Plan.DataTest.Data/` | Database-level data tests; embeds migration SQL scripts from `Data.DbMigrations/Sql/`. Imports `DataTest.Build.props` with `F2PTestType=Data`. |

### End-to-End / Selenium

| Project | Framework | Target | Description |
|---|---|---|---|
| `Test/EndToEnd/Floor2Plan.Selenium/` | Selenium 4.28, xunit.v3 | net8.0 | End-to-end browser tests |
| `Test/EndToEnd/Floor2Plan.Selenium.Base/` | Selenium 4.28 | net8.0 | Shared Selenium base classes |
| `Test/EndToEnd/Floor2Plan.Selenium.Utilities/` | — | net8.0 | Selenium utility helpers |

### Test Utilities

| Project | Description |
|---|---|
| `Test/Utility/Floor2Plan.TestUtility.Common/` | Common test helpers and assertion utilities |
| `Test/Utility/Floor2Plan.TestUtility.Data/` | EF / database test helpers |
| `Test/Utility/Floor2Plan.TestUtility.TestBase/` | ABP-based test base classes (`Volo.Abp.TestBase`) |
| `Test/Utility/Floor2Plan.TestUtility.WebTestBase/` | Web / HTTP test base classes |

---

## 7. Build Tooling, Target Frameworks, and Deprecated Patterns

### Global Configuration (`Directory.Build.props`)

| Setting | Value |
|---|---|
| **Default TargetFramework** | `net10.0` (all C# projects unless overridden) |
| **Language Version** | `latest` |
| **Platform** | `x64` (AnyCPU and x64 defined; `PlatformTarget=x64`) |
| **Runtime identifier** | `win-x64` (global default; DatabaseDeployer overrides to `win-x64;linux-x64`) |
| **Output** | Centralized artifacts: `Artifacts/` dir (`UseArtifactsOutput=true`) |
| **Warnings as errors** | `TreatWarningsAsErrors=true` (CS0618, CS0672 excluded) |
| **NuGet package management** | Central Package Management (CPM) via `ManagePackageVersionsCentrally=true` |
| **NuGet restore** | Lock files (`RestorePackagesWithLockFile=true`), static graph evaluation |
| **Transitive project refs** | Disabled globally (`DisableTransitiveProjectReferences=true`); overridden for runnable projects via `Runnable.Build.props` |
| **Razor source generator** | Off by default; enabled per-project where Razor is used |
| **Analyzers** | Microsoft.CodeAnalysis.NetAnalyzers, StyleCop.Analyzers, Meziantou.Analyzer, Microsoft.CodeAnalysis.BannedApiAnalyzers (with `BannedSymbols.txt`) |
| **Deterministic builds** | `Deterministic=true` |
| **Test runner** | `Microsoft.Testing.Platform` (via `global.json` `"runner": "Microsoft.Testing.Platform"`) |

### Version (`Build/Version.Build.props`)

| Setting | Value |
|---|---|
| **Product version** | `2026.3.0000` |
| **Product name** | `Core` |
| **Build number** | `0` (default; overridden in CI via `F2PBuildNumber` env variable) |
| **Company** | Floorganise Holding B.V. |

### Runnable Projects (`Build/Runnable.Build.props`)

Projects that reference `Runnable.Build.props` (via `<Import>`):
- `Src/UI/UI.Floor2Plan/UI.Floor2Plan.csproj`
- `Src/DatabaseDeployer/DatabaseDeployer/DatabaseDeployer.csproj`
- All test projects (via `Test.Build.props`)

Runnable projects set `IsPublishable=true`, `CopyLocalLockFileAssemblies=true`, `DisableTransitiveProjectReferences=false`, `GenerateDependencyFile=true`.

### Third-Party NuGet Packages Summary

| Package | Version | Usage |
|---|---|---|
| Volo.Abp.* | 10.3.0 | Full ABP framework stack (DI, EF, auditing, background jobs, MVC, localization) |
| Hangfire (core/AspNetCore/SqlServer/InMemory) | 1.8.23 | Background job processing |
| Elsa + Elsa.EntityFrameworkCore + Elsa.Hangfire | 3.5.3 | Workflow engine |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.6 | ORM |
| Microsoft.Identity.Web | 4.7.0 | Azure AD/Entra ID authentication |
| Serilog + sinks | 4.3.1 (core) | Structured logging; sinks: File, Console, Debug, Azure Blob, Application Insights, Async |
| AutoMapper | 16.1.1 | Object mapping |
| Telerik.Reporting | 18.2.24.806 | Report designer and rendering |
| Telerik.DataSource | 3.0.1 | Kendo UI data source server-side processing |
| NPOI | 2.7.6 | Excel import/export |
| Aspose.Tasks | 24.10.0 | MS Project / Primavera P6 file parsing |
| Spire.PDF | 12.4.5 | PDF generation |
| Swashbuckle.AspNetCore | 10.1.5 | Swagger/OpenAPI documentation |
| Microsoft.AspNetCore.OData | 9.4.1 | OData API endpoint |
| Vite.AspNetCore | 2.4.1 | Vite dev server integration |
| xunit.v3.mtp-v2 | 3.2.2 | xUnit v3 test framework |
| Microsoft.Testing.Platform | 2.2.1 | Test runner |
| AwesomeAssertions | 9.4.0 | Test assertions |
| Moq | 4.20.72 | Test mocking |
| MathNet.Numerics.Signed | 5.0.0 | Mathematical algorithms |
| DistributedLock.SqlServer | 1.0.7 | SQL Server-based distributed locking |
| Microsoft.SharePointOnline.CSOM | 16.1.23508.12000 | SharePoint integration [NEEDS REVIEW — confirm active usage] |
| System.ServiceModel.Http / Primitives | 10.0.652802 | WCF client (used in `Infrastructure.Webservice`) |
| Scrutor | 7.0.0 | Assembly scanning / decorator DI registration |
| SendGrid | 9.29.3 | Email delivery |
| Imageflow.AllPlatforms | 0.15.1 | Image processing |
| NetEscapades.AspNetCore.SecurityHeaders | 1.3.1 | HTTP security headers |

### CI/CD Pipelines

Located in `Build/` directory:

| File | Description |
|---|---|
| `Build/f2p-build-continuous-integration-core.yml` | Main CI pipeline for core solution |
| `Build/f2p-build-continuous-integration-core-data-tests.yml` | CI pipeline specifically for data tests |
| `Build/f2p-build-pull-request-core.yml` | PR validation pipeline |
| `Build/ci-feature-build.yml` | Feature branch CI pipeline |

### Deprecated Patterns / Notes

| Observation | Location | Notes |
|---|---|---|
| `System.ServiceModel.Http/Primitives` (WCF) | `Directory.Packages.props`, `Infrastructure.Webservice` | WCF client support in .NET is provided via `CoreWCF` / `System.ServiceModel` community ports. Modern replacement would be gRPC or REST. |
| `Microsoft.ApplicationInsights.AspNetCore` pinned to `< 3.0.0` | `Directory.Packages.props` comment | Explicitly pinned to 2.23.0 due to Serilog compatibility. This is a known compatibility constraint, not deprecated, but flagged. |
| `CS0618` (obsolete member usage) excluded from warnings-as-errors | `Directory.Build.props` line 79 | Indicates obsolete APIs are in use and suppressed rather than fixed. [NEEDS REVIEW — identify which obsolete APIs] |
| `CS0672` (obsolete override) excluded from warnings-as-errors | `Directory.Build.props` line 79 | Same as above. |
| Selenium tests target `net8.0` | `Test/EndToEnd/Floor2Plan.Selenium/` | All other projects target `net10.0`. The Selenium suite is one major version behind. |
| `CuttingEdge.Conditions.NetStandard` | `Directory.Packages.props` | Older precondition library (last updated 2019). Modern alternative: `ArgumentException.ThrowIfNull` or `ArgumentOutOfRangeException`. |
| `Microsoft.SharePointOnline.CSOM` | `Directory.Packages.props` | Very large package; active usage not confirmed. [NEEDS REVIEW] |
| `HangfireDbContext` extends bare `DbContext` | `Infrastructure.Hangfire.DbMigrations` | Does not extend `BaseDbContext<T>` / ABP context. Migration context only — not used at runtime. |
| `EmptyDbContext` extends bare `DbContext` | `Common.Database` | Purpose unclear — may be a placeholder or test helper. [NEEDS REVIEW] |
| Transitive project references disabled globally | `Directory.Build.props` | `DisableTransitiveProjectReferences=true`. This is a deliberate performance/explicitness choice but means all dependencies must be declared explicitly at every level. |
