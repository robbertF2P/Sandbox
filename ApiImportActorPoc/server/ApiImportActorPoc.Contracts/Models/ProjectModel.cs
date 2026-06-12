namespace ApiImportActorPoc.Contracts.Models;

public sealed record ProjectModel(
    Guid Id,
    string Name,
    IReadOnlyList<ComponentModel> Components,
    IReadOnlyDictionary<string, string> ExternalIds);
