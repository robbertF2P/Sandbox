namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record GanttProjectPlanDto(
    int ProjectId,
    string ProjectName,
    DateOnly PlannedStartDate,
    DateOnly PlannedEndDate,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<GanttActivityRowDto> Activities,
    IReadOnlyList<GanttMilestoneDto> Milestones);
