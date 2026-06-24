# Floor2Plan.Core — Bounded Context Map (Phase 2)

> **DRAFT — REQUIRES HUMAN VALIDATION**
> Do not extract use cases until this document is reviewed and signed off.

**Generated:** 2026-06-24
**Inputs:** 00-inventory.md, 01-entry-points.md
**Proposed context count:** 7

---

## Context Overview

| # | Name | Short label | Primary aggregates |
|---|------|-------------|-------------------|
| 1 | Planning & Resource Management | PBS | Project, Activity, Component, Assignment, Baseline |
| 2 | Workforce & Organization | HR/Workforce | Person, Organisation, Discipline, Team, Schedule |
| 3 | Timekeeping & Work Hours | Timesheets | TimeSheet, ErpHourline, Balance, BalancePolicy |
| 4 | Devices & Shop Floor Clocking | Clocking/Devices | ClockingTerminal, ShopFloorTerminal, AccessControlTime |
| 5 | Workflow & Ticketing | Tickets | Ticket, TransportRequest, StopWorkOrder |
| 6 | Synchronization & Import Pipeline | Sync | SyncLog, ImportConfig |
| 7 | System Administration & Infrastructure | System | AppSetting, License, SystemAction, User, Role |

---

## Context 1 — Planning & Resource Management (PBS)

**Description:** Core project planning capability — project/activity hierarchies, assignments, resource allocation, baseline versioning, task readiness, and predictions.

**Ubiquitous language:**
Project · Activity · Component · Assignment · Baseline · EarlyStart/EarlyFinish · LateStart/LateFinish · ObjectiveDate · AssignmentProfile · ActivityPhase · ActivityProgress · ActivityStatus · PlanningMode · ProjectScenario · ProjectMaterial · ManualHoursAndProgress · TaskReadiness · KPI

**Owns:**
- Project (root), Activity (root), Component (root), Assignment (root), Baseline (root), ProjectScenario (root)
- ProjectProperty, ProjectType, ProjectStatus, ActivityRelation, ActivityProperty, ActivityStatus, ActivityType, ActivityPhase, ComponentType, ComponentProperty, ComponentTypeProperty, ComponentState, ComponentStateGroup, Material, ProjectMaterial, ProjectMaterialUsageHistory, ManualHoursAndProgress, ActivityGroup, ActivityGroupActivity, ActivitySummary, AssignmentSummary, BaselineActivity, BaselineAssignment, BaselineComponent, BaselineProject, ActivityProgressTask, ProjectScenarioActivity
- TaskReadiness, TaskReadinessStatus, TaskReadinessType, TaskReadinessStatusItem
- AssignmentDurationPrediction, AssignmentShortTermPrediction
- KpiOverdue, KpiOverDuration
- Wor, WorType

**Does NOT own:**
- Person / Organisation / Discipline / Schedule (→ Workforce)
- TimeSheet / ErpHourline / Balance (→ Timekeeping)
- Ticket / TransportRequest / StopWorkOrder (→ Workflow)
- ClockingTerminal / Punch (→ Devices)
- SyncLog / ImportConfig (→ Sync)

**Entry points (from Phase 1):**
EP-402–EP-419 (Pbs Area), EP-601–EP-602 (Planboard), EP-603–EP-604 (Floorboard), EP-703–EP-704 (KPI/Reports), EP-1013 (TaskReadiness), EP-112–EP-113 (API import), EP-120–EP-135 (OData — Activity, Assignment, Project, Component, Baseline, etc.), EP-2001–EP-2003, EP-2009–EP-2010 (import jobs — XER, Sciforma, Planning, ProductBreakdown, TaskReadiness), EP-5002, EP-5005–EP-5006, EP-5101–EP-5105, EP-5201–EP-5209, EP-5301–EP-5303, EP-5501–EP-5502, EP-5971

