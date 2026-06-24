# Floor2Plan.Core — Entry-Point Catalog (Phase 1)

**Generated:** 2026-06-24
**Total entry points cataloged:** ~400+

---

## Summary Statistics

| Category | Count |
|----------|-------|
| HTTP API Endpoints (REST + OData) | 140+ |
| MVC Web Controllers (Razor) | 100+ |
| Hangfire Import Jobs | 25+ |
| Elsa Workflow Activities (custom) | 12+ |
| System Actions (on-demand / scheduled) | 39+ |
| Domain Processors | 68+ |
| Background Services (IHostedService) | 2 |
| SignalR Hubs | 2 |
| Database Deployer (CLI) | 4 |

---

## Cross-Cutting Notes

- **No MediatR or NServiceBus** — async patterns are Hangfire (jobs), Elsa (workflows), and `Infrastructure.FireAndForget` (fire-and-forget).
- **Two Hangfire queues:** `default` (1 worker) and `sync` (1 worker) — prevents import jobs blocking general work.
- **OData layer is generic** — all entity sets use `GenericEntityController<TDto,T>` with per-entity `@ODataRoute` attributes; 100+ sets follow the same pattern.
- **System Actions** use a factory pattern; triggered on-demand via UI (`/System/SystemAction/Run`) and via internal scheduler.
- **Processors** form a reusable data-transformation pipeline; most are idempotent cache-refresh operations invoked from System Actions or Elsa workflow activities.
- **Elsa domain events** (Ticket, Sync, StopWorkOrder) fire from EF Core change handlers and wire into workflow triggers.

---

## 1. Authentication & Authorization (EP-100)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-101 | HTTP | `Src/UI/Floor2Plan.Api/Controllers/AuthController.cs` | POST `/api/auth/login` | Authenticate user with username/password | yes | User, Person, PersonSchedule | none found |
| EP-102 | HTTP | `Src/UI/Floor2Plan.Api/Infrastructure/Attributes/ConfigurableEnableQueryAttribute.cs` | GET `/odata/v1/*` | OData query interceptor for filtering/expansion | no | (all) | none found |

---

## 2. REST API Endpoints (EP-110)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-111 | HTTP | `Src/UI/Floor2Plan.Api/Controllers/TimesheetController.cs` | POST `/api/timesheet/status` | External approval feedback on timesheet status | yes | TimeSheet, TimesheetEntry | none found |
| EP-112 | HTTP | `Src/UI/Floor2Plan.Api/Controllers/ImportController.cs` | POST/PUT `/api/import/productbreakdown` | Import product breakdown tree (async or sync) | yes | Component, Project, Activity, Material | none found |
| EP-113 | HTTP | `Src/UI/Floor2Plan.Api/Controllers/ImportController.cs` | POST/PUT `/api/import/taskallocations` | Import task allocations for a person | yes | Assignment, Activity, Person | none found |

---

## 3. OData Entity Sets (EP-120) — Generic Pattern

All entity sets use `Src/UI/Floor2Plan.Api/Controllers/GenericEntityController.cs`. 100+ sets registered via `@ODataRoute` on Contracts/Models. Sample below; full list is `[NEEDS REVIEW]`.

| ID | Route / Trigger | Primary Tables |
|----|-----------------|----------------|
| EP-120 | GET `/odata/v1/Activities` | Activity |
| EP-121 | GET `/odata/v1/Assignments` | Assignment |
| EP-122 | GET `/odata/v1/Projects` | Project |
| EP-123 | GET `/odata/v1/Components` | Component |
| EP-124 | GET `/odata/v1/Persons` | Person |
| EP-125 | GET `/odata/v1/Organisations` | Organisation |
| EP-126 | GET `/odata/v1/Materials` | Material |
| EP-127 | GET `/odata/v1/TimeSheets` | TimeSheet |
| EP-128 | GET `/odata/v1/Disciplines` | Discipline |
| EP-129 | GET `/odata/v1/AccessControlTimes` | AccessControlTime, ClockingTerminalPunch |
| EP-130 | GET `/odata/v1/ClockingTerminals` | ClockingTerminal |
| EP-131 | GET `/odata/v1/Balances` | Balance, BalancePolicy |
| EP-132 | GET `/odata/v1/CorrectionTimesheets` | CorrectionTimesheet |
| EP-133 | GET `/odata/v1/Baselines` | Baseline, BaselineProject, BaselineComponent, BaselineActivity, BaselineAssignment |
| EP-134 | GET `/odata/v1/ActivitySummaries` | ActivitySummary |
| EP-135 | GET `/odata/v1/AssignmentSummaries` | AssignmentSummary |

[NEEDS REVIEW] Full enumeration of all OData entity sets — examine `@ODataRoute` attributes across `Contracts/`.

---

## 4. MVC Web UI — Root Controllers (EP-200)

### Authentication (EP-200)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-201 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/AccountController.cs` | GET `/Account/Login` | Display login form | no | (none) | none found |
| EP-202 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/AccountController.cs` | POST `/Account/Login` | Process local username/password login | yes | User, Person, PersonSchedule, ClockingTerminal | none found |
| EP-203 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/AccountController.cs` | GET `/Account/AzureAdLogin` | Redirect to Azure AD SSO | no | (auth) | none found |
| EP-204 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/AccountController.cs` | GET `/Account/FloorganiseAzureAdLogin` | Redirect to Floorganise Azure AD SSO | no | (auth) | none found |
| EP-205 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/ElsaController.cs` | GET `/modules/elsa/auth` | Get JWT token for Elsa workflow UI | no | (auth) | none found |

