using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record GanttMilestoneDto(
    int Id,
    string Name,
    ScheduleDate TargetDate,
    int? ActivityId);
