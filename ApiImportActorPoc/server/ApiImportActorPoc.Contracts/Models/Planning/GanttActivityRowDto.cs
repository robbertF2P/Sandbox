using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models.Planning;

public sealed record GanttActivityRowDto(
    int ActivityId,
    string ActivityName,
    string ComponentName,
    ScheduleDate StartDate,
    ScheduleDate EndDate,
    IReadOnlyList<GanttAssignmentRowDto> Assignments);
