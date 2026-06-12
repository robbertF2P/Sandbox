namespace ApiImportActorPoc.Contracts.Models.Progress;

public sealed record ComponentProgressDto(
    int Id,
    string Name,
    ProgressSummary Progress,
    IReadOnlyList<ComponentProgressDto> ChildComponents,
    IReadOnlyList<ActivityProgressDto> Activities);
