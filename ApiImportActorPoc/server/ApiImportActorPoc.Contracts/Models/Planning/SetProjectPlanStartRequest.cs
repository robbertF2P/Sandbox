using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record SetProjectPlanStartRequest(ScheduleDate PlannedStartDate);