**Data stores:**
`Floor2PlanDbContext` — DbSets: Projects, Activities, Components, Assignments, Baselines, ProjectMaterials, ActivityRelations, ActivityPhases, ActivityGroups, ProjectScenarios, ActivitySummaries, AssignmentSummaries, BaselineProjects/Components/Activities/Assignments, TaskReadiness*, KpiOverdue, KpiOverDuration, AssignmentDurationPredictions, AssignmentShortTermPredictions, Wors, etc.

**External dependencies:**
- reads Person, Organisation, Discipline from **Workforce** (FK read-only)
- cross-refs via TimeSheetWeeklyHourline with **Timekeeping** (hours booked against activities)
- receives bulk data from **Sync** (XER, Sciforma, Planning, ProductBreakdown import jobs)
- pushes aggregates to **Reporting** (ReportingDbContext)

**Current code locations:**
- UI: `Src/UI/UI.Floor2Plan/Areas/Pbs/`, `Areas/Plan/`, `Areas/FloorSpace/`
- Application: `Src/Application/Application.Service/Pbs/`, `Application.Sync/ImportJobs/{Planning,Sciforma,Xer,ProductBreakdown,TaskReadiness}/`
- Domain: `Src/Domain/Domain.Model/{Planning,Activities,Assignments,Components,Projects,PBS,ProjectScenarios,TaskReadiness,Prediction,Kpi}/`
- Domain services: `Src/Domain/Domain.Service/Pbs/`, `Planning/`
- Data: `Src/Data/Data.Model/{Activity,Project,Component,Assignment,Baseline*,Material*,TaskReadiness*,Kpi*,Prediction*,Wor*}.cs`
- Processors: `Src/Processors/Processors.Domain/{SyncPlanningProcessor,ProductBreakdownProcessor,ApplyActivity/AssignmentSummaryProcessor,CreateBaselineProcessor,SetActivitySequenceProcessor,ActivityConnectorUpdateProcessor,RefreshActivity*,RefreshComponent*,LongTermPredictionProcessor,ShortTermPredictionProcessor}`

---

## Context 2 — Workforce & Organization (HR/Workforce)

**Description:** Organisational master data — employee records, org structure, discipline definitions, work schedules, and capacity.

**Ubiquitous language:**
Person · Organisation · Discipline · Team · PersonSchedule · OrganisationSchedule · ScheduleEntry · ScheduleEntryPeriod · WorkingHours · Efficiency · Tariff · OrganisationCapacityDashboardLine · DisciplineCapacityDashboardLine · ScheduleRoundingRuleSet · ScheduleSourceType

**Owns:**
- Person (root), Organisation (root), Discipline (root), Team (root), Schedule (root)
- PersonProperty, DisciplineProperty, DisciplinePerson, OrganisationRole, OrganisationInRole, OrganisationProperty, Role, User, UserInRole, PersonInTeam, OrganisationSchedule, OrganisationScheduleEntry, PersonSchedule, PersonScheduleEntry, ScheduleEntry, ScheduleEntryPeriod, ScheduleRoundingRuleSet, Location, LocationType, LocationProperty, LocationOrganisation, OrganisationCapacityDashboardLine, DisciplineCapacityDashboardLine

**Does NOT own:**
- TimeSheet / ErpHourline / Balance (→ Timekeeping)
- Project / Activity / Assignment (→ Planning)
- ClockingTerminal / Punch (→ Devices)
- Ticket (→ Workflow)

**Entry points:**
EP-407–EP-409 (Pbs: Person, Discipline, Organisation), EP-414–EP-415 (Pbs: Role, User), EP-501–EP-509 (HR Area), EP-1008–EP-1009 (PermissionManagement, PersonManagement), EP-124–EP-125, EP-128 (OData: Persons, Organisations, Disciplines), EP-2011–EP-2012, EP-2015–EP-2017 (import jobs — Persons, Organisations, Disciplines)

**Data stores:**
`Floor2PlanDbContext` — DbSets: Persons, Organisations, Disciplines, Teams, Schedules, ScheduleEntries, PersonSchedules, OrganisationSchedules, Roles, Users, UserInRoles, Locations, LocationTypes, OrganisationCapacityDashboardLines, DisciplineCapacityDashboardLines, etc.

