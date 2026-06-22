# Phase 1 — Entry-Point Catalog

> Input: [00-inventory.md](00-inventory.md)
> Note: MVC controllers serve Razor/SPA views unless explicitly flagged **API** in the description column.
> `existing_tests` is [NEEDS REVIEW] across the board — a dedicated test-mapping pass is required.

---

## Domain Area: Authentication & Security

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-001 | HTTP | `Src/UI/Floor2Plan.Api/Controllers/AuthController.cs` · `LoginAsync` | POST api/Auth/LoginAsync | JWT login with username + password; returns bearer token | yes | [NEEDS REVIEW] |
| EP-002 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/AccountController.cs` | GET/POST Account/* | Razor login/logout pages; local cookie and Azure AD sign-in | yes | [NEEDS REVIEW] |
| EP-003 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/ElsaController.cs` | GET modules/elsa/auth?returnUri=... | Auth gateway: validates `Workflow.Manage` permission and appends bearer token to returnUri for Elsa designer | no | [NEEDS REVIEW] |
| EP-004 | Interceptor | `Src/Infrastructure/Infrastructure.Authorisation/PermissionAuthorisationInterceptor.cs` | Every method call on services decorated with `[PermissionAuthorise]` | ABP dynamic-proxy interceptor; enforces permission checks before method execution | no | [NEEDS REVIEW] |
| EP-005 | Interceptor | `Src/Infrastructure/Infrastructure.Authorisation/Helper/ApiKeyRequirementHandler.cs` | ASP.NET `AuthorizationHandler<ApiKeyRequirement>` | Validates `X-API-KEY` header for CI/build-agent policy | no | [NEEDS REVIEW] |
| EP-006 | Interceptor | `Src/Data/Data.Base/AzureAuthentication/AzureAuthenticationInterceptor.cs` | EF Core `DbConnectionInterceptor` — connection opening | Injects Azure AD access token into SQL connection before it opens | no | [NEEDS REVIEW] |

---

## Domain Area: Planning (Activities, Assignments, Relations)

