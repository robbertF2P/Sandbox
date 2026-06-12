using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record GanttProjectPlanDto(
    int ProjectId,
    string ProjectName,
    ScheduleDate PlannedStartDate,
    ScheduleDate PlannedEndDate,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<GanttActivityRowDto> Activities,
    IReadOnlyList<GanttMilestoneDto> Milestones);