**External dependencies:**
- referenced by all other contexts (Person/Organisation are the most shared entities in the system)
- receives bulk imports from **Sync** (Persons, Organisations, Disciplines)
- pushes to **Reporting**

**Current code locations:**
- UI: `Src/UI/UI.Floor2Plan/Areas/HR/`, `Areas/PBS/` (Person, Org, Discipline controllers)
- Application: `Src/Application/Application.Service/Hr/{OffTime}/`, `Application.Sync/ImportJobs/{Persons,Organisations,Disciplines,Employees,DisciplineImport,OrganisationImport}/`
- Domain: `Src/Domain/Domain.Model/{Hr,PersonManagement,Schedules}/`
- Domain services: `Src/Domain/Domain.Service/Schedules/`
- Data: `Src/Data/Data.Model/{Person,Organisation,Discipline,Team,Schedule*,Location*}.cs`
- Processors: `Src/Processors/Processors.Domain/{HierarchyCacheProcessor,OrganisationLabelCacheProcessor,DisciplineLabelCacheProcessor,RefreshOrganisationCapacity*,RefreshDisciplineCapacity*,RefreshCachePersonalSchedules*}`

---

## Context 3 — Timekeeping & Work Hours (Timesheets)

**Description:** Timesheet entry, manager approval, ERP hourline synchronisation, balance policies (vacation/overtime accumulation), and correction timesheets.

**Ubiquitous language:**
TimeSheet · TimesheetEntry · TimeSheetStatus · TimeSheetRemark · TimeSheetWeeklyHourline · ErpHourline · Hourline · Balance · BalancePolicy · BalancePolicyRule · BalanceAccumulationRule · BalanceMutation · BalanceMutationHistory · PersonBalancePolicy · CorrectionTimesheet · OvertimeEntry · HourType · TimesheetEventMessage · PersonTimesheetApprover

**Owns:**
- TimeSheet (root), ErpHourline (root), Balance (root), BalancePolicy (root), CorrectionTimesheet (root), OvertimeEntry (root)
- TimesheetEntry, TimeSheetRemark, TimeSheetStatus, TimeSheetWeeklyHourline, Hourline, HourlyRate, HourType, BalanceAccumulationRule, BalancePolicyRule, BalancePolicyInBalancePolicyRule, BalanceMutationType, BalanceMutationHistory, CorrectionTimesheetWeeklyCorrectionLine, PersonTimesheetApprover, PersonHoliday, PersonHolidayStatus, PersonHolidayInTimesheetHours, OrganisationHoliday, OrganisationOffTimeInTimesheetHours, TimesheetEventMessage, MilestoneEventMessage

**Does NOT own:**
- Person / Organisation / Schedule (→ Workforce)
- Project / Activity / Assignment (→ Planning; though TimeSheetWeeklyHourline references them)
- ClockingTerminal / Punch (→ Devices)
- Ticket (→ Workflow)

**Entry points:**
EP-111 (API: timesheet status), EP-503–EP-509 (HR: balances, policies), EP-605–EP-610 (Do: weekly timesheet, employee timesheet, correction), EP-705–EP-706 (Check: ERP timesheet), EP-127, EP-131–EP-132 (OData: TimeSheets, Balances, CorrectionTimesheets), EP-2007, EP-2025 (import jobs — Timesheets, HourTypes), EP-5003, EP-5201–EP-5202, EP-5208, EP-5701–EP-5703, EP-5981–EP-5983

**Data stores:**
`Floor2PlanDbContext` — DbSets: TimeSheets, TimesheetEntries, TimeSheetRemarks, TimeSheetStatuses, TimeSheetWeeklyHourlines, Hourlines, ErpHourlines, HourlyRates, HourTypes, Balances, BalancePolicies, BalancePolicyRules, BalanceAccumulationRules, CorrectionTimesheets, PersonHolidays, PersonTimesheetApprovers, TimesheetEventMessages, etc.

