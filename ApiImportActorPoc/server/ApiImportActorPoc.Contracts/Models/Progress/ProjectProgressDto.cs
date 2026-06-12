namespace ApiImportActorPoc.Contracts.Models.Progress;

public sealed record ProjectProgressDto(
    int Id,
    string Name,
    ProgressSummary Progress,
    IReadOnlyList<ComponentProgressDto> Components);
