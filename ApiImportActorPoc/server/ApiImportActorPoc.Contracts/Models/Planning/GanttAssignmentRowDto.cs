using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record GanttAssignmentRowDto(
    int AssignmentId,
    string Label,
    DurationDays DurationDays,
    ScheduleDate StartDate,
    ScheduleDate EndDate);