**External dependencies:**
- reads Person, Organisation, PersonSchedule from **Workforce** (approval chain, schedule-based calculations)
- cross-refs Activity, Project from **Planning** via TimeSheetWeeklyHourline (hours per activity)
- receives punch data from **Devices** for clocking-based timesheet validation
- imports from **Sync** (ERP timesheet import, HoursAndProgress)
- raises domain events consumed by **Workflow** (Elsa) for state transitions

**Current code locations:**
- UI: `Src/UI/UI.Floor2Plan/Areas/HR/` (balances), `Areas/Do/` (timesheet entry/approval)
- Application: `Src/Application/Application.Service/{EmployeeTimesheet,Balances}/`, `Application.Sync/ImportJobs/{Timesheets,HoursAndProgress}/`
- Domain: `Src/Domain/Domain.Model/{Timesheets,Balances,OffTime}/`
- Domain services: `Src/Domain/Domain.Service/{Timesheets,Balances}/`
- Data: `Src/Data/Data.Model/{TimeSheet*,TimesheetEntry,ErpHourline,Hourline,Balance*,CorrectionTimesheet,OvertimeEntry,PersonHoliday*,HourType}.cs`
- Processors: `Src/Processors/Processors.Domain/{BookHoursProcessor,CreateErpHourlinesProcessor,UpdateBalanceProcessor,UpdateBalancePendingHoursProcessor,UpdateBalanceByTimesheetProcessor,RefreshTimesheetEntry*,RefreshTimesheetEventMessages*,SetErpHourlineHourTypesProcessor}`

---

## Context 4 — Devices & Shop Floor Clocking (Clocking/Devices)

**Description:** Clocking terminal hardware management, RFID access profiles, punch record capture, and integration with external access-control systems.

**Ubiquitous language:**
ClockingTerminal · ShopFloorTerminal · ClockingTerminalPunch · AccessControlTime · AccessControlTimeClockingTerminalPunch · PersonalDailyClockTimes · ClockingTerminalProfile · ClockingTerminalSetting · ShopFloorTerminalSetting · RfidSetting · ClockType · CachePersonalSchedule

**Owns:**
- ClockingTerminal (root), ShopFloorTerminal (root), AccessControlTime (root)
- ClockingTerminalPunch, ClockingTerminalProfile, ClockingTerminalSetting, ShopFloorTerminalSetting, AccessControlTimeClockingTerminalPunch, PersonalDailyClockTimes, RfidSetting, ClockType, CachePersonalSchedule, CachePersonalScheduleOvertime, CachePersonalSchedulePeriod

**Does NOT own:**
- Person / PersonSchedule (→ Workforce; referenced via FK for punch matching)
- TimeSheet (→ Timekeeping)
- Project / Activity (→ Planning)

**Entry points:**
EP-501–EP-502 (HR: ClockingTerminal, ClockingTerminalProfile), EP-604 (Do: Floorboard clocking update), EP-921–EP-922 (Devices area), EP-129–EP-130 (OData: AccessControlTimes, ClockingTerminals), EP-2008 (import job — AccessControl), EP-5004, EP-5973

**Data stores:**
`Floor2PlanDbContext` — DbSets: ClockingTerminals, ShopFloorTerminals, ClockingTerminalPunches, AccessControlTimes, AccessControlTimeClockingTerminalPunches, ClockingTerminalProfiles, ClockingTerminalSettings, ShopFloorTerminalSettings, RfidSettings, ClockTypes, PersonalDailyClockTimes, CachePersonalSchedule*, etc.

**External dependencies:**
- reads Person, PersonSchedule from **Workforce** (punch-to-person matching)
- feeds PersonalDailyClockTimes into **Timekeeping** for balance/hourline calculations
- imports from **Sync** (AccessControl import job)

