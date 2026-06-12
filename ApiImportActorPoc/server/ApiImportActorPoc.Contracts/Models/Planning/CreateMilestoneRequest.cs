using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record CreateMilestoneRequest(
    string Name,
    ScheduleDate TargetDate,
    int? ActivityId = null);
