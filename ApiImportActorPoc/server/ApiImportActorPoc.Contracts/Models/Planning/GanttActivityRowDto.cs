namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record GanttActivityRowDto(
    int ActivityId,
    string ActivityName,
    string ComponentName,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<GanttAssignmentRowDto> Assignments);
