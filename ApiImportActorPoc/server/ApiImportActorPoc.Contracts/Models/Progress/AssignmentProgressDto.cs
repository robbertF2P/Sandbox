namespace ApiImportActorPoc.Contracts.Models.Progress;

public sealed record AssignmentProgressDto(
    int Id,
    string PersonName,
    string? Description,
    ProgressSummary Progress);
