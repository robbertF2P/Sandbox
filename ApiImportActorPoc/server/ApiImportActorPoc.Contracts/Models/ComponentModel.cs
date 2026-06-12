namespace ApiImportActorPoc.Contracts.Models;

public sealed record ComponentModel(
    int Id,
    string Name,
    bool IsTemplate,
    IReadOnlyList<ComponentModel> ChildComponents,
    IReadOnlyList<ActivityModel> Activities,
    IReadOnlyDictionary<string, string> ExternalIds);