**Current code locations:**
- UI: `Src/UI/UI.Floor2Plan/Areas/Devices/`, `Areas/HR/` (ClockingTerminalController)
- Application: `Src/Application/Application.Service/Clocking/`
- Domain: `Src/Domain/Domain.Model/{Clocking,Devices}/`
- Domain services: `Src/Domain/Domain.Service/Clocking/`
- Data: `Src/Data/Data.Model/{ClockingTerminal*,ShopFloorTerminal*,AccessControlTime*,PersonalDailyClockTimes,RfidSetting,ClockType}.cs`
- Processors: `Src/Processors/Processors.Domain/EntryTimes/GenericEntryTimeProcessor.cs`, `RefreshCachePersonalSchedulesProcessor`

---

## Context 5 — Workflow & Ticketing (Tickets)

**Description:** Issue tickets, transport requests, and stop work orders with lifecycle state machines driven by Elsa workflows. Tracks remarks, custom properties, and links to planning entities.

**Ubiquitous language:**
TicketBase · Ticket · TransportRequest · StopWorkOrder · TicketStatus · TicketRemark · TicketProperty · ActivityTicketRelation · ReporterPerson · AssignedPerson · TicketStateChangeEvent · TicketRemarkAddedEvent · TransportRequestCreatedEvent · StopWorkOrderCreatedEvent

**Owns:**
- TicketBase (root, polymorphic discriminator), TransportRequest (subtype), StopWorkOrder (subtype)
- TicketRemark, TicketProperty, ActivityTicketRelation

**Does NOT own:**
- Person / Organisation (→ Workforce; used for reporter/assignee FKs)
- Project / Activity / Component (→ Planning; referenced via ActivityTicketRelation)
- Location (shared with Devices/Workforce — [NEEDS REVIEW])

**Entry points:**
EP-801–EP-804 (Ticket area), EP-3001–EP-3012 (Elsa workflow activities), EP-3101–EP-3106 (Elsa domain event triggers)

**Data stores:**
`Floor2PlanDbContext` — DbSets: Tickets (TicketBase discriminator), TransportRequests, StopWorkOrders, TicketRemarks, TicketProperties, ActivityTicketRelations.
`Infrastructure.Elsa.DbContext` — Elsa workflow definitions, instances, bookmarks.

**External dependencies:**
- reads Activity, Project, Component from **Planning** via ActivityTicketRelation (read-only reference)
- reads Person, Organisation from **Workforce** (reporter, assignee)
- publishes domain events (TicketStateChangeEvent, TransportRequestCreatedEvent, StopWorkOrderCreatedEvent) consumed by **Elsa** workflow engine
- **Sync** has no import jobs for tickets (tickets are UI/workflow-created only)

**Current code locations:**
- UI: `Src/UI/UI.Floor2Plan/Areas/Ticket/`
- Application: `Src/Application/Application.Service/Ticket/`
- Domain: `Src/Domain/Domain.Model/Ticket/{StopWorkOrder}/`
- Infrastructure: `Src/Infrastructure/Infrastructure.Workflow/Activities/Ticket/`, `Infrastructure.Workflow/Events/Ticket/`
- Data: `Src/Data/Data.Model/Base/TicketBase.cs`, `TransportRequest.cs`, `StopWorkOrder.cs`, `TicketRemark.cs`, `TicketProperty.cs`, `ActivityTicketRelation.cs`

---

## Context 6 — Synchronization & Import Pipeline (Sync)

**Description:** Orchestrates periodic bulk imports from external systems (Sciforma, Oracle Primavera XER, ERP, EntryTime, Aspose/Excel) via Hangfire. Manages connector configuration, import mappings, and result tracking via SyncLog.

**Ubiquitous language:**
SyncLog · ImportConfig · ImportConfigRule · ImportJob · ImportProvider · Connector · ProjectAllocation · TaskAllocation · HourlineSync · Mapping · Resolver · DataTransformation

**Owns:**
- SyncLog (root), ImportConfig (root)
- ImportConfigRule, NewsFeedMessage (sync result notifications)

**Does NOT own:**
- Any domain entities — Sync writes *into* other contexts' tables via import jobs but does not own them