*MVC Area: `Plan`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-010 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Plan/Controllers/ActivityController.cs` | GET/POST Plan/Activity/* | Activity CRUD and planning board actions | yes | [NEEDS REVIEW] |
| EP-011 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Plan/Controllers/ActivityRelationController.cs` | GET/POST Plan/ActivityRelation/* | Activity dependency/relation management | yes | [NEEDS REVIEW] |
| EP-012 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Plan/Controllers/AssignmentController.cs` | GET/POST Plan/Assignment/* | Assignment (person-to-activity) CRUD | yes | [NEEDS REVIEW] |
| EP-013 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Plan/Controllers/HolidayController.cs` | GET/POST Plan/Holiday/* | Person and organisation holiday management | yes | [NEEDS REVIEW] |
| EP-014 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/AllocationController.cs` | GET/POST Allocation/* | Person-to-organisation allocation management | yes | [NEEDS REVIEW] |
| EP-015 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/StructureController.cs` | GET/POST Structure/* | Structure and structure relation management | yes | [NEEDS REVIEW] |
| EP-016 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/SelectController.cs` | GET Select/* | Lookup/select endpoints for dropdowns (read-only) | no | [NEEDS REVIEW] |
| EP-017 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/StateController.cs` | GET/POST State/* | Component/activity state transitions | yes | [NEEDS REVIEW] |
| EP-018 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/GenericPropertyController.cs` | GET/POST GenericProperty/* | Generic property type CRUD | yes | [NEEDS REVIEW] |

---

## Domain Area: PBS / Product Breakdown Structure

*MVC Area: `Pbs`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-030 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ComponentController.cs` | GET/POST Pbs/Component/* | Component (shipyard object) CRUD | yes | [NEEDS REVIEW] |
| EP-031 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ComponentTypeController.cs` | GET/POST Pbs/ComponentType/* | Component type management | yes | [NEEDS REVIEW] |
| EP-032 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ProjectController.cs` | GET/POST Pbs/Project/* | Project CRUD and settings | yes | [NEEDS REVIEW] |
| EP-033 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ProjectTypeController.cs` | GET/POST Pbs/ProjectType/* | Project type management | yes | [NEEDS REVIEW] |
| EP-034 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ProjectScenarioController.cs` | GET/POST Pbs/ProjectScenario/* | Project scenario (what-if) CRUD | yes | [NEEDS REVIEW] |
| EP-035 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/OrganisationController.cs` | GET/POST Pbs/Organisation/* | Organisation CRUD | yes | [NEEDS REVIEW] |
| EP-036 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/OrganisationPropertyController.cs` | GET/POST Pbs/OrganisationProperty/* | Organisation property management | yes | [NEEDS REVIEW] |
| EP-037 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/OrganisationCapacityController.cs` | GET/POST Pbs/OrganisationCapacity/* | Organisation capacity dashboard | no | [NEEDS REVIEW] |
| EP-038 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/PersonController.cs` | GET/POST Pbs/Person/* | Person CRUD | yes | [NEEDS REVIEW] |
| EP-039 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/PersonTimesheetApproverController.cs` | GET/POST Pbs/PersonTimesheetApprover/* | Timesheet approver assignments | yes | [NEEDS REVIEW] |
| EP-040 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/DisciplineController.cs` | GET/POST Pbs/Discipline/* | Discipline CRUD | yes | [NEEDS REVIEW] |
| EP-041 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/DisciplineCapacityController.cs` | GET/POST Pbs/DisciplineCapacity/* | Discipline capacity dashboard | no | [NEEDS REVIEW] |
| EP-042 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/RoleController.cs` | GET/POST Pbs/Role/* | Role and permission management | yes | [NEEDS REVIEW] |
| EP-043 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/UserController.cs` | GET/POST Pbs/User/* | User account management | yes | [NEEDS REVIEW] |
| EP-044 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ScheduleController.cs` | GET/POST Pbs/Schedule/* | Schedule and schedule entries | yes | [NEEDS REVIEW] |
| EP-045 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/StructureTypeController.cs` | GET/POST Pbs/StructureType/* | Structure type management | yes | [NEEDS REVIEW] |
| EP-046 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ActivityPropertyController.cs` | GET/POST Pbs/ActivityProperty/* | Activity property type management | yes | [NEEDS REVIEW] |
| EP-047 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/AssetController.cs` | GET/POST Pbs/Asset/* | Asset management | yes | [NEEDS REVIEW] |
| EP-048 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/BaselineController.cs` | GET/POST Pbs/Baseline/* | Project baseline creation and comparison | yes | [NEEDS REVIEW] |
| EP-049 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/CopyController.cs` | POST Pbs/Copy/* | Component/activity copy operations | yes | [NEEDS REVIEW] |
| EP-050 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/PlanningController.cs` | GET/POST Pbs/Planning/* | Planning configuration | yes | [NEEDS REVIEW] |
| EP-051 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/WorController.cs` | GET/POST Pbs/Wor/* | Weak object relation (WOR) management | yes | [NEEDS REVIEW] |
| EP-052 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/IndexController.cs` | GET Pbs/Index | PBS module home view | no | [NEEDS REVIEW] |
| EP-053 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/SettingsController.cs` | GET/POST Pbs/Settings/* | PBS-level settings | yes | [NEEDS REVIEW] |
| EP-054 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ShipyardController.cs` | GET/POST Pbs/Shipyard/* | Shipyard configuration view (route: system-administration/shipyard/*) | yes | [NEEDS REVIEW] |

---

## Domain Area: Timesheet & Work Execution

*MVC Area: `Do`; REST API: `api/Timesheet`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-060 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/EmployeeTimesheetController.cs` | GET/POST Do/EmployeeTimesheet/* | Employee timesheet entry and submission | yes | [NEEDS REVIEW] |
| EP-061 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/WeeklyTimesheetController.cs` | GET/POST Do/WeeklyTimesheet/* | Weekly timesheet review and approval | yes | [NEEDS REVIEW] |
| EP-062 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/CorrectionTimesheetController.cs` | GET/POST Do/CorrectionTimesheet/* | Timesheet correction entry | yes | [NEEDS REVIEW] |
| EP-063 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/PlanboardController.cs` | GET/POST Do/Planboard/* | Assignment planboard view | no | [NEEDS REVIEW] |
| EP-064 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/FloorboardController.cs` | GET/POST Do/Floorboard/* | Floorboard (shop floor) view | no | [NEEDS REVIEW] |
| EP-065 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/ReporterController.cs` | GET/POST Do/Reporter/* | Reporter dashboard view | no | [NEEDS REVIEW] |
| EP-066 | HTTP API | `Src/UI/Floor2Plan.Api/Controllers/TimesheetController.cs` · `Status` | POST api/Timesheet/Status | External approval status update (ERP integration) | yes | [NEEDS REVIEW] |
| EP-067 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/EventController.cs` | GET/POST Event/* | Timesheet event messages and milestone events | yes | [NEEDS REVIEW] |
| EP-068 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/NewsFeedController.cs` | GET/POST NewsFeed/* | News feed messages | yes | [NEEDS REVIEW] |

---

## Domain Area: HR / Balance / Scheduling

*MVC Area: `HR`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-070 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/BalanceController.cs` | GET/POST HR/Balance/* | Person balance overview | no | [NEEDS REVIEW] |
| EP-071 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/BalancePolicyController.cs` | GET/POST HR/BalancePolicy/* | Balance policy management | yes | [NEEDS REVIEW] |
| EP-072 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/BalancePolicyRuleController.cs` | GET/POST HR/BalancePolicyRule/* | Balance policy rules | yes | [NEEDS REVIEW] |
| EP-073 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/BalanceAccumulationRuleController.cs` | GET/POST HR/BalanceAccumulationRule/* | Balance accumulation rules | yes | [NEEDS REVIEW] |
| EP-074 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/PersonBalanceController.cs` | GET/POST HR/PersonBalance/* | Per-person balance detail | no | [NEEDS REVIEW] |
| EP-075 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/OffTimeController.cs` | GET/POST HR/OffTime/* | Off-time / personal holiday requests | yes | [NEEDS REVIEW] |
| EP-076 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/ScheduleManagementController.cs` | GET/POST HR/ScheduleManagement/* | Schedule management for persons and organisations | yes | [NEEDS REVIEW] |
| EP-077 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/ClockingTerminalController.cs` | GET/POST HR/ClockingTerminal/* | Clocking terminal configuration | yes | [NEEDS REVIEW] |
| EP-078 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/ClockingTerminalProfileController.cs` | GET/POST HR/ClockingTerminalProfile/* | Clocking terminal profile management | yes | [NEEDS REVIEW] |

---

## Domain Area: Reporting & KPI (Check)

*MVC Area: `Check`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-080 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ReportsController.cs` | GET Check/Reports/* (catch-all for Vue routes) | Report viewer shell | no | [NEEDS REVIEW] |
| EP-081 | HTTP API | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ReportsApiController.cs` | GET/POST Check/ReportsApi/* | API endpoints powering report data | no | [NEEDS REVIEW] |
| EP-082 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ReportDesignerController.cs` | GET/POST Check/ReportDesigner/* | Report designer views | yes | [NEEDS REVIEW] |
| EP-083 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/KPIController.cs` | GET/POST Check/KPI/* | KPI overview | no | [NEEDS REVIEW] |
| EP-084 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/Kpi/KpiSpiCpiController.cs` | GET/POST Check/KpiSpiCpi/* | SPI/CPI schedule and cost performance indexes | no | [NEEDS REVIEW] |
| EP-085 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ErpCorrectionTimesheetController.cs` | GET/POST Check/ErpCorrectionTimesheet/* | ERP timesheet correction review | yes | [NEEDS REVIEW] |
| EP-086 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ErpWeeklyTimesheetController.cs` | GET/POST Check/ErpWeeklyTimesheet/* | ERP weekly timesheet review | no | [NEEDS REVIEW] |

---

## Domain Area: Sync / Import / Export (Data Integration)

*MVC Area: `Sync`; API: `api/Import`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-090 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | GET/POST Sync/ImportExport/* | File-based import/export trigger (Excel, XER, Aspose, Sciforma) | yes | [NEEDS REVIEW] |
| EP-091 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ConnectorConfigurationController.cs` | GET/POST Sync/ConnectorConfiguration/* | Connector configuration management | yes | [NEEDS REVIEW] |
| EP-092 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/IndexController.cs` | GET Sync/Index | Sync module home view | no | [NEEDS REVIEW] |
| EP-093 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/PdmController.cs` | GET/POST Sync/Pdm/* | PDM (product data management) sync | yes | [NEEDS REVIEW] |
| EP-094 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/PlanningHoursController.cs` | GET/POST Sync/PlanningHours/* | Planning hours sync views | yes | [NEEDS REVIEW] |
| EP-095 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ProductModelLinkController.cs` | GET/POST Sync/ProductModelLink/* | Product model link management | yes | [NEEDS REVIEW] |
| EP-096 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/TransferHoursController.cs` | GET/POST Sync/TransferHours/* | Hour transfer operations | yes | [NEEDS REVIEW] |
| EP-097 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ConvertPlanningFileController.cs` | GET/POST Sync/ConvertPlanningFile/* | Planning file format conversion | yes | [NEEDS REVIEW] |
| EP-098 | HTTP API | `Src/UI/Floor2Plan.Api/Controllers/ImportController.cs` · `ProductBreakdownAsync` | PUT+POST api/Import/ProductBreakdownAsync | REST: import product breakdown data (returns 201/202) | yes | [NEEDS REVIEW] |
| EP-099 | HTTP API | `Src/UI/Floor2Plan.Api/Controllers/ImportController.cs` · `TaskAllocationsAsync` | PUT+POST api/Import/TaskAllocationsAsync | REST: import task allocation data | yes | [NEEDS REVIEW] |
| EP-100 | Job | `Src/Application/Application.Sync/ImportJobs/Planning/ImportPlanningJob.cs` | Hangfire one-shot · queue: sync | Import XER/MS Project planning fields (task_name, start/end dates) | yes | [NEEDS REVIEW] |
| EP-101 | Job | `Src/Application/Application.Sync/ImportJobs/Xer/ImportXerJob.cs` | Hangfire one-shot · queue: sync | Import Oracle Primavera P6 XER files; backwards-compat mode | yes | [NEEDS REVIEW] |
| EP-102 | Job | `Src/Application/Application.Sync/ImportJobs/Sciforma/ImportSciformaJob.cs` | Hangfire one-shot · queue: sync | Import Sciforma project data | yes | [NEEDS REVIEW] |
| EP-103 | Job | `Src/Application/Application.Sync/ImportJobs/HoursAndProgress/ImportHoursAndProgressJob.cs` | Hangfire one-shot · queue: sync | Import manual hours and progress | yes | [NEEDS REVIEW] |
| EP-104 | Job | `Src/Application/Application.Sync/ImportJobs/TaskReadiness/ImportTaskReadinessJob.cs` | Hangfire one-shot · queue: sync | Import task readiness status | yes | [NEEDS REVIEW] |
| EP-105 | Job | `Src/Application/Application.Sync/ImportJobs/ProductBreakdown/ImportProductBreakdownJob.cs` | Hangfire one-shot · queue: sync | Import product breakdown structure | yes | [NEEDS REVIEW] |
| EP-106 | Job | `Src/Application/Application.Sync/ImportJobs/OrganisationImport/ImportOrganisationJob.cs` | Hangfire one-shot · queue: sync | Import organisation/role data via ERP service | yes | [NEEDS REVIEW] |
| EP-107 | Job | `Src/Application/Application.Sync/ImportJobs/DisciplineImport/ImportDisciplineJob.cs` | Hangfire one-shot · queue: sync | Import discipline data | yes | [NEEDS REVIEW] |
| EP-108 | Job | `Src/Application/Application.Sync/ImportJobs/Materials/ImportMaterialJob.cs` | Hangfire one-shot · queue: sync | Import material data | yes | [NEEDS REVIEW] |
| EP-109 | Job | `Src/Application/Application.Sync/ImportJobs/ProjectMaterials/ImportProjectMaterialJob.cs` | Hangfire one-shot · queue: sync | Import project-level material assignments | yes | [NEEDS REVIEW] |
| EP-110 | Job | `Src/Application/Application.Sync/ImportJobs/AccessControl/ImportAccessControlJob.cs` | Hangfire one-shot · queue: sync | Import access control settings | yes | [NEEDS REVIEW] |
| EP-111 | Job | `Src/Application/Application.Sync/ImportJobs/HourTypes/ImportHourTypeDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import hour type domain model (Excel) | yes | [NEEDS REVIEW] |
| EP-112 | Job | `Src/Application/Application.Sync/ImportJobs/UnitTypes/ImportUnitTypeDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import unit type domain model (Excel) | yes | [NEEDS REVIEW] |
| EP-113 | Job | `Src/Application/Application.Sync/ImportJobs/ProjectStatusses/ImportProjectStatusDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import project status domain model | yes | [NEEDS REVIEW] |
| EP-114 | Job | `Src/Application/Application.Sync/ImportJobs/Disciplines/ImportDisciplineDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import discipline domain model with properties | yes | [NEEDS REVIEW] |
| EP-115 | Job | `Src/Application/Application.Sync/ImportJobs/ComponentTypes/ImportComponentTypeDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import component type domain model with properties | yes | [NEEDS REVIEW] |
| EP-116 | Job | `Src/Application/Application.Sync/ImportJobs/Organisations/ImportOrganisationDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import organisation domain model with properties | yes | [NEEDS REVIEW] |
| EP-117 | Job | `Src/Application/Application.Sync/ImportJobs/Materials/ImportMaterialDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import material domain model with properties | yes | [NEEDS REVIEW] |
| EP-118 | Job | `Src/Application/Application.Sync/ImportJobs/ActivityPhases/ImportActivityPhaseDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import activity phase domain model | yes | [NEEDS REVIEW] |
| EP-119 | Job | `Src/Application/Application.Sync/ImportJobs/ActivityStatus/ImportActivityStatusDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import activity status domain model | yes | [NEEDS REVIEW] |
| EP-120 | Job | `Src/Application/Application.Sync/ImportJobs/Persons/ImportPersonDomainModelJob.cs` | Hangfire one-shot · queue: sync | Import person domain model with properties | yes | [NEEDS REVIEW] |
| EP-121 | Job | `Src/Infrastructure/Infrastructure.FireAndForget/Jobs/SyncFireAndForgetJob.cs` | Hangfire one-shot · queue: sync | Generic sync-queue fire-and-forget dispatcher (routes ISyncQueueJob processors) | yes | [NEEDS REVIEW] |

---

## Domain Area: Tickets & Transport Requests

*MVC Area: `Ticket`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-130 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Ticket/Controllers/TicketController.cs` | GET/POST Ticket/Ticket/* | General ticket CRUD and status transitions | yes | [NEEDS REVIEW] |
| EP-131 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Ticket/Controllers/TransportRequestController.cs` | GET/POST Ticket/TransportRequest/* | Transport request management | yes | [NEEDS REVIEW] |
| EP-132 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Ticket/Controllers/IssueController.cs` | GET/POST Ticket/Issue/* | Issue / defect ticket management | yes | [NEEDS REVIEW] |
| EP-133 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Ticket/Controllers/StopWorkOrderController.cs` | GET/POST Ticket/StopWorkOrder/* | Stop work order management | yes | [NEEDS REVIEW] |

---

## Domain Area: Devices (Clocking & Shop Floor Terminals)

*MVC Area: `Devices`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-140 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Devices/Controllers/ClockingTerminalController.cs` | GET/POST Devices/ClockingTerminal/* | Clocking terminal device management | yes | [NEEDS REVIEW] |
| EP-141 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Devices/Controllers/ShopFloorTerminalController.cs` | GET/POST Devices/ShopFloorTerminal/* | Shop floor terminal device management | yes | [NEEDS REVIEW] |

---

## Domain Area: Material

*MVC Area: `Material`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-145 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Material/Controllers/MaterialController.cs` | GET/POST Material/Material/* | Material management | yes | [NEEDS REVIEW] |
| EP-146 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Material/Controllers/ProjectMaterialController.cs` | GET/POST Material/ProjectMaterial/* | Project material assignments | yes | [NEEDS REVIEW] |

---

## Domain Area: FloorSpace

*MVC Area: `FloorSpace`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-150 | HTTP | `Src/UI/UI.Floor2Plan/Areas/FloorSpace/Controllers/FloorSpaceController.cs` | GET/POST floorspace/* (catch-all) | FloorSpace planning and assignment view | yes | [NEEDS REVIEW] |
| EP-151 | HTTP | `Src/UI/UI.Floor2Plan/Areas/FloorSpace/Controllers/LocationController.cs` | GET/POST FloorSpace/Location/* | Location management | yes | [NEEDS REVIEW] |

---

## Domain Area: Prediction

*MVC Area: `Prediction`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-155 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Prediction/PredictionController.cs` | GET/POST Prediction/* | Long/short-term prediction views | no | [NEEDS REVIEW] |

---

## Domain Area: System Administration

*MVC Area: `System`*

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-160 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/Controllers/AdministrationController.cs` | GET System-Administration/* (catch-all) | System administration dashboard | no | [NEEDS REVIEW] |
| EP-161 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/Controllers/SystemActionController.cs` | GET/POST System/SystemAction/* | Triggers system actions (cache refresh, SQL defrag, etc.) | yes | [NEEDS REVIEW] |
| EP-162 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/Controllers/AppSettingController.cs` | GET/POST System/AppSetting/* | Application settings management | yes | [NEEDS REVIEW] |
| EP-163 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/Controllers/LicenseController.cs` | GET/POST System/License/* | License management | yes | [NEEDS REVIEW] |
| EP-164 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/Controllers/TableController.cs` | GET/POST System/Table/* | Database table viewer | no | [NEEDS REVIEW] |
| EP-165 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/ShipyardController.cs` | GET/POST System/Shipyard/* | Shipyard-level system settings | yes | [NEEDS REVIEW] |
| EP-166 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/PermissionManagementController.cs` | GET/POST PermissionManagement/* | Role and permission assignment | yes | [NEEDS REVIEW] |
| EP-167 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/PersonManagementController.cs` | GET/POST PersonManagement/* | User and person management | yes | [NEEDS REVIEW] |
| EP-168 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/PersonInfoController.cs` | GET PersonInfo/* | Person information display (read-only) | no | [NEEDS REVIEW] |
| EP-169 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/GeneralSettingsController.cs` | GET/POST GeneralSettings/* | General application settings | yes | [NEEDS REVIEW] |
| EP-170 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/PageSettingsController.cs` | GET/POST PageSettings/* | Per-user page settings (column visibility, filters) | yes | [NEEDS REVIEW] |
| EP-171 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/TeamController.cs` | GET/POST Team/* | Team management | yes | [NEEDS REVIEW] |
| EP-172 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/TaskReadinessController.cs` | GET/POST TaskReadiness/* | Task readiness status management | yes | [NEEDS REVIEW] |
| EP-173 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/ExternalLinkController.cs` | GET ExternalLink/* | External link redirects | no | [NEEDS REVIEW] |
| EP-174 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/FileController.cs` | GET/POST File/* | File upload and download | yes | [NEEDS REVIEW] |
| EP-175 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/OptionListController.cs` | GET OptionList/* | Option list lookups | no | [NEEDS REVIEW] |
| EP-176 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/ClientInitController.cs` | GET ClientInit/* | Client-side initialisation data (read-only) | no | [NEEDS REVIEW] |
| EP-177 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/ScriptsController.cs` | GET Scripts/* | Dynamic script bundles | no | [NEEDS REVIEW] |
| EP-178 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/HomeController.cs` | GET / | Application shell / SPA entry point | no | [NEEDS REVIEW] |
| EP-179 | CLI | `Src/DatabaseDeployer/DatabaseDeployer/Program.cs` | CLI arg: `schema-only` or none | Database migration tool: `schema-only` runs schema-only migration; default runs all EF migrations + SQL scripts | yes | [NEEDS REVIEW] |

---

## Domain Area: Workflows & Automation (Elsa)

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-180 | HTTP | `modules/elsa/api/*` (Elsa REST API) | Elsa built-in REST endpoints via `UseWorkflowsApi` + `ElsaF2PMiddleware` | Create/manage/trigger Elsa workflows via REST | yes | [NEEDS REVIEW] |
| EP-181 | Other | `Src/Infrastructure/Infrastructure.Elsa/UI/WorkflowInsanceProxyHub.cs` | SignalR hub: `modules/elsa/hubs/workflow-instance-proxy` | Real-time workflow execution status for Elsa designer | no | [NEEDS REVIEW] |
| EP-182 | Job | `Elsa: RunWorkflowJob` (via `WorkflowSchedulerLocalTimeZone.cs`) | Hangfire recurring · cron expression · local timezone | Starts a new Elsa workflow instance on cron schedule | yes | [NEEDS REVIEW] |
| EP-183 | Job | `Elsa: ResumeWorkflowJob` (via `WorkflowSchedulerLocalTimeZone.cs`) | Hangfire recurring · cron expression · local timezone | Resumes a suspended Elsa workflow instance on schedule | yes | [NEEDS REVIEW] |
| EP-184 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Processor/RunProcessorActivity.cs` | Elsa activity: "Run processor" | Dynamically resolves and executes a processor by type + config; Success/Fail branch | yes | [NEEDS REVIEW] |
| EP-185 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Sync/RunConnectorActivity.cs` | Elsa activity: "Run connector" | Enqueues sync processor (Planning/EntryTime/ERP/Generic); outputs sync correlation ID | yes | [NEEDS REVIEW] |
| EP-186 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Sync/SyncLogSendMailActivity.cs` | Elsa activity: "Send sync log mail" | Loads sync log and sends mail to recipients | no | [NEEDS REVIEW] |
| EP-187 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Data/SetActivityStatusActivity.cs` | Elsa activity: "Set activity status" | Sets an activity's status in the database | yes | [NEEDS REVIEW] |
| EP-188 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Data/GetActivityByIdActivity.cs` | Elsa activity: "Get activity by ID" | Reads an activity's StatusId from the database | no | [NEEDS REVIEW] |
| EP-189 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Data/ActivityStatusGateActivity.cs` | Elsa activity: "Assert activity status" | Routes workflow Success/Fail based on activity status match | no | [NEEDS REVIEW] |
| EP-190 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TicketStatusUpdateActivity.cs` | Elsa activity: "Set state" | Updates TransportRequest status; optionally sends status mail | yes | [NEEDS REVIEW] |
| EP-191 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TicketSendMailActivity.cs` | Elsa activity: "Send transport request mail" | Sends transport request status mail to configured recipients | no | [NEEDS REVIEW] |
| EP-192 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TicketActivityCreateActivity.cs` | Elsa activity: "Create mitigating activity for transport request" | Creates a mitigating activity linked to the transport request | yes | [NEEDS REVIEW] |
| EP-193 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TicketActivityCloseActivity.cs` | Elsa activity: "Complete transport request's mitigating activities" | Sets mitigating activities to 100% progress | yes | [NEEDS REVIEW] |
| EP-194 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TransportRequestActivityUpdateActivity.cs` | Elsa activity: "Update mitigating activity" | Updates mitigating activity windows/constraints to match execution date | yes | [NEEDS REVIEW] |
| EP-195 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/StopWorkOrder/StopWorkOrderSendMailActivity.cs` | Elsa activity: "Send stop work order mail" | Sends stop work order email to affected persons | no | [NEEDS REVIEW] |
| EP-196 | Message | `Src/Infrastructure/Infrastructure.Elsa/Utilities/WorkflowTransientVariablePopulator.cs` | `INotificationHandler<WorkflowExecuting>` (Elsa mediator) | Injects current IPrincipal into workflow execution context as transient variable | no | [NEEDS REVIEW] |

---

## Domain Area: Health & Monitoring

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-200 | HTTP | `Src/UI/UI.Floor2Plan/Floor2PlanUIWebModule.cs` | GET /Health/plain | Plain-text health check; auth: CiBuildAgent (bypassed in Debug) | no | [NEEDS REVIEW] |
| EP-201 | HTTP | `Src/UI/UI.Floor2Plan/Floor2PlanUIWebModule.cs` | GET /Health/json | JSON health report; auth: CiBuildAgent (bypassed in Debug) | no | [NEEDS REVIEW] |
| EP-202 | HTTP | `Src/UI/UI.Floor2Plan/Floor2PlanUIWebModule.cs` | GET /Mitigate/json | Runs mitigating actions on unhealthy checks and returns result | yes | [NEEDS REVIEW] |
| EP-203 | Other | `Src/Infrastructure/Infrastructure.Hangfire/Floor2PlanInfrastructureHangfireModule.cs` | GET /hangfire | Hangfire dashboard; auth: HangfireDashboardAuthorizationFilter | no | [NEEDS REVIEW] |
| EP-204 | Other | `Src/Infrastructure/Infrastructure.Health/DelayedHealthCheckPublisherHostedService.cs` | BackgroundService loop; configurable period (minutes) | Runs all health checks on schedule; publishes results to IHealthCheckPublisher | no | [NEEDS REVIEW] |

---

## Domain Area: Fire-and-Forget Infrastructure (Cross-Cutting)

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-210 | Job | `Src/Infrastructure/Infrastructure.FireAndForget/Jobs/FireAndForgetJob.cs` | Hangfire one-shot · queue: default | Main fire-and-forget dispatcher; runs IFireAndForgetExecutable actions (cache, balance, label processors) | yes | [NEEDS REVIEW] |
| EP-211 | Other | `Src/Infrastructure/Infrastructure.Hangfire/JobStateEventHostedService.cs` | BackgroundService channel reader; triggered by Hangfire state changes | Dispatches IJobStateChangeEventHandler handlers when Hangfire job states change | no | [NEEDS REVIEW] |

---

## Domain Area: External API — OData

> 131 read-only entity sets are auto-generated at startup by `GenericControllerFeatureProvider` for each `IODataDto<>` implementation.
> All use `GET /odata/{EntitySetName}` with OData query syntax ($filter, $select, $expand, $top, $skip).
> All require `[Authorize(AuthenticationSchemes = F2PAuthenticationPolicy.Api)]` + `[PermissionAuthorise(ModulePermissions.General.OData)]`.

| EP-### | type | location | route_or_trigger | short_description | mutates_state | existing_tests |
|--------|------|----------|------------------|-------------------|---------------|----------------|
| EP-220 | HTTP OData | `Src/UI/Floor2Plan.Api/Controllers/GenericEntityController.cs` | GET /odata/Activities | OData read: Activities | no | [NEEDS REVIEW] |
| EP-221 | HTTP OData | (same controller, different entity) | GET /odata/Assignments | OData read: Assignments | no | [NEEDS REVIEW] |
| EP-222 | HTTP OData | (same controller) | GET /odata/Allocations | OData read: Allocations | no | [NEEDS REVIEW] |
| EP-223 | HTTP OData | (same controller) | GET /odata/Persons | OData read: Persons | no | [NEEDS REVIEW] |
| EP-224 | HTTP OData | (same controller) | GET /odata/Organisations | OData read: Organisations | no | [NEEDS REVIEW] |
| EP-225 | HTTP OData | (same controller) | GET /odata/Projects | OData read: Projects | no | [NEEDS REVIEW] |
| EP-226 | HTTP OData | (same controller) | GET /odata/TimesheetEntries | OData read: TimesheetEntries | no | [NEEDS REVIEW] |
| EP-227 | HTTP OData | (same controller) | GET /odata/Components | OData read: Components | no | [NEEDS REVIEW] |
| EP-228 | HTTP OData | (same controller) | GET /odata/ClockingTerminalPunches | OData read: ClockingTerminalPunches (Guid? key) | no | [NEEDS REVIEW] |
| EP-229 | HTTP OData | (same controller) | GET /odata/{...} (123 additional entity sets) | Full list in `Src/UI/Floor2Plan.Api/Contracts/Models/`; all read-only OData GET endpoints | no | [NEEDS REVIEW] |

---

## Domain Area: Change Handlers (Post-SaveChanges Triggers)

> All handlers implement `IEntityChangeHandler` via `BaseChangeHandler`. They fire pre- and post-`SaveChangesAsync`.
> They are internal integration points, not external entry points, but they mutate state and trigger downstream side effects.
> Located in: `Src/Data/Data.ChangeHandlers/`

| EP-### | type | location | entity/trigger | short_description | mutates_state |
|--------|------|----------|----------------|-------------------|---------------|
| EP-300 | Interceptor | `RecordAuditChangeHandler.cs` | IRecordAudit (pre-save) | Sets CreatedBy/On and LastModifiedBy/On audit fields | yes |
| EP-301 | Interceptor | `ActivityChangeHandler.cs` | Activity (post-save) | Updates component status cache, can-book-hours, windows, floor space | yes |
| EP-302 | Interceptor | `AssignmentChangeHandler.cs` | Assignment (post-save) | Updates can-book-hours; triggers readiness state | yes |
| EP-303 | Interceptor | `AssignmentProgressChangeHandler.cs` | Assignment (post-save) | Auto-calculates progress; updates activity status by threshold | yes |
| EP-304 | Interceptor | `AllocationChangeHandler.cs` | Allocation (post-save) | Updates person timesheet organisation IDs; manages auto-book cache | yes |
| EP-305 | Interceptor | `TimesheetEntryChangeHandler.cs` | TimesheetEntry (post-save) | Refreshes booked/clocked hours cache | yes |
| EP-306 | Interceptor | `TimesheetEntryUpdateBalancesChangeHandler.cs` | TimesheetEntry (post-save) | Updates person balance pending hours on status changes | yes |
| EP-307 | Interceptor | `ScheduleChangeHandler.cs` | Schedule, PersonSchedule, OvertimeEntry (post-save) | Refreshes timesheet entry cache and personal schedule caches | yes |
| EP-308 | Interceptor | `BalanceChangeHandler.cs` | Balance (post-save) | Creates balance mutation history; prevents balance deletion | yes |
| EP-309 | Interceptor | `TicketChangeHandler.cs` | TicketBase (post-save) | Generates ticket codes; creates remarks on status changes | yes |
| EP-310 | Interceptor | `ComponentChangeHandler.cs` | Component (pre-save) | Removes WORs and resets component type defaults on delete | yes |
| EP-311 | Interceptor | `OrganisationChangeHandler.cs` | Organisation (pre-save) | Closes allocations when organisation is disabled | yes |
| EP-312 | Interceptor | `ActivityConnectorChangeHandler.cs` | Activity (post-save) | Syncs activity changes with external planning connectors (fire-and-forget) | yes |
| EP-313 | Interceptor | `CacheChangeHandler.cs` | Activity, Component, Structure, Organisation + others (post-save) | Refreshes hierarchy cache via fire-and-forget processor | yes |
| EP-314 | Interceptor | `CapacityChangeHandler.cs` | Assignment, Project, Activity (post-save) | Refreshes capacity dashboard line distributions | yes |
| EP-315 | Interceptor | `LongTermPredictionChangeHandler.cs` | Assignment, Activity, Component (post-save) | Triggers long-term prediction processor when enabled | yes |
| EP-316 | Interceptor | `ShortTermPredictionChangeHandler.cs` | Assignment, TimeSheetWeeklyHourline (post-save) | Triggers short-term prediction processor when enabled | yes |
| EP-317 | Interceptor | `PermissionGrantChangeHandler.cs` | PermissionGrant (post-save) | Clears provider cache; refreshes role cache | yes |
| EP-318 | Interceptor | `*(49 additional change handlers)*` | Various entities | Full list in `Src/Data/Data.ChangeHandlers/` — 63 total concrete handlers | yes |

---

## Domain Area: Processors (Fire-and-Forget Background Work)

> All processors are dispatched via `IFireAndForget` from change handlers or system actions, running as Hangfire `FireAndForgetJob` on the `default` queue.
> Located in: `Src/Processors/Processors.Domain/`
> Full list: 65 concrete processor classes.

| EP-### | type | location | trigger | short_description | mutates_state |
|--------|------|----------|---------|-------------------|---------------|
| EP-400 | Job | `RefreshActivitySummaryProcessor` | FireAndForgetJob (default queue) | Aggregated activity summaries | yes |
| EP-401 | Job | `RefreshDailyPersonDistributionProcessor` | FireAndForgetJob (default queue) | Daily per-person distribution data | yes |
| EP-402 | Job | `RefreshDailyAssignmentDistributionProcessor` | FireAndForgetJob (default queue) | Daily per-assignment distribution data | yes |
| EP-403 | Job | `HierarchyCacheProcessor` | FireAndForgetJob (default queue) | Generic hierarchy cache refresh | yes |
| EP-404 | Job | `LongTermPredictionProcessor` | FireAndForgetJob (default queue) | Long-term capacity predictions | yes |
| EP-405 | Job | `ShortTermPredictionProcessor` | FireAndForgetJob (default queue) | Short-term capacity predictions | yes |
| EP-406 | Job | `SyncPlanningProcessor` | SyncFireAndForgetJob (sync queue) | Syncs planning data from external connector | yes |
| EP-407 | Job | `SyncErpProcessor` | SyncFireAndForgetJob (sync queue) | Syncs ERP data | yes |
| EP-408 | Job | `SyncEntryTimeProcessor` | SyncFireAndForgetJob (sync queue) | Syncs entry-time data | yes |
| EP-409 | Job | `SyncGenericProcessor` | SyncFireAndForgetJob (sync queue) | Generic connector sync | yes |
| EP-410 | Job | `UpdateBalanceProcessor` | FireAndForgetJob (default queue) | Person/organisation balance recalculation | yes |
| EP-411 | Job | `FloorSpaceGenerationProcessor` | FireAndForgetJob (default queue) | FloorSpace data generation | yes |
| EP-412 | Job | `SqlDefragmentationProcessor` | FireAndForgetJob (default queue) | SQL Server index defragmentation | yes |
| EP-413 | Job | `*(52 additional processors)*` | FireAndForgetJob (default or sync queue) | Full list in `Src/Processors/Processors.Domain/` | yes |

---

## Domain Area: System Actions (Admin-Triggered Background Work)

> 42 system action classes invoked from `System/SystemActionController`. Each enqueues one or more processors via `IFireAndForget`.
> Located in: `Src/Application/Application.SystemActions/`

| EP-### | type | location | trigger | short_description | mutates_state |
|--------|------|----------|---------|-------------------|---------------|
| EP-450 | Other | `RefreshActivitySummariesSystemAction` | HTTP POST → FireAndForgetJob | Refresh all activity summaries | yes |
| EP-451 | Other | `RefreshAllDistributionsSystemAction` | HTTP POST → FireAndForgetJob | Refresh all distribution processors | yes |
| EP-452 | Other | `SqlDefragmentationSystemAction` | HTTP POST → FireAndForgetJob | SQL index defragmentation | yes |
| EP-453 | Other | `UpdateBalancesSystemAction` | HTTP POST → FireAndForgetJob | Full balance recalculation | yes |
| EP-454 | Other | `ReloadAppSettingsSystemAction` | HTTP POST (direct) | Reloads application settings in memory | yes |
| EP-455 | Other | `RescheduleTemplatesSystemAction` | HTTP POST (direct) | Reschedules Elsa workflow templates | yes |
| EP-456 | Other | `CleanupUserInRolesSystemAction` | HTTP POST → FireAndForgetJob | Removes inconsistent user-role assignments | yes |
| EP-457 | Other | `*(35 additional system actions)*` | HTTP POST → FireAndForgetJob | Full list in `Src/Application/Application.SystemActions/` | yes |

---

## Summary Statistics

| Category | Count |
|----------|-------|
| HTTP MVC controllers (UI Areas + root) | 75 |
| HTTP REST API controllers (Floor2Plan.Api) | 3 |
| HTTP OData entity sets (auto-generated) | 131 |
| Hangfire import jobs (sync queue) | 21 |
| Hangfire fire-and-forget dispatchers | 2 |
| Hangfire Elsa recurring jobs | 2 |
| Elsa workflow activities | 12 |
| Background services (IHostedService) | 3 (1 is decorator) |
| EF/ABP interceptors | 2 |
| Entity change handlers | 63 |
| Processors | 65 |
| System actions | 42 |
| Import providers | 25 |
| CLI entry points | 1 |
| SignalR hubs | 1 |
| Health check endpoints | 3 |
| **Total catalogued entry points** | **~525** |

---

## Key observations for Phase 2

- **God context anti-pattern**: `Floor2PlanDbContext` (~150 DbSets) is mutated by all 63 change handlers — no context isolation exists today.
- **No external message bus**: all async work routes through Hangfire `FireAndForgetJob` (default queue) or `SyncFireAndForgetJob` (sync queue). There is no NServiceBus, MassTransit, or similar.
- **Change handler pipeline is the primary integration bus**: 63 handlers fire on every `SaveChangesAsync`; they are the main coupling mechanism between bounded contexts.
- **OData API is read-only**: all 131 OData endpoints are GET-only; writes go through the MVC controllers or REST API.
- **Elsa workflow engine is the only scheduling/orchestration abstraction** above raw Hangfire.
- **Two import queues**: `default` (cache/balance/label work) and `sync` (all connector and import jobs) — a natural seam for modularization.