### Global MVC Actions (EP-210)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-210 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/HomeController.cs` | GET `/` | Home page / dashboard entry | no | (none) | none found |
| EP-211 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/PersonInfoController.cs` | GET/POST `/PersonInfo/*` | View/update current user info | yes | Person, PersonSchedule | none found |
| EP-212 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/EventController.cs` | POST `/Event/*` | Handle client-side events (analytics, notifications) | yes | (varies) | none found |
| EP-213 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/AllocationController.cs` | POST `/Allocation/*` | Manage allocations (AJAX) | yes | Assignment, Activity, Person | none found |
| EP-214 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/StateController.cs` | GET/POST `/State/*` | Manage UI state / preferences | yes | PageSetting | none found |
| EP-215 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/SelectController.cs` | GET `/Select/*` | Provide select list options for dropdowns | no | (varies) | none found |
| EP-216 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/OptionListController.cs` | GET `/OptionList/*` | Provide option lists for UI | no | (varies) | none found |
| EP-1005 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/FileController.cs` | GET `/File/*` | Download/upload files | yes | File | none found |
| EP-1006 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/GeneralSettingsController.cs` | GET/POST `/GeneralSettings/*` | Global application settings | yes | AppSetting | none found |
| EP-1007 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/PageSettingsController.cs` | GET/POST `/PageSettings/*` | Page-specific UI settings | yes | PageSetting | none found |
| EP-1008 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/PermissionManagementController.cs` | POST `/PermissionManagement/*` | Manage user permissions/roles | yes | Role, Permission, UserInRole, OrganisationRole | none found |
| EP-1009 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/PersonManagementController.cs` | POST `/PersonManagement/*` | Bulk person/user management | yes | Person, User, PersonSchedule | none found |
| EP-1010 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/NewsFeedController.cs` | POST `/NewsFeed/*` | Manage news feed messages | yes | NewsFeedMessage | none found |
| EP-1011 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/ExternalLinkController.cs` | POST `/ExternalLink/*` | Configure external links | yes | ExternalLink | none found |
| EP-1012 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/GenericPropertyController.cs` | POST `/GenericProperty/*` | Manage custom properties | yes | PropertyType, PropertyTypeOption, PropertySet | none found |
| EP-1013 | HTTP | `Src/UI/UI.Floor2Plan/Controllers/TaskReadinessController.cs` | GET/POST `/TaskReadiness/*` | Manage task readiness status | yes | TaskReadiness, TaskReadinessStatus* | none found |

---

## 5. MVC Area: Sync / Import (EP-300)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-301 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/IndexController.cs` | GET `/Sync/` | Sync module home | no | (none) | none found |
| EP-302 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/IndexController.cs` | GET `/Sync/GetConnectorSyncLogsAsync` | Fetch connector sync logs | no | SyncLog | none found |
| EP-303 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/IndexController.cs` | GET `/Sync/GetSyncLogsAsync` | Fetch manual sync logs by import type | no | SyncLog | none found |
| EP-304 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | GET `/Sync/Import*` | Display import form | no | (none) | none found |
| EP-305 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportAspose*` | Trigger Aspose (Excel) file import | yes | (varies by template) | none found |
| EP-306 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportXer*` | Trigger Oracle Primavera XER import | yes | Project, Activity, Assignment, Component | none found |
| EP-307 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportSciforma*` | Trigger Daptiv/Sciforma import | yes | Project, Activity, Assignment | none found |
| EP-308 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportPbs*` | Trigger PBS (personnel/structure) import | yes | Person, Discipline, Organisation, Project | none found |
| EP-309 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportAccessControl*` | Trigger access control (clocking) import | yes | AccessControlTime, ClockingTerminalPunch, ClockingTerminal | none found |
| EP-310 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportHoursAndProgress*` | Trigger hours/progress import | yes | TimeSheet, AssignmentProgressHistory, Activity | none found |
| EP-311 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportTimesheet*` | Trigger timesheet import from ERP | yes | TimeSheet, TimesheetEntry | none found |
| EP-312 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportTaskReadiness*` | Trigger task readiness import | yes | TaskReadiness, Material, Engineering | none found |
| EP-313 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ExportImportPlanningHours*` | Export/import planning hours (Excel) | yes | Activity, Assignment | none found |
| EP-314 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ExportImportTransferHours*` | Transfer hours between timesheets | yes | TimeSheet, TimesheetEntry | none found |
| EP-315 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportGeneric*` | Trigger generic domain model import | yes | (varies by domain) | none found |
| EP-316 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ImportExportController.cs` | POST `/Sync/ImportProductBreakdown*` | Trigger product breakdown import | yes | Component, Project, Material | none found |
| EP-317 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/PdmController.cs` | POST `/Sync/Pdm/*` | PDM connector actions | yes | (varies) | none found |
| EP-318 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ConnectorConfigurationController.cs` | POST `/Sync/ConnectorConfiguration/*` | Configure connector (ERP/Planning/EntryTime) | yes | ImportConfig, ImportConfigRule | none found |
| EP-319 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/ConvertPlanningFileController.cs` | POST `/Sync/ConvertPlanningFile/*` | Convert planning file formats | no | (file only) | none found |

---

## 6. MVC Area: PBS / Planning (EP-400)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-401 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/IndexController.cs` | GET `/Pbs/` | PBS module home | no | (none) | none found |
| EP-402 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ProjectController.cs` | POST `/Pbs/Project/*` | Create/update/delete projects | yes | Project, ProjectProperty, ProjectMaterial | none found |
| EP-403 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ActivityController.cs` | POST `/Pbs/Activity/*` | Create/update/delete activities | yes | Activity, ActivityProperty, ActivityRelation | none found |
| EP-404 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ComponentController.cs` | POST `/Pbs/Component/*` | Create/update/delete components (WBS) | yes | Component, ComponentProperty, ComponentType | none found |
| EP-405 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/WorController.cs` | POST `/Pbs/Wor/*` | Manage work orders (WOR) | yes | Wor, WorType | none found |
| EP-406 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/AssignmentController.cs` | POST `/Pbs/Assignment/*` | Create/update/delete assignments | yes | Assignment, AssignmentProperty, AssignmentProgressHistory | none found |
| EP-407 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/PersonController.cs` | POST `/Pbs/Person/*` | Create/update/delete personnel | yes | Person, PersonProperty, PersonSchedule, DisciplinePerson | none found |
| EP-408 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/DisciplineController.cs` | POST `/Pbs/Discipline/*` | Create/update/delete disciplines | yes | Discipline, DisciplinePerson, DisciplineCapacity | none found |
| EP-409 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/OrganisationController.cs` | POST `/Pbs/Organisation/*` | Create/update/delete organisations | yes | Organisation, OrganisationProperty, OrganisationCapacity | none found |
| EP-410 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ScheduleController.cs` | POST `/Pbs/Schedule/*` | Create/update/delete schedules (shifts) | yes | Schedule, ScheduleEntry, PersonSchedule, OrganisationSchedule | none found |
| EP-411 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/BaselineController.cs` | POST `/Pbs/Baseline/*` | Create/update/delete baselines (snapshots) | yes | Baseline, BaselineProject, BaselineComponent, BaselineActivity, BaselineAssignment | none found |
| EP-412 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/AssetController.cs` | POST `/Pbs/Asset/*` | Create/update/delete assets (equipment) | yes | Asset, AssetType | none found |
| EP-413 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/MaterialController.cs` | POST `/Pbs/Material/*` | Create/update/delete materials | yes | Material, ProjectMaterial | none found |
| EP-414 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/RoleController.cs` | POST `/Pbs/Role/*` | Create/update/delete roles | yes | Role, UserInRole, OrganisationRole | none found |
| EP-415 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/UserController.cs` | POST `/Pbs/User/*` | Create/update/delete users | yes | User, UserInRole | none found |
| EP-416 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/HolidayController.cs` | POST `/Plan/Holiday/*` | Create/update/delete holidays | yes | PersonHoliday, OrganisationHoliday | none found |
| EP-417 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ActivityRelationController.cs` | POST `/Plan/ActivityRelation/*` | Manage activity relationships (predecessors/successors) | yes | ActivityRelation | none found |
| EP-418 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/CopyController.cs` | POST `/Pbs/Copy/*` | Copy project/activity/component structures | yes | Project, Activity, Component, Assignment | none found |
| EP-419 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/ProjectScenarioController.cs` | POST `/Pbs/ProjectScenario/*` | Create/update/delete project scenarios | yes | ProjectScenario, ProjectScenarioActivity | none found |

---

## 7. MVC Area: HR / Personnel (EP-500)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-501 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/ClockingTerminalController.cs` | POST `/HR/ClockingTerminal/*` | Manage clocking terminals | yes | ClockingTerminal, ClockingTerminalProfile | none found |
| EP-502 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/ClockingTerminalProfileController.cs` | POST `/HR/ClockingTerminalProfile/*` | Configure clocking terminal profiles (RFID, access) | yes | ClockingTerminalProfile, RfidSetting | none found |
| EP-503 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/PersonBalanceController.cs` | GET `/HR/PersonBalance/*` | View employee time/overtime balances | no | Balance, BalancePolicy, Person | none found |
| EP-504 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/PersonBalanceController.cs` | POST `/HR/PersonBalance/*` | Update person balance policies | yes | Balance, BalancePolicy | none found |
| EP-505 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/BalancePolicyController.cs` | POST `/HR/BalancePolicy/*` | Create/update/delete balance policies | yes | BalancePolicy, BalancePolicyRule, BalanceAccumulationRule | none found |
| EP-506 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/BalancePolicyRuleController.cs` | POST `/HR/BalancePolicyRule/*` | Manage balance policy rules | yes | BalancePolicyRule | none found |
| EP-507 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/BalanceAccumulationRuleController.cs` | POST `/HR/BalanceAccumulationRule/*` | Manage balance accumulation rules | yes | BalanceAccumulationRule | none found |
| EP-508 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/OffTimeController.cs` | POST `/HR/OffTime/*` | Manage off-time / leave | yes | PersonHoliday, OffTime | none found |
| EP-509 | HTTP | `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/ScheduleManagementController.cs` | POST `/HR/ScheduleManagement/*` | Manage work schedules / shift assignments | yes | PersonSchedule, OrganisationSchedule | none found |

---

## 8. MVC Area: Execution / Do (EP-600)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-601 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/PlanboardController.cs` | GET `/Do/Planboard` | Display planboard (activity planning view) | no | Activity, Assignment, Component | none found |
| EP-602 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/PlanboardController.cs` | POST `/Do/Planboard/*` | Update assignments on planboard (drag-drop) | yes | Assignment, AssignmentProgressHistory | none found |
| EP-603 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/FloorboardController.cs` | GET `/Do/Floorboard` | Display floorboard (real-time shop floor status) | no | Activity, Assignment, Person, ClockingTerminal | none found |
| EP-604 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/FloorboardController.cs` | POST `/Do/Floorboard/*` | Update shop floor status (clocking, progress) | yes | TimeSheet, ClockingTerminalPunch, Activity | none found |
| EP-605 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/WeeklyTimesheetController.cs` | GET `/Do/WeeklyTimesheet` | Display weekly timesheet for user | no | TimeSheet, TimesheetEntry, Person | none found |
| EP-606 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/WeeklyTimesheetController.cs` | POST `/Do/WeeklyTimesheet/Submit*` | Submit timesheet for approval | yes | TimeSheet, TimesheetEntry, TimeSheetStatus | none found |
| EP-607 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/WeeklyTimesheetController.cs` | POST `/Do/WeeklyTimesheet/Update*` | Update timesheet entry lines | yes | TimesheetEntry, TimeSheet | none found |
| EP-608 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/EmployeeTimesheetController.cs` | GET `/Do/EmployeeTimesheet` | View employee timesheets (manager view) | no | TimeSheet, TimesheetEntry, Person | none found |
| EP-609 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/EmployeeTimesheetController.cs` | POST `/Do/EmployeeTimesheet/*` | Manage employee timesheet approvals | yes | TimeSheet, TimeSheetStatus | none found |
| EP-610 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/CorrectionTimesheetController.cs` | POST `/Do/CorrectionTimesheet/*` | Submit/manage correction timesheets | yes | CorrectionTimesheet, CorrectionTimesheetWeeklyCorrectionLine | none found |
| EP-611 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/ReporterController.cs` | POST `/Do/Reporter/*` | Report on activities/timesheets | yes | (varies) | none found |

---

## 9. MVC Area: Check / Reporting (EP-700)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-701 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ReportsController.cs` | GET `/Check/Reports` | Display available reports | no | (none) | none found |
| EP-702 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ReportDesignerController.cs` | GET/POST `/Check/ReportDesigner` | Design/modify reports | yes | (report definitions) | none found |
| EP-703 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ReportsApiController.cs` | POST `/Check/ReportsApi/*` | Execute report queries | no | (varies by report) | none found |
| EP-704 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/KPIController.cs` | GET `/Check/KPI` | View KPI dashboard | no | KpiOverdue, KpiOverDuration | none found |
| EP-705 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ErpWeeklyTimesheetController.cs` | GET `/Check/ErpWeeklyTimesheet` | View ERP timesheet integration status | no | TimeSheet, ErpHourline | none found |
| EP-706 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/ErpCorrectionTimesheetController.cs` | GET `/Check/ErpCorrectionTimesheet` | View ERP correction timesheet status | no | CorrectionTimesheet, ErpHourline | none found |

---

## 10. MVC Area: Tickets (EP-800)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-801 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Ticket/Controllers/TicketController.cs` | POST `/Ticket/Ticket/*` | Create/update/delete tickets | yes | Ticket, TicketProperty, TicketRemark | none found |
| EP-802 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Ticket/Controllers/TicketController.cs` | GET `/Ticket/Ticket/*` | View ticket details | no | Ticket, TicketRemark, Activity | none found |
| EP-803 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Ticket/Controllers/TransportRequestController.cs` | POST `/Ticket/TransportRequest/*` | Create/update/delete transport requests | yes | TransportRequest, Activity, Component | none found |
| EP-804 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Ticket/Controllers/StopWorkOrderController.cs` | POST `/Ticket/StopWorkOrder/*` | Create/manage stop work orders | yes | StopWorkOrder, Activity | none found |

---

## 11. MVC Areas: FloorSpace, Materials, Devices (EP-900)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-901 | HTTP | `Src/UI/UI.Floor2Plan/Areas/FloorSpace/Controllers/FloorSpaceController.cs` | POST `/FloorSpace/FloorSpace/*` | Manage floorspace configurations | yes | FloorSpace, Location | none found |
| EP-902 | HTTP | `Src/UI/UI.Floor2Plan/Areas/FloorSpace/Controllers/LocationController.cs` | POST `/FloorSpace/Location/*` | Manage factory locations | yes | Location | none found |
| EP-911 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Material/Controllers/MaterialController.cs` | POST `/Material/Material/*` | Create/update/delete materials | yes | Material | none found |
| EP-912 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Material/Controllers/ProjectMaterialController.cs` | POST `/Material/ProjectMaterial/*` | Manage project material requirements | yes | ProjectMaterial, ProjectMaterialUsageHistory | none found |
| EP-921 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Devices/Controllers/ShopFloorTerminalController.cs` | POST `/Devices/ShopFloorTerminal/*` | Configure shop floor terminals | yes | ShopFloorTerminal, ShopFloorTerminalSetting | none found |
| EP-922 | HTTP | `Src/UI/UI.Floor2Plan/Areas/Devices/Controllers/ClockingTerminalController.cs` | POST `/Devices/ClockingTerminal/*` | Manage clocking terminals | yes | ClockingTerminal, ClockingTerminalProfile | none found |

---

## 12. MVC Area: System / Admin (EP-1000)

| ID | Type | Location | Route / Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|-----------------|-------------|---------|----------------|-------|
| EP-1001 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/Controllers/SystemActionController.cs` | GET `/System/SystemAction/GetAll` | List available system actions | no | (none) | none found |
| EP-1002 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/Controllers/SystemActionController.cs` | POST `/System/SystemAction/Run` | Execute system action (run processor) | yes | (varies) | none found |
| EP-1003 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/Controllers/TableController.cs` | GET/POST `/System/Table/*` | Manage system tables | yes | (varies) | none found |
| EP-1004 | HTTP | `Src/UI/UI.Floor2Plan/Areas/System/Controllers/LicenseController.cs` | GET/POST `/System/License/*` | Manage licenses | yes | License | none found |

---

## 13. Hangfire Import Jobs (EP-2000)

All jobs run on the `sync` queue (1 dedicated worker).

| ID | Type | Location | Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|---------|-------------|---------|----------------|-------|
| EP-2001 | Job | `Src/Application/Application.Sync/ImportJobs/Xer/ImportXerJob.cs` | Hangfire: sync queue | Import Oracle Primavera XER file | yes | Project, Activity, Assignment, Component, Material, ActivityRelation | none found |
| EP-2002 | Job | `Src/Application/Application.Sync/ImportJobs/Sciforma/ImportSciformaJob.cs` | Hangfire: sync queue | Import Daptiv/Sciforma planning data | yes | Project, Activity, Assignment | none found |
| EP-2003 | Job | `Src/Application/Application.Sync/ImportJobs/Planning/ImportPlanningJob.cs` | Hangfire: sync queue | Import planning hours/progress from file | yes | Activity, Assignment, AssignmentProgressHistory | none found |
| EP-2004 | Job | `Src/Application/Application.Sync/ImportJobs/Aspose/ImportAsposeJob.cs` | Hangfire: sync queue | Import Excel file via Aspose | yes | (varies by template) | none found |
| EP-2005 | Job | `Src/Application/Application.Sync/ImportJobs/HoursAndProgress/ImportHoursAndProgressJob.cs` | Hangfire: sync queue | Import hours and progress data | yes | TimeSheet, AssignmentProgressHistory | none found |
| EP-2006 | Job | `Src/Application/Application.Sync/ImportJobs/Employees/ImportEmployeeJob.cs` | Hangfire: sync queue | Import employee data (legacy) | yes | Person, DisciplinePerson | none found |
| EP-2007 | Job | `Src/Application/Application.Sync/ImportJobs/Timesheets/ImportTimesheetJob.cs` | Hangfire: sync queue | Import timesheet entries from ERP | yes | TimeSheet, TimesheetEntry | none found |
| EP-2008 | Job | `Src/Application/Application.Sync/ImportJobs/AccessControl/ImportAccessControlJob.cs` | Hangfire: sync queue | Import clocking/access control data | yes | AccessControlTime, ClockingTerminalPunch, ClockingTerminal | none found |
| EP-2009 | Job | `Src/Application/Application.Sync/ImportJobs/ProductBreakdown/ImportProductBreakdownJob.cs` | Hangfire: sync queue | Import product/component breakdown | yes | Component, Project, Material | none found |
| EP-2010 | Job | `Src/Application/Application.Sync/ImportJobs/TaskReadiness/ImportTaskReadinessJob.cs` | Hangfire: sync queue | Import task readiness data | yes | TaskReadiness, Material, Engineering | none found |
| EP-2011 | Job | `Src/Application/Application.Sync/ImportJobs/OrganisationImport/ImportOrganisationJob.cs` | Hangfire: sync queue | Import organisation structure | yes | Organisation, Discipline | none found |
| EP-2012 | Job | `Src/Application/Application.Sync/ImportJobs/DisciplineImport/ImportDisciplineJob.cs` | Hangfire: sync queue | Import discipline data | yes | Discipline, DisciplinePerson | none found |
| EP-2013 | Job | `Src/Application/Application.Sync/ImportJobs/Materials/ImportMaterialJob.cs` | Hangfire: sync queue | Import materials | yes | Material | none found |
| EP-2014 | Job | `Src/Application/Application.Sync/ImportJobs/ProjectMaterials/ImportProjectMaterialJob.cs` | Hangfire: sync queue | Import project material allocations | yes | ProjectMaterial | none found |
| EP-2015 | Job | `Src/Application/Application.Sync/ImportJobs/Persons/ImportPersonDomainModelJob.cs` | Hangfire: sync queue | Import persons (domain model v2) | yes | Person, PersonProperty, PersonSchedule | none found |
| EP-2016 | Job | `Src/Application/Application.Sync/ImportJobs/Organisations/ImportOrganisationDomainModelJob.cs` | Hangfire: sync queue | Import organisations (domain model v2) | yes | Organisation, OrganisationProperty | none found |
| EP-2017 | Job | `Src/Application/Application.Sync/ImportJobs/Disciplines/ImportDisciplineDomainModelJob.cs` | Hangfire: sync queue | Import disciplines (domain model v2) | yes | Discipline, DisciplinePerson | none found |
| EP-2018 | Job | `Src/Application/Application.Sync/ImportJobs/ComponentTypes/ImportComponentTypeDomainModelJob.cs` | Hangfire: sync queue | Import component types | yes | ComponentType, ComponentProperty | none found |
| EP-2019 | Job | `Src/Application/Application.Sync/ImportJobs/Materials/ImportMaterialDomainModelJob.cs` | Hangfire: sync queue | Import materials (domain model v2) | yes | Material | none found |
| EP-2020 | Job | `Src/Application/Application.Sync/ImportJobs/UnitTypes/ImportUnitTypeDomainModelJob.cs` | Hangfire: sync queue | Import unit types | yes | UnitType | none found |
| EP-2021 | Job | `Src/Application/Application.Sync/ImportJobs/PropertyTypes/ImportPropertyTypeJob.cs` | Hangfire: sync queue | Import property type definitions | yes | PropertyType, PropertyTypeOption | none found |
| EP-2022 | Job | `Src/Application/Application.Sync/ImportJobs/ActivityPhases/ImportActivityPhaseDomainModelJob.cs` | Hangfire: sync queue | Import activity phases | yes | ActivityPhase | none found |
| EP-2023 | Job | `Src/Application/Application.Sync/ImportJobs/ActivityStatus/ImportActivityStatusDomainModelJob.cs` | Hangfire: sync queue | Import activity statuses | yes | ActivityStatus | none found |
| EP-2024 | Job | `Src/Application/Application.Sync/ImportJobs/ProjectStatusses/ImportProjectStatusDomainModelJob.cs` | Hangfire: sync queue | Import project statuses | yes | ProjectStatus | none found |
| EP-2025 | Job | `Src/Application/Application.Sync/ImportJobs/HourTypes/ImportHourTypeDomainModelJob.cs` | Hangfire: sync queue | Import hour type definitions | yes | HourType | none found |

---

## 14. Background Services (EP-2100)

| ID | Type | Location | Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|---------|-------------|---------|----------------|-------|
| EP-2101 | IHostedService | `Src/Infrastructure/Infrastructure.Hangfire/JobStateEventHostedService.cs` | Startup | Listen to Hangfire job state changes; forward to handlers | no | (Hangfire schema) | none found |
| EP-2102 | SignalR | `Src/Infrastructure/Infrastructure.Hangfire/JobStateEventHub.cs` | Real-time | Notify clients of job state changes (progress) | no | (none) | none found |
| EP-2103 | IHostedService | `Src/Infrastructure/Infrastructure.Health/DelayedHealthCheckPublisherHostedService.cs` | Startup | Delayed health check publishing | no | HealthCheckLog | none found |

---

## 15. Elsa Workflow — Custom Activities (EP-3000)

| ID | Type | Location | Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|---------|-------------|---------|----------------|-------|
| EP-3001 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TicketStatusUpdateActivity.cs` | `Set state` activity | Update ticket/transport request status | yes | TransportRequest, Ticket | none found |
| EP-3002 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TicketActivityCreateActivity.cs` | `Create activity` activity | Create activity linked to ticket | yes | Activity, Ticket | none found |
| EP-3003 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TicketActivityCloseActivity.cs` | `Close activity` activity | Close activity related to ticket | yes | Activity, Ticket | none found |
| EP-3004 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TicketSendMailActivity.cs` | `Send mail` activity | Send email notification (ticket) | yes | (email log) | none found |
| EP-3005 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/TransportRequestActivityUpdateActivity.cs` | `Update activity` activity | Update activity from transport request | yes | Activity, TransportRequest | none found |
| EP-3006 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Sync/RunConnectorActivity.cs` | `Run connector` activity | Execute ERP/Planning/EntryTime connector sync | yes | SyncLog, (varies) | none found |
| EP-3007 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Sync/SyncLogSendMailActivity.cs` | `Send mail` activity | Send sync result notification email | yes | (email log) | none found |
| EP-3008 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/StopWorkOrder/StopWorkOrderSendMailActivity.cs` | `Send mail` activity | Send stop work order notification | yes | (email log) | none found |
| EP-3009 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Processor/RunProcessorActivity.cs` | `Run processor` activity | Execute a domain processor | yes | (varies) | none found |
| EP-3010 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Data/GetActivityByIdActivity.cs` | `Get activity` activity | Retrieve activity from database | no | Activity | none found |
| EP-3011 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Data/SetActivityStatusActivity.cs` | `Set status` activity | Update activity status | yes | Activity | none found |
| EP-3012 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Activities/Data/ActivityStatusGateActivity.cs` | `Gate` activity | Conditional gate based on activity status | no | Activity | none found |

---

## 16. Elsa Workflow — Domain Event Triggers (EP-3100)

| ID | Type | Location | Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|---------|-------------|---------|----------------|-------|
| EP-3101 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Events/Ticket/TicketStateChangeEvent.cs` | Domain event | Ticket status changes | yes | Ticket | none found |
| EP-3102 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Events/Ticket/TicketRemarkAddedEvent.cs` | Domain event | New remark/comment on ticket | yes | TicketRemark | none found |
| EP-3103 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Events/Ticket/TicketUpdatedEvent.cs` | Domain event | Generic ticket modification | yes | Ticket | none found |
| EP-3104 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Events/Ticket/TransportRequestCreatedEvent.cs` | Domain event | New transport request created | yes | TransportRequest | none found |
| EP-3105 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Events/Ticket/StopWorkOrder/StopWorkOrderCreatedEvent.cs` | Domain event | New stop work order | yes | StopWorkOrder | none found |
| EP-3106 | Workflow | `Src/Infrastructure/Infrastructure.Workflow/Events/SyncFinishEvent.cs` | Domain event | Connector sync complete | yes | SyncLog | none found |
| EP-3201 | SignalR | `Src/Infrastructure/Infrastructure.Elsa/UI/WorkflowInstanceProxyHub.cs` | Real-time | Real-time workflow execution status updates | no | (Elsa schema) | none found |

---

## 17. System Actions (EP-4000) — On-Demand or Scheduled

Invoked via `POST /System/SystemAction/Run` or scheduled internally. All are idempotent (safe to re-run).

| ID | Location | Description | Mutates | Primary Tables |
|----|----------|-------------|---------|----------------|
| EP-4001 | `RefreshActivityHierarchyCacheSystemAction.cs` | Refresh activity hierarchy cache | yes | Activity (cache) |
| EP-4002 | `RefreshComponentHierarchyCacheSystemAction.cs` | Refresh component/WBS hierarchy cache | yes | Component (cache) |
| EP-4003 | `RefreshOrganisationHierarchyCacheSystemAction.cs` | Refresh organisation structure cache | yes | Organisation (cache) |
| EP-4004 | `RefreshDisciplineHierarchyCacheSystemAction.cs` | Refresh discipline hierarchy cache | yes | Discipline (cache) |
| EP-4005 | `RefreshStructureHierarchyCacheSystemAction.cs` | Refresh general structure hierarchy cache | yes | Structure (cache) |
| EP-4006 | `RefreshActivitySummariesSystemAction.cs` | Recalculate activity summary rollups | yes | ActivitySummary |
| EP-4007 | `RefreshAllDistributionsSystemAction.cs` | Refresh all distribution caches | yes | (cache tables) |
| EP-4008 | `RefreshCacheHoursWorkedSystemAction.cs` | Recalculate hours worked aggregates | yes | (cache) |
| EP-4009 | `RefreshCachePersonalSchedulesSystemAction.cs` | Refresh personal schedule cache | yes | CachePersonalSchedule |
| EP-4010 | `RefreshComponentLabelsSystemAction.cs` | Refresh component label cache | yes | (cache) |
| EP-4011 | `RefreshStructureLabelsSystemAction.cs` | Refresh structure label cache | yes | (cache) |
| EP-4012 | `RefreshComponentRelationCacheSystemAction.cs` | Refresh component relationship cache | yes | (cache) |
| EP-4013 | `RefreshComponentStatusCacheSystemAction.cs` | Refresh component status cache | yes | (cache) |
| EP-4014 | `RefreshComponentStructureHierarchySystemAction.cs` | Refresh component structure hierarchy | yes | (cache) |
| EP-4015 | `RefreshDailyAssignmentDistributionSystemAction.cs` | Refresh daily assignment distributions | yes | (cache) |
| EP-4016 | `RefreshDailyPersonDistributionSystemAction.cs` | Refresh daily person/resource distributions | yes | (cache) |
| EP-4017 | `RefreshDateDistributionSystemAction.cs` | Refresh date-based distributions | yes | (cache) |
| EP-4018 | `RefreshHoursBudgetWeightSystemAction.cs` | Recalculate hours budget weights | yes | (cache) |
| EP-4019 | `RefreshLocationHierarchyCacheSystemAction.cs` | Refresh location hierarchy cache | yes | (cache) |
| EP-4020 | `RefreshMaterialHierarchyCacheSystemAction.cs` | Refresh material hierarchy cache | yes | Material (cache) |
| EP-4021 | `RefreshOrganisationCapacityDashboardLineDistributionSystemAction.cs` | Refresh organisation capacity dashboard | yes | OrganisationCapacityDashboardLine |
| EP-4022 | `RefreshPersonCacheEfficiencySystemAction.cs` | Refresh person efficiency cache | yes | (cache) |
| EP-4023 | `RefreshPredecessorReadySystemAction.cs` | Refresh predecessor ready status cache | yes | (cache) |
| EP-4024 | `RefreshProjectTypeHierarchyCacheSystemAction.cs` | Refresh project type hierarchy | yes | ProjectType (cache) |
| EP-4025 | `RefreshRoleCacheUserTypeSystemAction.cs` | Refresh role user type cache | yes | (cache) |
| EP-4026 | `RefreshScheduleSourceTypeSystemAction.cs` | Refresh schedule source type cache | yes | (cache) |
| EP-4027 | `RefreshStructureRelationCacheSystemAction.cs` | Refresh structure relationship cache | yes | (cache) |
| EP-4028 | `RefreshActivitySummaryModeCacheSystemAction.cs` | Refresh activity summary mode cache | yes | (cache) |
| EP-4029 | `RefreshAssetTypeHierarchyCacheSystemAction.cs` | Refresh asset type hierarchy | yes | AssetType (cache) |
| EP-4030 | `RefreshCurrentPersonBalancePolicyIds.cs` | Update person balance policy cache | yes | (cache) |
| EP-4031 | `RefreshPermissionGrantDataSystemAction.cs` | Refresh permission grants cache | yes | (cache) |
| EP-4032 | `UpdateBalancesSystemAction.cs` | Recalculate all employee balances | yes | Balance, BalanceMutationHistory |
| EP-4033 | `TaskReadinessStatusUpdateSystemAction.cs` | Update task readiness status | yes | TaskReadiness, Material, Engineering |
| EP-4034 | `FloorspaceGenerationSystemAction.cs` | Generate floorspace configuration | yes | FloorSpace, Location |
| EP-4035 | `SqlDefragmentationSystemAction.cs` | Database index/heap defragmentation | yes | (all tables — index) |
| EP-4036 | `ReloadAppSettingsSystemAction.cs` | Reload application settings from database | yes | AppSetting |
| EP-4037 | `CleanupUserInRolesSystemAction.cs` | Clean up orphaned user-role assignments | yes | UserInRole |
| EP-4038 | `RefreshMessageBoxMessagesSystemAction.cs` | Refresh message box/notification cache | yes | (cache) |
| EP-4039 | `RescheduleTemplatesSystemAction.cs` | Reschedule templates (recurring patterns) | yes | Schedule, ScheduleEntry |

All System Actions are in `Src/Application/Application.SystemActions/`. Tests: none found.

---

## 18. Domain Processors (EP-5000) — Data Processing Pipeline

Processors are invoked from System Actions (EP-4000), Elsa activities (EP-3009), or import jobs. All are in `Src/Processors/Processors.Domain/`.

### Sync Processors (EP-5000)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5001 | Base processor for all sync operations | (various) |
| EP-5002 | Process planning data (hours, progress) | Activity, Assignment, AssignmentProgressHistory |
| EP-5003 | Process ERP timesheet/hours data | TimeSheet, TimesheetEntry, ErpHourline |
| EP-5004 | Process entry time/clocking data | AccessControlTime, ClockingTerminalPunch, Person |
| EP-5005 | Generic processor for flexible data mapping | (varies) |
| EP-5006 | Process component/product breakdown | Component, Project, Material |

### Cache / Hierarchy Processors (EP-5100)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5101 | Cache hierarchy structures (generic) | (cache tables) |
| EP-5102 | Update structure relationship cache | (cache) |
| EP-5103 | Refresh component hours aggregates | (cache) |
| EP-5104 | Refresh personal schedule cache | CachePersonalSchedule |
| EP-5105 | Refresh activity summary rollups | ActivitySummary |

### Data Transformation / Maintenance Processors (EP-5200)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5201 | Assign hour types to ERP hourlines | ErpHourline, HourType |
| EP-5202 | Book hours to assignments (timesheet processing) | TimeSheet, Assignment, AssignmentProgressHistory |
| EP-5203 | Write assignment progress history | AssignmentProgressHistory |
| EP-5204 | Apply/recalculate assignment summary | AssignmentSummary |
| EP-5205 | Apply/recalculate activity summary | ActivitySummary |
| EP-5206 | Recalculate budget weight distributions | (cache) |
| EP-5207 | Set activity execution sequence | Activity, ActivityRelation |
| EP-5208 | Create ERP hourline records | ErpHourline, TimeSheet |
| EP-5209 | Create project baseline snapshot | Baseline, BaselineProject, BaselineComponent, BaselineActivity, BaselineAssignment |

### Component / Structure Processors (EP-5300)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5301 | Create component hierarchy structures | Component, ComponentType |
| EP-5302 | Refresh component structure hierarchy cache | (cache) |
| EP-5303 | Refresh component relationship data | (cache) |

### Distribution / Dashboard Processors (EP-5400)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5401 | Recalculate daily person distributions | (cache) |
| EP-5402 | Recalculate daily assignment distributions | (cache) |
| EP-5403 | Refresh date-based distributions | (cache) |
| EP-5404 | Refresh baseline assignment distributions | (cache) |
| EP-5405 | Recalculate organisation capacity dashboard | OrganisationCapacityDashboardLine |
| EP-5406 | Recalculate discipline capacity dashboard | DisciplineCapacityDashboardLine |
| EP-5407 | Refresh all distribution caches | (all cache tables) |

### Prediction / Analytics Processors (EP-5500)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5501 | Calculate long-term assignment predictions | AssignmentDurationPrediction |
| EP-5502 | Calculate short-term assignment predictions | AssignmentShortTermPrediction |

### Balance / HR Processors (EP-5700)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5701 | Recalculate employee time/overtime balances | Balance, BalanceMutationHistory |
| EP-5702 | Update pending hours in balance | Balance |
| EP-5703 | Update balance after timesheet submission | Balance, TimeSheet, TimesheetEntry |

### Maintenance / Cleanup Processors (EP-5800)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5801 | Remove orphaned page settings | PageSetting |
| EP-5802 | Clean up orphaned allocations | Allocation |
| EP-5803 | Clean up orphaned user-role mappings | UserInRole |
| EP-5804 | Clean up health check logs | HealthCheckLog |
| EP-5805 | Execute arbitrary SQL commands (admin) | (any table) |
| EP-5806 | Database index/heap defragmentation | (all tables — index) |

### FloorSpace / Timeline Processors (EP-5900)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5901 | Generate floorspace layouts | FloorSpace, Location |
| EP-5902 | Refresh activity group surface (timeline) | (cache) |
| EP-5903 | Refresh actual start/finish time cache | (cache) |

### Miscellaneous Processors (EP-5950–5983)

| ID | Description | Primary Tables |
|----|-------------|----------------|
| EP-5951 | Refresh file metadata cache | File (cache) |
| EP-5952 | Refresh task ID cache | (cache) |
| EP-5953 | Recalculate bookable hours | (cache) |
| EP-5954 | Refresh predecessor ready status cache | (cache) |
| EP-5955 | Refresh person balance policy cache | (cache) |
| EP-5956 | Refresh role user type cache | (cache) |
| EP-5957 | Refresh schedule source type cache | (cache) |
| EP-5971 | Update task readiness predecessor status | TaskReadiness |
| EP-5972 | Update activities from connector data | Activity, ActivityProperty |
| EP-5973 | Process entry time/clocking records | AccessControlTime, ClockingTerminalPunch |
| EP-5974 | Clear provider/service caches | (cache) |
| EP-5981 | Refresh timesheet entry cache after update | (cache) |
| EP-5982 | Refresh all timesheet entries | TimesheetEntry, (cache) |
| EP-5983 | Refresh timesheet event messages | TimesheetEventMessage |

---

## 19. Database Deployer / CLI (EP-6000)

| ID | Type | Location | Trigger | Description | Mutates | Primary Tables | Tests |
|----|------|----------|---------|-------------|---------|----------------|-------|
| EP-6001 | CLI | `Src/DatabaseDeployer/DatabaseDeployer/Program.cs` | Console entry point | EF Core migration runner (all contexts) | yes | (all tables during migration) | none found |
| EP-6002 | Processor | `Src/DatabaseDeployer/DatabaseDeployer.Domain/Migrators/ContextMigrator.cs` | IContextMigrator | Migrate individual DbContext schemas | yes | (all tables) | none found |
| EP-6003 | Processor | `Src/DatabaseDeployer/DatabaseDeployer.Domain/Migrators/SchemaMigrator.cs` | ISchemaMigrator | Migrate database schema | yes | (all tables) | none found |
| EP-6004 | Processor | `Src/DatabaseDeployer/DatabaseDeployer.Domain/Utils/MigrationScriptRunner.cs` | IMigrationScriptRunner | Execute custom migration scripts | yes | (any table) | none found |

---

## Items Requiring Human Review

| Item | Notes |
|------|-------|
| Full OData entity set list | [NEEDS REVIEW] Enumerate all `@ODataRoute` attributes in Contracts/ — estimated 100+ sets |
| Connector scheduler configuration | [NEEDS REVIEW] How are Elsa workflow triggers (EP-3100) scheduled — via Elsa timer or external cron? |
| RecurringJob registrations | [NEEDS REVIEW] Where are `RecurringJob.AddOrUpdate` calls made for import jobs? |
| System Action scheduler | [NEEDS REVIEW] Are EP-4000 system actions also run on a cron schedule, or purely on-demand? |
| Fire-and-Forget usage | [NEEDS REVIEW] Enumerate all callers of `IFireAndForgetService` in `Infrastructure.FireAndForget` |
| Full HTTP route list per controller | [NEEDS REVIEW] Each controller has multiple action methods — enumerate all verbs per route |

---

*Phase 1 complete. Proceed to Phase 2 (bounded context map) after review.*