**Entry points:**
EP-301–EP-319 (Sync area — UI), EP-2001–EP-2025 (all Hangfire import jobs), EP-3006–EP-3007 (Elsa RunConnectorActivity, SyncLogSendMailActivity), EP-3106 (SyncFinishEvent), EP-6001–EP-6004 (DatabaseDeployer — related infrastructure)

**Data stores:**
`Floor2PlanDbContext` — DbSets: SyncLogs, ImportConfigs, ImportConfigRules, NewsFeedMessages.

**External dependencies:**
- writes to **Planning** (Project, Activity, Component, Assignment, Material, TaskReadiness)
- writes to **Workforce** (Person, Organisation, Discipline)
- writes to **Timekeeping** (TimeSheet, Hourline, HourType)
- writes to **Devices** (AccessControlTime, ClockingTerminalPunch)
- notifies **Workflow** via SyncFinishEvent
- scheduled and monitored by **System** (Hangfire via Infrastructure.Hangfire)

**Current code locations:**
- Application: `Src/Application/Application.Sync/` (all import jobs and providers)
- Connectors: `Src/Connectors/` (external system adapters)
- Infrastructure: `Src/Infrastructure/Infrastructure.Hangfire/` (job scheduling)
- Data: `Src/Data/Data.Model/{SyncLog,ImportConfig*}.cs`
- UI: `Src/UI/UI.Floor2Plan/Areas/Sync/`

---

## Context 7 — System Administration & Infrastructure (System)

**Description:** Application-wide configuration, licensing, cache management, processor orchestration, user/role administration, and platform infrastructure.

**Ubiquitous language:**
AppSetting · SystemAction · License · Cache · CacheKey · CacheInvalidation · Processor · AuditLog · PageSetting · RuntimeConfiguration · PropertySet · PropertyType · Task

**Owns:**
- AppSetting (root), License (root), SystemAction (root)
- PageSetting, PropertySet, PropertyType, PropertyTypeOption, GenericPropertyBase (extensible configuration)
- F2PTask (internal system task representation)
- User, Role, UserInRole (authentication/authorization — [NEEDS REVIEW]: partially shared with Workforce)

**Does NOT own:**
- Domain-specific aggregates from other contexts

**Entry points:**
EP-1001–EP-1004 (System area), EP-1006–EP-1007 (GeneralSettings, PageSettings), EP-1008 (PermissionManagement), EP-4001–EP-4039 (all System Actions), EP-5801–EP-5806 (cleanup processors), EP-6001–EP-6004 (DatabaseDeployer), EP-2101–EP-2103 (hosted services)

**Data stores:**
`Floor2PlanDbContext` — DbSets: AppSettings, Licenses, PageSettings, PropertySets, PropertyTypes, PropertyTypeOptions, GenericProperties, Tasks, Users, Roles, UserInRoles.
`Infrastructure.Configuration.Runtime.DbContext` — runtime settings.
`Infrastructure.Authorisation.DbContext` — permission/role data.
`Infrastructure.Health.DbContext` — health check state.
`Infrastructure.Audit.DbContext` — audit trail (system-wide).

**External dependencies:**
- provides cache invalidation and processor execution to **all other contexts**
- manages Elsa workflow engine lifecycle (→ **Workflow**)
- manages Hangfire job scheduling (→ **Sync**)
- provides AppSetting lookup to all contexts

**Current code locations:**
- UI: `Src/UI/UI.Floor2Plan/Areas/System/`
- Infrastructure: `Src/Infrastructure/Infrastructure.Configuration.*`, `Infrastructure.Authorisation.*`, `Infrastructure.State.*`, `Infrastructure.Processors.*`, `Infrastructure.Audit.*`
- Application: `Src/Application/Application.Service/{SystemActions,License}/`, `Application.SystemActions/`
- Domain: `Src/Domain/Domain.Model/{License,Table}/`
- Processors: `Src/Processors/Processors.Domain/` (all 35+ processors — [NEEDS REVIEW]: ownership per context)

