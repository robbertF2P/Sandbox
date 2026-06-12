namespace ApiImportActorPoc.Contracts.Models;

public sealed record ComponentModel(
    Guid Id,
    string Name,
    IReadOnlyList<ComponentModel> ChildComponents,
    IReadOnlyList<ActivityModel> Activities,
    IReadOnlyDictionary<string, string> ExternalIds);
