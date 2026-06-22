# Phase 2 — Bounded Context Map

> DRAFT — REQUIRES HUMAN VALIDATION
> Generated: 2026-06-20
> Input: [00-inventory.md](00-inventory.md), [01-entry-points.md](01-entry-points.md)

---

## Table of Contents

1. [Context 1 — Planning](#context-1--planning)
2. [Context 2 — Project & Resource Structure (PBS)](#context-2--project--resource-structure-pbs)
3. [Context 3 — Timesheet & Work Execution](#context-3--timesheet--work-execution)
4. [Context 4 — HR & Workforce Management](#context-4--hr--workforce-management)
5. [Context 5 — Reporting, KPI & Prediction](#context-5--reporting-kpi--prediction)
6. [Context 6 — Data Integration & Sync](#context-6--data-integration--sync)
7. [Context 7 — Platform & Operations](#context-7--platform--operations)
8. [Cross-Context Relationships](#cross-context-relationships)
9. [Top 10 Boundary Violations](#top-10-boundary-violations)

---

## Context 1 — Planning

**Description:** Manages the construction/engineering project schedule — activities (tasks), their dependencies (relations), assignments of persons to activities, baselines (schedule snapshots for comparison), and the planning board views. This context owns the core scheduling algorithm and the network of activity constraints. It is the central domain of Floor2Plan and the consumer of data from PBS, HR/Scheduling, and Timesheet to drive its capacity-aware scheduling logic.

**Ubiquitous language:**
- Activity — a scheduled unit of work with a start/finish window and progress
- ActivityRelation — a finish-to-start or other dependency between activities
- Assignment — the binding of a person (or discipline) to an activity for a quantity of hours
- Baseline — a frozen snapshot of the schedule for deviation tracking
- ActivityStatus / ActivityPhase — lifecycle state of an activity
- AssignmentProfile — how hours are distributed across the assignment window
- ActivitySummary — rolled-up progress and hours for reporting
- ActivityGroup / ActivityGroupActivity — grouping of activities (hammock / summary bars)
- Window — the earliest/latest dates an activity may start/finish
- Hammock — a summary activity that derives its dates from its children
- Predecessor readiness — gate that blocks an activity until all predecessors complete
- Task readiness — broader gate including material, engineering, predecessor, and design-change checks

**Owns:**
- `Activity`, `ActivitySummary`, `ActivityRelation`, `ActivityType`, `ActivityStatus`, `ActivityPhase`
- `ActivityProgressTask`, `ActivityTicketRelation`, `ActivityProperty`
- `ActivityGroup`, `ActivityGroupActivity`
- `Assignment`, `AssignmentSummary`, `AssignmentOption`, `AssignmentProfile`, `AssignmentProgressHistory`
- `AssignmentDurationPrediction`, `AssignmentShortTermPrediction`, `AssignmentProperty`, `AssignmentHourIncrement`
- `Baseline`, `BaselineActivity`, `BaselineAssignment`, `BaselineComponent`, `BaselineProject`, `BaselineReportScurve`
- `TaskReadinessStatusBase`, `TaskReadinessStatusAssignment`, `TaskReadinessStatusActivity`, `TaskReadinessStatusComponent`
- `TaskReadinessStatusItemGeneric`, `TaskReadinessStatusItemPredecessor`, `TaskReadinessType`, `TaskReadinessStatusType`
- `ManualHoursAndProgress`, `SplitActivityEntry`
- `F2PTask`

**Does not own:**
- `Allocation` — owned by HR/Workforce (determines which organisation a person belongs to for planning)
- `Schedule` / `PersonSchedule` / `ScheduleEntry` — owned by HR/Workforce (work calendar source, read by Planning)
- `Person`, `Organisation`, `Discipline` — owned by PBS (referenced as foreign keys in Activity/Assignment)
- `Component` — owned by PBS (activities are linked to components, but PBS manages components)
- `TicketBase` / `TransportRequest` / `StopWorkOrder` — owned by Tickets (relation exists via `ActivityTicketRelation`)
- `KpiDeviationPlot`, `KpiOverdue`, `KpiOverDuration` — owned by Reporting/KPI

**Entry points (EP-### from Phase 1):**
- EP-010 — Activity CRUD and planning board actions
- EP-011 — ActivityRelation management
- EP-012 — Assignment CRUD
- EP-013 — Holiday management (affects planning capacity)
- EP-015 — Structure management
- EP-016 — Select/lookup endpoints
- EP-048 — Baseline creation and comparison (Pbs area but logically Planning-owned)
- EP-172 — Task readiness status management
- EP-100 to EP-103 — Import planning/XER/Sciforma/hours jobs
- EP-187 to EP-189 — Elsa workflow activities for activity status
- EP-300, EP-301, EP-302, EP-303 — Change handlers: RecordAudit, Activity, Assignment, AssignmentProgress
- EP-400 — RefreshActivitySummaryProcessor
- EP-403 — HierarchyCacheProcessor (activity/structure part)

**Data stores:**
- `Floor2PlanDbContext` — primary; all activity, assignment, baseline, task readiness tables
- `DistributionsDbContext` — `DailyAssignmentDistribution`, `DailyActivityGroupDistribution`, `BaselineAssignmentDistribution`

**External dependencies:**
- PBS — reads `Component`, `Project`, `Person`, `Organisation`, `Discipline`, `Structure` as foreign-key references
- HR/Workforce — reads `Schedule`, `PersonSchedule`, `Allocation`, `PersonHoliday` for capacity calculations
- Timesheet — reads booked hours from `TimesheetEntry` cache for progress tracking (via change handlers)
- Platform — uses `IFireAndForget` for processor dispatch; Elsa for workflow-driven status transitions

**Current code locations:**
- `Src/UI/UI.Floor2Plan/Areas/Plan/Controllers/`
- `Src/Domain/Domain.Service/Planning/` (PlanningService, UpdateTaskHelper)
- `Src/Domain/Domain.Service/ActivityService.cs`, `ActivityRelationService.cs`, `AssignmentService.cs`
- `Src/Domain/Domain.Service/TaskReadiness/`
- `Src/Application/Application.Service/ActivityProvider.cs`, `ActivityRelationProvider.cs`, `AssignmentProvider.cs`, `BaselineProvider.cs`
- `Src/Application/Application.Service/Floorboard/`
- `Src/Data/Data.ChangeHandlers/ActivityChangeHandler.cs`, `AssignmentChangeHandlers/`
- `Src/Processors/Processors.Domain/RefreshActivitySummaryProcessor.cs`, `ApplyActivitySummaryProcessor.cs`, `ApplyAssignmentSummaryProcessor.cs`
- `Src/Processors/Processors.Domain/TaskReadinessProcessors/`

---

## Context 2 — Project & Resource Structure (PBS)

**Description:** Manages the product breakdown structure — the hierarchy of projects, components, component types, structures, and structure types — together with the human and organisational resources that execute work: persons, organisations, disciplines, roles, assets, materials, and floor space / locations. PBS is the master data context: it defines what exists (what components to build, who the workers are) and provides the reference data that Planning and Timesheet depend on. It also manages the weak-object-relation (WOR) system for cross-entity linking and property type management.

**Ubiquitous language:**
- Project — top-level planning container
- Component — a physical or logical shipyard/construction object to be built
- ComponentType — category of component (e.g. block, panel, outfitting)
- Structure / StructureType — organizational hierarchy nodes grouping components
- Organisation — a crew, department, or external company
- Discipline — a skill category (e.g. welding, fitting)
- Person — a worker or resource
- Asset — equipment or tools
- Material — physical materials consumed by activities
- Role — permission group
- WOR (Weak Object Relation) — flexible cross-entity links
- ProjectScenario — what-if planning variant
- PropertyType — extensible property schema for entities

**Owns:**
- `Project`, `ProjectStatus`, `ProjectType`, `ProjectScenario`, `ProjectScenarioActivity`, `ProjectDisciplineTariff`, `ProjectTypeProperty`, `ProjectProperty`
- `Component`, `ComponentType`, `ComponentTypeProperty`, `ComponentProperty`, `ComponentState`, `ComponentStateGroup`, `ComponentStateGroupProperty`, `ComponentStateProperty`
- `Structure`, `StructureRelation`, `StructureType`
- `Organisation`, `OrganisationRole`, `OrganisationProperty`, `OrganisationInRole`, `OrganisationCapacityDashboardLine`
- `Person`, `PersonProperty`, `PersonInTeam`, `PersonTimesheetApprover`, `PersonCapacityDashboardLineFilter`
- `Discipline`, `DisciplineProperty`, `DisciplinePerson`, `DisciplineCapacityDashboardLine`, `DisciplineCapacityDashboardLineDistribution`
- `Role`, `User`, `UserInRole`
- `Asset`, `AssetType`
- `Material`, `MaterialProperty`
- `ProjectMaterial`, `ProjectMaterialUsageHistory`
- `Wor`, `WorType`
- `PropertyType`, `PropertyTypeOption`, `PropertySet`
- `Team`
- `F2PSetting`
- `Location`, `LocationOrganisation`, `LocationProperty`, `LocationType`
- `UnitType`

**Does not own:**
- `Activity`, `Assignment` — owned by Planning (components are referenced, not owned, within planning context)
- `TimesheetEntry`, `Balance` — owned by Timesheet/HR respectively
- `OrganisationHoliday`, `PersonHoliday` — owned by HR/Workforce (calendar data)
- `Schedule`, `PersonSchedule` — owned by HR/Workforce (work calendar)
- `Allocation` — owned by HR/Workforce (determines person's organisational unit for a time period)

**Entry points (EP-### from Phase 1):**
- EP-030 to EP-054 — All PBS area MVC controllers (Component, ComponentType, Project, ProjectType, ProjectScenario, Organisation, Person, Discipline, Role, User, Schedule, Asset, Baseline, Copy, Planning, Wor, Settings, Shipyard)
- EP-015 — StructureController
- EP-047 — AssetController
- EP-145, EP-146 — Material, ProjectMaterial controllers
- EP-150, EP-151 — FloorSpace, Location controllers
- EP-018 — GenericPropertyController
- EP-049 — CopyController (PBS copy operations)
- EP-310, EP-311 — ComponentChangeHandler, OrganisationChangeHandler
- EP-106, EP-107, EP-108, EP-109, EP-114 to EP-120 — Import jobs for Organisation, Discipline, Material, Persons, etc.

**Data stores:**
- `Floor2PlanDbContext` — primary; all PBS entities
- `FileDbContext` — `ComponentFile`, `ProjectFile`, `AssetFile`, `MaterialFile`, `OrganisationFile`, `ComponentUrl`, `ProjectUrl`
- `DocumentStoreDbContext` — `DsFileContent` (file binary storage)
- `DistributionsDbContext` — `OrganisationCapacityDashboardLineDistribution` (read, written by Processors)

**External dependencies:**
- Platform — Authentication/authorisation for user and role management; `IFireAndForget` for cache processors
- Data Integration — writes to PBS entities during import jobs

**Current code locations:**
- `Src/UI/UI.Floor2Plan/Areas/Pbs/Controllers/`
- `Src/UI/UI.Floor2Plan/Areas/Material/Controllers/`
- `Src/UI/UI.Floor2Plan/Areas/FloorSpace/Controllers/`
- `Src/Domain/Domain.Service/ComponentService.cs`, `ProjectService.cs`, `OrganisationService.cs`, `PersonService.cs`, `MaterialService.cs`, `AssetService.cs`, `StructureService.cs`
- `Src/Domain/Domain.Service/ProductBreakdown/`
- `Src/Domain/Domain.Service/Pbs/`
- `Src/Application/Application.Service/ComponentProvider.cs`, `ProjectProvider.cs`, `OrganisationProvider.cs`, `PersonProvider.cs`, `MaterialProvider.cs`
- `Src/Data/Data.ChangeHandlers/ComponentChangeHandler.cs`, `ComponentStructureChangeHandler.cs`, `ComponentTypeChangeHandler.cs`, `OrganisationChangeHandler.cs`, `ProjectChangeHandler.cs`
- `Src/Processors/Processors.Domain/ComponentStructureProcessors/`

---

## Context 3 — Timesheet & Work Execution

**Description:** Manages the capture and approval of actual work hours — employee timesheets, correction timesheets, ERP hour integration, split activity entries, and the shop floor / clocking terminal interfaces. This context records what actually happened on the shop floor: who booked hours on which activity, when, and how those hours flow to ERP systems. It is closely coupled to Planning (provides actuals that drive progress) and HR/Workforce (supplies schedule data and balance updates).

**Ubiquitous language:**
- TimeSheet — a weekly container for a person's booked hours
- TimesheetEntry — a single hour-booking line on a timesheet
- TimesheetStatus — approval state (Open, Submitted, Approved, Rejected)
- CorrectionTimesheet — a correction entry for previously submitted timesheets
- ErpHourline / Hourline — lines transferred to or from an ERP system
- HourType — category of work (e.g. normal, overtime, leave)
- SplitActivityEntry — subdivision of an activity's hours across sub-tasks
- ClockingTerminal / ClockingTerminalPunch — physical device recording clock-in/out
- ShopFloorTerminal — shop floor display / interaction device
- AccessControlTime — access control timestamps from physical gates
- ManualHoursAndProgress — direct entry of hours/progress outside timesheet flow
- ErpCorrectionTimesheet — corrections visible to ERP systems

**Owns:**
- `TimeSheet`, `TimesheetEntry`, `TimeSheetRemark`, `TimeSheetStatus`, `TimeSheetWeeklyHourline`
- `TimesheetEventMessage`, `MilestoneEventMessage`, `NewsFeedMessage`
- `CorrectionTimesheet`, `CorrectionTimesheetWeeklyCorrectionLine`
- `ErpHourline`, `Hourline`, `HourlyRate`, `HourType`
- `SplitActivityEntry`
- `ClockingTerminal`, `ClockingTerminalPunch`, `ClockingTerminalProfile`, `ClockingTerminalProfileClockingTerminal`, `ClockingTerminalSetting`
- `ClockType`, `RfidSetting`
- `ShopFloorTerminal`, `ShopFloorTerminalSetting`
- `AccessControlTime`, `AccessControlTimeClockingTerminalPunch`
- `PersonalDailyClockTimes`

**Does not own:**
- `Assignment` — owned by Planning; timesheet entries reference assignments but Planning owns the relationship
- `Person`, `Organisation` — owned by PBS; referenced for timesheet lookup
- `Balance`, `BalancePolicy` — owned by HR/Workforce (timesheet changes trigger balance updates via change handler)
- `Allocation` — owned by HR/Workforce

**Entry points (EP-### from Phase 1):**
- EP-060 to EP-065 — Do area controllers (EmployeeTimesheet, WeeklyTimesheet, CorrectionTimesheet, Planboard, Floorboard, Reporter)
- EP-066 — REST: external timesheet status update (ERP callback)
- EP-067, EP-068 — EventController, NewsFeedController
- EP-077, EP-078 — HR/ClockingTerminal, ClockingTerminalProfile (clocking device config — spans HR and Devices)
- EP-085, EP-086 — Check area ERP timesheet controllers
- EP-140, EP-141 — Devices area controllers (ShopFloorTerminal, ClockingTerminal)
- EP-305, EP-306 — TimesheetEntry change handlers (hours cache, balance update trigger)
- EP-307 — ScheduleChangeHandler (triggers timesheet cache refresh)

**Data stores:**
- `Floor2PlanDbContext` — all timesheet and clocking terminal entities

**External dependencies:**
- Planning — reads `Activity`, `Assignment` for timesheet entry; sends progress updates via change handlers
- HR/Workforce — triggers balance recalculation via `TimesheetEntryUpdateBalancesChangeHandler`; reads `Schedule`, `Allocation`
- Data Integration — ERP integration for `ErpHourline` export and `TimesheetController.Status` callback
- Platform — `IFireAndForget` for `RefreshTimesheetEntriesProcessor`, `CreateErpHourlinesProcessor`

**Current code locations:**
- `Src/UI/UI.Floor2Plan/Areas/Do/Controllers/`
- `Src/UI/UI.Floor2Plan/Areas/Devices/Controllers/`
- `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/ClockingTerminal*.cs`
- `Src/Domain/Domain.Service/Timesheets/`
- `Src/Domain/Domain.Service/Clocking/`
- `Src/Domain/Domain.Service/ShopFloor/`
- `Src/Application/Application.Service/EmployeeTimesheet/`
- `Src/Application/Application.Service/Clocking/`
- `Src/Application/Application.Service/ShopFloor/`
- `Src/Data/Data.ChangeHandlers/TimesheetEntryChangeHandler.cs`, `TimesheetEntryUpdateBalancesChangeHandler.cs`, `TimesheetStatusChangeHandler.cs`, `TimesheetWeeklyHourlineChangeHandler.cs`
- `Src/Processors/Processors.Domain/TimesheetEntry/`, `EntryTimes/`, `HoursProcessors/`

---

## Context 4 — HR & Workforce Management

**Description:** Manages the people-side of workforce capacity: work schedules (when people work), balance policies (how leave and overtime accrue), personal balance accounts, off-time requests, holiday calendars, and the allocation of persons to organisations for a given time period. This context is the authoritative source of capacity for Planning and Timesheet — it answers "how many hours is this person available this week?" and "what is their current balance?".

**Ubiquitous language:**
- Schedule / ScheduleEntry / ScheduleEntryPeriod — the template defining working hours per week pattern
- PersonSchedule — a person's currently assigned schedule
- OrganisationSchedule — default schedule for an organisation
- OvertimeEntry — explicitly recorded overtime hours
- CachePersonalSchedule — pre-computed daily hours from a schedule (performance cache)
- Allocation — the period during which a person is assigned to an organisation
- AllocationHourDistribution — daily distribution of allocation hours
- PersonHoliday / OrganisationHoliday — specific dates off
- BalancePolicy / BalancePolicyRule / BalanceAccumulationRule — rules governing how balance types accrue
- Balance / BalanceMutationHistory — running account of a person's leave/overtime balance
- BalanceMutationType — category of balance mutation (accrual, deduction, correction)
- PersonBalancePolicy — the balance policy assigned to a person
- OffTime — a formal off-time request consuming balance
- ScheduleRoundingRuleSet — rounding rules applied when computing schedule hours

**Owns:**
- `Schedule`, `ScheduleEntry`, `ScheduleEntryPeriod`, `ScheduleRoundingRuleSet`
- `PersonSchedule`, `PersonScheduleEntry`
- `OrganisationSchedule`, `OrganisationScheduleEntry`
- `OvertimeEntry`
- `CachePersonalSchedule`, `CachePersonalScheduleOvertime`, `CachePersonalSchedulePeriod`
- `Allocation`, `AllocationHourDistribution`
- `PersonHoliday`, `PersonHolidayStatus`, `PersonHolidayInTimesheetHours`
- `OrganisationHoliday`, `OrganisationOffTimeInTimesheetHours`
- `Balance`, `BalanceMutationHistory`, `BalanceMutationType`
- `BalancePolicy`, `BalancePolicyRule`, `BalancePolicyInBalancePolicyRule`, `BalanceAccumulationRule`
- `PersonBalancePolicy`
- `DayWeekYear`

**Does not own:**
- `Person`, `Organisation` — owned by PBS; HR reads them as master data
- `TimesheetEntry` — owned by Timesheet; HR reads hours for balance computation
- `Assignment` — owned by Planning; HR reads assignment hours for capacity
- `ClockingTerminal`, `ShopFloorTerminal` — owned by Timesheet (device management)

**Entry points (EP-### from Phase 1):**
- EP-013 — HolidayController (Plan area)
- EP-014 — AllocationController
- EP-044 — Pbs/ScheduleController
- EP-070 to EP-076 — HR area controllers (Balance, BalancePolicy, BalancePolicyRule, BalanceAccumulationRule, PersonBalance, OffTime, ScheduleManagement)
- EP-304 — AllocationChangeHandler (manages person organisation assignment)
- EP-306 — TimesheetEntryUpdateBalancesChangeHandler (balance update trigger from Timesheet)
- EP-307, EP-308 — ScheduleChangeHandler, BalanceChangeHandler
- EP-410 — UpdateBalanceProcessor

**Data stores:**
- `Floor2PlanDbContext` — all schedule, allocation, balance, and holiday entities

**External dependencies:**
- PBS — reads `Person`, `Organisation` as master references
- Timesheet — `TimesheetEntry` mutations trigger balance recalculation via change handler
- Planning — `Allocation` changes affect person availability in the planning board
- Platform — `IFireAndForget` for `UpdateBalanceProcessor`, `RefreshCachePersonalSchedulesProcessor`

**Current code locations:**
- `Src/UI/UI.Floor2Plan/Areas/HR/Controllers/`
- `Src/UI/UI.Floor2Plan/Controllers/AllocationController.cs`
- `Src/Domain/Domain.Service/Balances/`
- `Src/Domain/Domain.Service/AllocationService.cs`, `OrganisationHolidayService.cs`, `PersonBalancesCalculatorService.cs`, `AbsenceApprovalService.cs`
- `Src/Domain/Domain.Service/Schedules/`
- `Src/Application/Application.Service/Balances/`
- `Src/Application/Application.Service/Hr/`
- `Src/Application/Application.Service/OrganisationScheduleProvider.cs`, `PersonScheduleProvider.cs`, `HolidayPlanningProvider.cs`
- `Src/Data/Data.ChangeHandlers/AllocationChangeHandler.cs`, `BalanceChangeHandler.cs`, `ScheduleChangeHandler.cs`
- `Src/Data/Data.ChangeHandlers/PersonBalancePolicyChangeHandler.cs`, `PersonHolidayChangeHandler.cs`
- `Src/Processors/Processors.Domain/BalanceProcessors/`

---

## Context 5 — Reporting, KPI & Prediction

**Description:** Provides analytical, read-heavy views over the project data: Telerik/NPOI-based reports, KPI dashboards (SPI, CPI, schedule performance indexes), S-curves, long-term and short-term capacity predictions, resource planning graphs, and capacity dashboard lines. This context consumes aggregated and pre-computed data from other contexts (primarily Planning and PBS) and produces derived analytical artefacts. It does not own primary operational entities; it reads and aggregates them.

**Ubiquitous language:**
- KPI (Key Performance Index) — a schedule or cost performance metric
- SPI (Schedule Performance Index) — ratio of earned value to planned value
- CPI (Cost Performance Index) — ratio of earned value to actual cost
- S-curve — a cumulative progress or cost curve over time
- Prediction — a forward projection of capacity requirements (long-term or short-term)
- ReportingResource — a stored Telerik report template file
- ReportingFolder — folder grouping for report templates
- CapacityDashboardLine — an aggregate capacity metric line (per organisation or discipline)
- DailyPersonDistribution / DailyAssignmentDistribution — pre-computed daily hour distributions
- BaselineAssignmentDistribution — baseline distribution for deviation analysis
- KpiDeviationPlot, KpiOverdue, KpiOverDuration — KPI data shapes

**Owns:**
- `KpiDeviationPlot`, `KpiOverdue`, `KpiOverDuration` (transient/computed — not persisted as entities)
- `ReportingResource`, `ReportingFolder`, `ReportingResourcePageSetting` (in `ReportingDbContext`)
- `CapacityDashboardLine`, `OrganisationCapacityDashboardLine`, `DisciplineCapacityDashboardLine`
- `DailyPersonDistribution`, `DailyAssignmentDistribution`, `DailyActivityGroupDistribution`, `DateDistribution`, `BaselineAssignmentDistribution` (in `DistributionsDbContext`)
- `ProgressPerWeekDto`, `ReportSCurveGridModel` (domain model DTOs)

**Does not own:**
- Operational entities (`Activity`, `Assignment`, `TimesheetEntry`, etc.) — only reads them for aggregation
- `ReportingResourcePageSetting` is in `Floor2PlanDbContext` (NEEDS REVIEW — verify this is truly in `ReportingDbContext` only)

**Entry points (EP-### from Phase 1):**
- EP-080 to EP-086 — Check area controllers (Reports, ReportsApi, ReportDesigner, KPI, KpiSpiCpi, ErpCorrectionTimesheet, ErpWeeklyTimesheet)
- EP-155 — Prediction controller
- EP-314, EP-315, EP-316 — CapacityChangeHandler, LongTermPredictionChangeHandler, ShortTermPredictionChangeHandler
- EP-400 to EP-405 — RefreshActivitySummary, distribution, and prediction processors
- EP-451 — RefreshAllDistributionsSystemAction

**Data stores:**
- `ReportingDbContext` — `ReportingResource`, `ReportingFolder`, `ReportingResourcePageSetting`
- `DistributionsDbContext` — all distribution tables
- `ODataDbContext` — read-only OData projection for all 131 entity sets

**External dependencies:**
- Planning — reads `Activity`, `Assignment`, `ActivitySummary`, `Baseline` data for all KPIs and predictions
- PBS — reads `Component`, `Project`, `Organisation`, `Discipline` for capacity dashboard grouping
- HR/Workforce — reads `Schedule`, `Allocation` for capacity computation
- Timesheet — reads `TimesheetEntry` for actual vs. planned analysis
- Platform — `IFireAndForget` for distribution and prediction processors

**Current code locations:**
- `Src/UI/UI.Floor2Plan/Areas/Check/Controllers/`
- `Src/UI/UI.Floor2Plan/Areas/Prediction/`
- `Src/Domain/Domain.Service/Kpi/`
- `Src/Domain/Domain.Service/Distributions/`
- `Src/Domain/Domain.Service/LongTermPredictionService.cs`, `ShortTermPredictionService.cs`, `PredictionScoreHelper.cs`
- `Src/Application/Application.Service/Kpi/`
- `Src/Application/Application.Service/Predictions/`
- `Src/Application/Application.Service/Reporting/`
- `Src/Application/Application.Service/Reports/`
- `Src/Data/Data.Reporting/`, `Src/Data/Data.Reporting.EntityFramework/`
- `Src/Distributions/`
- `Src/Processors/Processors.Domain/PredictionProcessors/`
- `Src/Processors/Processors.Domain/RefreshDailyPersonDistributionProcessor.cs`, `RefreshDailyAssignmentDistributionProcessor.cs`, `RefreshDailyActivityGroupDistributionsProcessor.cs`

---

## Context 6 — Data Integration & Sync

**Description:** Manages the ingestion of external data into Floor2Plan and the push of Floor2Plan data to external systems. Import formats include Primavera P6 XER, MS Project (via Aspose), Sciforma XML, Excel/CSV, and various ERP connector protocols. Export/sync includes planning connector updates, ERP hour-line transfers, and access control sync. This context provides the translation layer between external systems and Floor2Plan's internal domain model. It owns the sync configuration, sync logs, import config, and connector management.

**Ubiquitous language:**
- ImportConfig / ImportConfigRule — persisted import configuration defining how fields map
- SyncLog / SyncHistory — audit trail of a sync or import run
- Connector — an external system endpoint (ERP, PDM, planning tool)
- ConnectorConfiguration — runtime settings for a connector
- ImportJob — a Hangfire job that executes a single import run
- ImportProvider — a strategy class for parsing a specific file format
- SyncScope — defines which entities are in scope for a given sync
- XER — Oracle Primavera P6 file format
- ProductBreakdown — imported product structure (Components/Projects from external source)
- TaskReadiness (import side) — importing engineering/design readiness status
- SyncLog mail — automated email sent on sync completion

**Owns:**
- `ImportConfig`, `ImportConfigRule`
- `SyncLog`, `SyncHistory`
- `LoginRequest` (external system authentication tokens)
- `UnitType` (shared reference data imported from external systems — NEEDS REVIEW: may belong to PBS)
- All connector configuration entities (stored in `Floor2PlanDbContext` under various connector tables)

**Does not own:**
- All domain entities being imported (`Activity`, `Person`, `Organisation`, `Material`, etc.) — they are owned by Planning and PBS; Sync writes to them as a writer-agent, not an owner
- `ProcessLog`, `ProcessLogMessage` — owned by Platform (process logging infrastructure)

**Entry points (EP-### from Phase 1):**
- EP-090 to EP-097 — Sync area MVC controllers (ImportExport, ConnectorConfiguration, Pdm, PlanningHours, ProductModelLink, TransferHours, ConvertPlanningFile)
- EP-098, EP-099 — REST import API (ProductBreakdown, TaskAllocations)
- EP-100 to EP-121 — All Hangfire import and sync jobs (21 jobs)
- EP-185, EP-186 — Elsa workflow: RunConnectorActivity, SyncLogSendMailActivity
- EP-312 — ActivityConnectorChangeHandler (syncs activity changes to external connector)
- EP-406 to EP-409 — Sync processors (SyncPlanningProcessor, SyncErpProcessor, SyncEntryTimeProcessor, SyncGenericProcessor)

**Data stores:**
- `Floor2PlanDbContext` — `ImportConfig`, `ImportConfigRule`, `SyncLog`, `SyncHistory`
- `ProcessDbContext` — `ProcessLog`, `ProcessLogMessage` (via Platform infrastructure)

**External dependencies:**
- Planning — writes `Activity`, `Assignment`, `ActivityRelation` data on planning imports
- PBS — writes `Component`, `Person`, `Organisation`, `Discipline`, `Material` on master data imports
- HR/Workforce — writes `TimesheetEntry`, `Allocation` on timesheet and hours imports
- Platform — uses `IFireAndForget` (sync queue), `Infrastructure.Webservice` (WCF/SOAP connectors), `Infrastructure.Mail` (sync log mails), `ProcessDbContext` (process logging)

**Current code locations:**
- `Src/UI/UI.Floor2Plan/Areas/Sync/Controllers/`
- `Src/Application/Application.Sync/` (full project — all import jobs, providers, services)
- `Src/Application/Application.Service/Sync/`
- `Src/Domain/Domain.Service/Sync/`
- `Src/Domain/Domain.Service/ProductBreakdown/`
- `Src/Processors/Processors.Domain/SyncProcessors/`
- `Src/Processors/Processors.Domain/EntryTimes/`

---

## Context 7 — Platform & Operations

**Description:** The horizontal infrastructure context that all other contexts depend on. It encompasses identity and authorisation (permission grants, JWT, Azure AD, local login), application configuration (runtime app settings, feature flags), background job infrastructure (Hangfire, fire-and-forget), workflow orchestration (Elsa engine), audit logging, health monitoring, file storage, process logging, and system administration. Platform provides the enabling capabilities: it does not own business domain entities but it owns the operational and cross-cutting entities that run the system. The Ticket/work-order sub-domain also lives here because it is more of an operational coordination tool than a core planning domain.

**Ubiquitous language:**
- PermissionGrant — a record granting a role or user a specific permission
- AppSetting — a runtime-configurable application setting stored in the database
- AuditLog — an immutable record of who changed what and when
- HealthCheckLog / HealthCheckReport — results of periodic health checks
- ProcessLog / ProcessLogMessage — execution log for long-running background operations
- SystemAction — an admin-triggered maintenance operation
- Workflow (Elsa) — a declarative, durable automation pipeline
- FireAndForget — single-shot Hangfire job for background work
- TicketBase — general-purpose issue/request raised during project execution
- TransportRequest — a request to move materials or components
- StopWorkOrder — an order to halt work on an activity
- PageSetting — per-user UI column visibility and filter settings
- NewsFeedMessage, MilestoneEventMessage — operational notifications

**Owns:**
- `PermissionGrant` (in `AuthorisationDbContext`)
- `AppSetting` (in `ConfigurationDbContext`)
- `AuditLog`, `AuditLogExcelFile` (in `AuditDbContext`)
- `HealthCheckLog`, `HealthCheckReport` (in `HealthDbContext`)
- `ProcessLog`, `ProcessLogMessage` (in `ProcessDbContext`)
- `DsFileContent` (in `DocumentStoreDbContext`)
- `Floor2PlanBaseFileInfo`, `Floor2PlanFileInfo`, `Floor2PlanFileCategory`, `Floor2PlanFileThumbnail`, `Floor2PlanFileSource` (in `FileDbContext`)
- `TicketBase`, `TransportRequest`, `StopWorkOrder`, `TicketRemark`, `TicketProperty` (in `Floor2PlanDbContext`)
- `PageSetting`, `ImportConfig` (for page/UI config — `PageSetting` in `Floor2PlanDbContext`)
- `LoginRequest`
- Elsa workflow persistence (Elsa `RuntimeElsaMigrationDbContext`, `ManagementElsaMigrationDbContext`)
- Hangfire schema (`HangfireDbContext`)

**Does not own:**
- Domain entities of other contexts — Platform provides infrastructure, not domain logic

**Entry points (EP-### from Phase 1):**
- EP-001 to EP-006 — Auth/Security (JWT login, cookie login, Elsa auth gateway, permission interceptor, API key, Azure AD DB interceptor)
- EP-130 to EP-133 — Ticket area controllers (TicketBase, TransportRequest, Issue, StopWorkOrder)
- EP-160 to EP-179 — System area and root management controllers (Administration, SystemAction, AppSetting, License, Table, PermissionManagement, PersonManagement, PersonInfo, GeneralSettings, PageSettings, Team, TaskReadiness, File)
- EP-180 to EP-196 — Elsa workflow entry points (REST API, SignalR hub, recurring jobs, all workflow activities)
- EP-200 to EP-204 — Health check and Hangfire dashboard endpoints
- EP-210, EP-211 — FireAndForget infrastructure
- EP-309, EP-317 — TicketChangeHandler, PermissionGrantChangeHandler
- EP-412 — SqlDefragmentationProcessor
- EP-450 to EP-457 — System actions

**Data stores:**
- `AuthorisationDbContext` — `PermissionGrant`
- `AuditDbContext` — `AuditLog`, `AuditLogExcelFile`
- `HealthDbContext` — `HealthCheckLog`, `HealthCheckReport`
- `ProcessDbContext` — `ProcessLog`, `ProcessLogMessage`
- `ConfigurationDbContext` — `AppSetting`
- `FileDbContext` — all file/attachment entities
- `DocumentStoreDbContext` — `DsFileContent`
- `HangfireDbContext` — Hangfire schema
- `RuntimeElsaMigrationDbContext`, `ManagementElsaMigrationDbContext` — Elsa persistence
- `Floor2PlanDbContext` — `TicketBase`, `TransportRequest`, `StopWorkOrder`, `TicketRemark`, `TicketProperty`, `PageSetting`, `NewsFeedMessage`, `MilestoneEventMessage`

**External dependencies:**
- All other contexts depend on Platform for: authorisation (`[PermissionAuthorise]`), fire-and-forget job dispatch (`IFireAndForget`), file storage (`IFileService`), process logging, runtime settings (`IAppSettings`), email sending, workflow execution (Elsa)
- Platform has no runtime dependency on domain contexts (intentional — it is a pure infrastructure layer)

**Current code locations:**
- `Src/Infrastructure/Infrastructure.Authorisation/`
- `Src/Infrastructure/Infrastructure.Audit/`
- `Src/Infrastructure/Infrastructure.Configuration.Runtime/`
- `Src/Infrastructure/Infrastructure.Configuration.Startup/`
- `Src/Infrastructure/Infrastructure.Elsa/`, `Infrastructure.Elsa.EntityFramework/`
- `Src/Infrastructure/Infrastructure.Files/`, `Infrastructure.Files.EntityFramework/`
- `Src/Infrastructure/Infrastructure.FireAndForget/`
- `Src/Infrastructure/Infrastructure.Hangfire/`
- `Src/Infrastructure/Infrastructure.Health/`
- `Src/Infrastructure/Infrastructure.Logging/`
- `Src/Infrastructure/Infrastructure.Mail/`
- `Src/Infrastructure/Infrastructure.Process/`
- `Src/Infrastructure/Infrastructure.Workflow/` (Elsa activities)
- `Src/Application/Application.SystemActions/`
- `Src/Application/Application.Service/Ticket/`
- `Src/Domain/Domain.Service/StopWorkOrderService.cs`
- `Src/UI/UI.Floor2Plan/Areas/Ticket/`
- `Src/UI/UI.Floor2Plan/Areas/System/`
- `Src/Connectors/DocumentStore/`

---

## Cross-Context Relationships

| Context A | Direction | Context B | Relationship type | Description |
|-----------|-----------|-----------|-------------------|-------------|
| Planning | reads | PBS | In-process direct DB | Reads `Component`, `Project`, `Person`, `Organisation`, `Discipline`, `Structure` as FK references |
| Planning | reads | HR/Workforce | In-process direct DB | Reads `Schedule`, `PersonSchedule`, `Allocation`, `PersonHoliday` for capacity calculations |
| Planning | reads | Timesheet | Change handler (async) | `TimesheetEntry` mutations trigger `TimesheetEntryChangeHandler` which refreshes planning caches |
| Planning | calls | Platform | Direct service call | `IFireAndForget` for processors; Elsa workflow activities modify activity status |
| PBS | calls | Platform | Direct service call | File storage for component/project attachments; `IFireAndForget` for cache processors |
| Timesheet | reads | Planning | In-process direct DB | Reads `Activity`, `Assignment` to validate timesheet entries; progress linked to assignments |
| Timesheet | triggers | HR/Workforce | Change handler (async) | `TimesheetEntryUpdateBalancesChangeHandler` triggers balance recalculation |
| Timesheet | calls | Platform | Direct service call | `IFireAndForget` for ERP hourline and timesheet cache processors |
| HR/Workforce | reads | PBS | In-process direct DB | Reads `Person`, `Organisation` as master references for schedule/balance assignment |
| HR/Workforce | triggers | Planning | Change handler (async) | `AllocationChangeHandler` updates person organisation IDs used in planning |
| HR/Workforce | calls | Platform | Direct service call | `IFireAndForget` for balance update processors |
| Reporting/KPI | reads | Planning | In-process direct DB | Reads `Activity`, `Assignment`, `ActivitySummary`, `Baseline`, distribution tables |
| Reporting/KPI | reads | PBS | In-process direct DB | Reads `Component`, `Project`, `Organisation`, `Discipline` for KPI grouping |
| Reporting/KPI | reads | HR/Workforce | In-process direct DB | Reads `Schedule`, `Allocation` for capacity computations |
| Reporting/KPI | reads | Timesheet | In-process direct DB | Reads `TimesheetEntry` for actual vs planned analysis |
| Reporting/KPI | calls | Platform | Direct service call | `IFireAndForget` for distribution and prediction processors; file storage for report templates |
| Data Integration | writes | Planning | In-process direct DB (import) | Import jobs write `Activity`, `Assignment`, `ActivityRelation` data |
| Data Integration | writes | PBS | In-process direct DB (import) | Import jobs write `Component`, `Person`, `Organisation`, `Discipline`, `Material` |
| Data Integration | writes | HR/Workforce | In-process direct DB (import) | Import jobs write `TimesheetEntry`, `Allocation` data |
| Data Integration | calls | Platform | Direct service call | `IFireAndForget` (sync queue), `ProcessDbContext` process logging, mail sending |
| Data Integration | triggers | Planning | Change handler (async) | `ActivityConnectorChangeHandler` syncs activity changes back to external connector |
| Platform | enables | All contexts | Dependency provision | Auth, fire-and-forget, file storage, mail, health, configuration, audit — consumed by all |
| Platform | calls | Planning | Workflow activity (Elsa) | `SetActivityStatusActivity`, `GetActivityByIdActivity` modify planning activity state |
| Platform | calls | Timesheet | Workflow activity (Elsa) | `TicketStatusUpdateActivity` may update transport request linked to activities |

---

## Top 10 Boundary Violations

The following violations are the most significant architectural coupling problems that would need resolution before independent deployment of any bounded context.

### 1. God DbContext — `Floor2PlanDbContext` (~150 DbSets, all contexts)

**Severity: Critical**  
`Floor2PlanDbContext` contains entities from all seven bounded contexts in a single EF context. Any SaveChanges call from any context fires all 63 change handlers, which can mutate entities belonging to other contexts. There is no context isolation. This is the foundational violation that all others follow from.

**Resolution path:** Introduce per-context DbContexts with explicit FK crossing only through read-only projections or integration events.

### 2. Change handler pipeline acts as implicit integration bus (all contexts)

**Severity: Critical**  
63 `IEntityChangeHandler` implementations fire synchronously before/after `SaveChangesAsync`. A single save of a `TimesheetEntry` can trigger `TimesheetEntryChangeHandler` (Timesheet), `TimesheetEntryUpdateBalancesChangeHandler` (HR/Workforce boundary), `ScheduleChangeHandler` (HR/Workforce), and `CapacityChangeHandler` (Reporting). This creates tight transactional coupling across context boundaries with no contract.

**Resolution path:** Replace cross-context change handlers with integration events (published after commit) consumed by the owning context.

### 3. Processor dispatch via `IFireAndForget` carries no context isolation

**Severity: High**  
All 65 processors in `Processors.Domain` extend `DataProcessorBase<Floor2PlanDbContext, …>` — they all share the god context. `RefreshDailyAssignmentDistributionProcessor` writes to `DistributionsDbContext` while reading from `Floor2PlanDbContext` via the same unit of work, coupling Reporting/KPI to Planning.

**Resolution path:** Split processors by owning context and inject only the context they need.

### 4. Direct PBS reads inside Planning domain services

**Severity: High**  
`ActivityService`, `AssignmentService`, and `PlanningService` in `Domain.Service` directly query `Component`, `Project`, `Person`, `Organisation` from `Floor2PlanDbContext`. In a modular world, Planning should receive these via read models or contract interfaces, not shared table access.

**Resolution path:** Introduce read-model projections or an anti-corruption layer; Planning asks PBS "does component X exist?" via a contract rather than a direct EF query.

### 5. Balance update triggered from Timesheet via change handler crossing HR boundary

**Severity: High**  
`TimesheetEntryUpdateBalancesChangeHandler` (in the Timesheet domain area) writes to `Balance` and `BalanceMutationHistory` (HR/Workforce boundary) within the same transaction. This couples two contexts transactionally with no explicit contract.

**Resolution path:** Emit a `TimesheetEntryApproved` integration event; HR/Workforce subscribes and runs balance update in its own transaction.

### 6. `AllocationHourDistribution` and capacity distribution processors span HR, Planning, and Reporting

**Severity: High**  
`RefreshOrganisationCapacityDashboardLineDistributionProcessor` and `RefreshDisciplineCapacityDashboardLineDistributionProcessor` read from `Allocation` (HR/Workforce), `Assignment` (Planning), and `Schedule` (HR/Workforce) to write to `OrganisationCapacityDashboardLine` (PBS/Reporting). This processor alone spans three context boundaries.

**Resolution path:** Define a `CapacityProjection` aggregate owned by Reporting that is rebuilt from integration events from HR/Workforce and Planning.

### 7. Sync import jobs write directly to master data owned by PBS and Planning

**Severity: High**  
Import jobs in `Application.Sync` write to `Component`, `Project`, `Person`, `Organisation`, `Material`, `Activity`, `Assignment` — all owned by PBS or Planning — via direct `Floor2PlanDbContext` access. This means Data Integration has write access to every other context's aggregate roots.

**Resolution path:** Import jobs should call defined application services or command handlers from the owning context, not bypass them with direct DB writes.

### 8. `ActivityConnectorChangeHandler` creates a synchronous dependency from Planning to Data Integration

**Severity: Medium**  
`ActivityConnectorChangeHandler` (in `Data.ChangeHandlers`) fires whenever an `Activity` is saved and enqueues a sync processor via `IFireAndForget`. This means every Planning write potentially triggers a Data Integration side-effect inside the same logical transaction scope.

**Resolution path:** Decouple with an integration event (`ActivityUpdated`) published after Planning's transaction commits; Data Integration subscribes independently.

### 9. `Floor2PlanDbContext` contains `PermissionGrant`-related change handler (`PermissionGrantChangeHandler`) that crosses into Platform

**Severity: Medium**  
`PermissionGrantChangeHandler` watches `PermissionGrant` mutations (Platform boundary) inside `Data.ChangeHandlers` and clears role caches that affect `User` and `Role` (PBS boundary). Platform infrastructure is intercepted in the same change handler pipeline as domain entities.

**Resolution path:** Move permission change reactions to `Infrastructure.Authorisation` using its own context; remove the change handler from `Data.ChangeHandlers`.

### 10. Ticket context is embedded in Platform but triggers planning-side workflow activities

**Severity: Medium**  
Tickets (`TicketBase`, `TransportRequest`, `StopWorkOrder`) are stored in `Floor2PlanDbContext` alongside planning entities. The Elsa workflow activities `TicketActivityCreateActivity` and `TicketActivityCloseActivity` create/close `Activity` records (Planning context) from a ticket event (Platform/Ticket context), creating a bidirectional dependency at the workflow-activity level.

**Resolution path:** If Tickets remain in Platform, expose a Planning command (`CreateMitigatingActivity`) as an explicit contract; the workflow calls the command rather than directly accessing Planning's data.