---

## Cross-Context Relationship Map

| Direction | Type | Evidence | Risk |
|-----------|------|----------|------|
| Planning → Workforce | Shared table FK (read) | PersonId on Assignment; OrganisationId on Project | LOW |
| Planning ↔ Timekeeping | Shared bridge table | TimeSheetWeeklyHourline (FK to Activity + TimeSheet) | **MEDIUM** |
| Timekeeping → Workforce | Shared table FK (read) + approval chain | PersonId on TimeSheet; PersonTimesheetApprover | **MEDIUM** |
| Devices → Workforce | Shared table FK (read) | PersonId on ClockingTerminalPunch; PersonSchedule | LOW |
| Devices → Timekeeping | Data integration | PersonalDailyClockTimes feeds balance/hourline calcs | **MEDIUM** |
| Workflow → Planning | Shared table FK (read) | ActivityTicketRelation (Activity, Component, Project) | LOW |
| Workflow → Workforce | Shared table FK (read) | Person on Ticket (reporter, assignee) | LOW |
| Workflow → Elsa engine | Domain events | TicketStateChangeEvent, TransportRequestCreatedEvent, StopWorkOrderCreatedEvent | **MEDIUM** |
| Sync → Planning | Bulk write (import job) | XER, Sciforma, Planning, ProductBreakdown, TaskReadiness jobs | **MEDIUM** |
| Sync → Workforce | Bulk write (import job) | Persons, Organisations, Disciplines jobs | **MEDIUM** |
| Sync → Timekeeping | Bulk write (import job) | Timesheets, HoursAndProgress, HourTypes jobs | **MEDIUM** |
| Sync → Devices | Bulk write (import job) | AccessControl import job | LOW |
| Sync → Workflow | Domain event | SyncFinishEvent | LOW |
| System → All | Processor invocation + cache flush | 35+ processors; AppSetting lookup | **HIGH** |
| System → Workflow | State management | Elsa engine lifecycle | **MEDIUM** |
| System → Sync | Job scheduling | Hangfire configuration | **MEDIUM** |

---

## Top 10 Boundary Violations

| # | Violation | Files | Risk | Notes |
|---|-----------|-------|------|-------|
| 1 | **Shared Person/Organisation across 6 contexts** | `Src/Data/Data.Model/Person.cs`, `Organisation.cs` | **HIGH** | Person has cache fields mutated by multiple contexts (CacheCurrentPersonBalancePolicyId, CacheScheduleSourceType, CacheEfficiency). |
| 2 | **TimeSheetWeeklyHourline bridges Planning ↔ Timekeeping** | `Src/Data/Data.Model/TimeSheetWeeklyHourline.cs` | **HIGH** | FK to both Activity/Project (Planning) and TimeSheet (Timekeeping) creates bidirectional coupling. |
| 3 | **Processor system implicitly orchestrates all contexts** | `Src/Processors/Processors.Domain/` (all 35+ processors) | **HIGH** | No declared cross-context boundary — processors update entities in any context from any trigger. |
| 4 | **Single FloorplanDbContext owns 200+ entities across all contexts** | `Src/Data/Data.EntityFramework/Floor2PlanDbContext.cs` | **HIGH** | All contexts share one DbContext; impossible to enforce FK isolation. |
| 5 | **ActivityTicketRelation bridges Workflow → Planning** | `Src/Data/Data.Model/ActivityTicketRelation.cs` | **MEDIUM** | Ticket may update Activity status via Elsa activity — mutation direction unclear. |
| 6 | **Cache fields stored on domain entities** | `Src/Data/Data.Model/{Person,Activity,Component}.cs` | **MEDIUM** | CacheHoursBudget, CacheCanBookHours, CacheIcon on entities cross context boundaries for read performance. |
| 7 | **Elsa workflow event subscribers not enumerated** | `Src/Infrastructure/Infrastructure.Workflow/Events/` | **MEDIUM** | Hidden cross-context coupling: which contexts subscribe to TicketStateChangeEvent, TimesheetEventMessage is not mapped. |
| 8 | **BalancePolicy assigned to Person (Timekeeping → Workforce coupling)** | `Src/Data/Data.Model/PersonBalancePolicy.cs` | **MEDIUM** | Timekeeping policy governed by Workforce entity — ownership ambiguous. |
| 9 | **Sync jobs write to multiple contexts in single Hangfire job** | `Src/Application/Application.Sync/ImportJobs/` | **MEDIUM** | One import job may update Planning + Workforce + Timekeeping in one transaction. No partial-failure isolation. |
| 10 | **User/Role shared between Workforce and System** | `Src/Data/Data.Model/{User,Role,UserInRole}.cs` | **LOW–MEDIUM** | Identity entities used for auth (System) and org structure (Workforce) without clear ownership boundary. |

