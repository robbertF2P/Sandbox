namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record GanttAssignmentRowDto(
    int AssignmentId,
    string Label,
    decimal DurationDays,
    DateOnly StartDate,
    DateOnly EndDate);
