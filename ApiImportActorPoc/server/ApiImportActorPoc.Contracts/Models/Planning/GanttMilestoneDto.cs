namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record GanttMilestoneDto(
    int Id,
    string Name,
    DateOnly TargetDate,
    int? ActivityId);
