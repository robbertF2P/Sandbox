namespace ApiImportActorPoc.Contracts.Models;

public sealed record ProjectModel(
    int Id,
    string Name,
    IReadOnlyList<ComponentModel> Components,
    IReadOnlyDictionary<string, string> ExternalIds);