---

## Mermaid Diagram

See `02-context-map.mermaid`.

---

## Rationale: Why 7 Contexts?

### Merges rejected

| Candidate | Decision | Reason |
|-----------|----------|--------|
| Devices + Timekeeping | Rejected | Different lifecycles and stakeholders: Devices = hardware integration; Timekeeping = business approval process |
| Workflow + Planning | Rejected | Workflow is reactive/state-machine; Planning is hierarchical resource allocation |
| Workforce + Planning | Rejected | Person is shared but owned by different teams; Planning shouldn't own org structure |
| Sync + System | Rejected | Sync is domain-specific import orchestration; System is cross-cutting infrastructure |
| Planning + Materials | Rejected (kept together) | Materials are tightly coupled to ProjectMaterial in project planning vocabulary |
| Timekeeping + Balances | Rejected (kept together) | Balance policy drives timesheet approval — same stakeholder owns both |

### Splits rejected

| Candidate | Decision | Reason |
|-----------|----------|--------|
| Reporting as separate context | Rejected | Read-only projection (ReportingDbContext); no domain logic; excluded from map |
| Planning → Planning + Materials | Rejected | Material mastery is planning-centric; no separate team or lifecycle |
| Timekeeping → Timekeeping + Balances | Rejected | Approval and balance policy are unified business domain |

### Observations for future splitting

- **Person/Organisation "Core" subdomain** — if Person becomes the bottleneck across contexts, consider extracting a read-only Person registry that all contexts reference via events.
- **Reporting** — currently excluded; treat as a separate concern driven by ETL/projections from all contexts.
- **Prediction** — could eventually be its own context if it grows (ML model, separate team); currently included in Planning.

---

## Items Requiring Human Review

| Item | Notes |
|------|-------|
| Context 1: Planning boundary | [NEEDS REVIEW] Confirm TaskReadiness, Predictions, and KPI belong here vs. a separate Analytics context |
| Context 2: User/Role ownership | [NEEDS REVIEW] Should User/Role belong to Workforce or System? |
| Context 5: Ticket→Activity mutation direction | [NEEDS REVIEW] Does Elsa activity `SetActivityStatusActivity` modify Planning entities from within the Workflow context? If yes, this is a violation. |
| Context 5: Location ownership | [NEEDS REVIEW] Location used by Transport requests (Workflow), Organisation (Workforce), and FloorSpace (Planning/System) — determine owner |
| Context 7: Processor ownership | [NEEDS REVIEW] 35+ processors in `Processors.Domain/` need to be mapped to their owning context |
| Elsa event subscribers | [NEEDS REVIEW] Enumerate all Elsa workflow event handlers to confirm cross-context event flows |
| Sync transaction boundaries | [NEEDS REVIEW] Confirm whether multi-context import jobs run in a single DB transaction or separate ones |
| ReportingDbContext | [NEEDS REVIEW] Clarify how reporting projections are maintained — ETL job, event-driven, or direct DbContext write |

---

*Phase 2 draft complete. **HUMAN VALIDATION REQUIRED** before proceeding to Phase 3 (use case extraction).*
*Validation checklist: see `analysis-instructions.md` Phase 2 section.*
