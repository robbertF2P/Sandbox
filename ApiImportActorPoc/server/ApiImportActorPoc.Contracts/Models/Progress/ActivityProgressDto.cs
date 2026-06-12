namespace ApiImportActorPoc.Contracts.Models.Progress;

public sealed record ActivityProgressDto(
    int Id,
    string Name,
    ProgressSummary Progress,
    IReadOnlyList<AssignmentProgressDto> Assignments);
