namespace ApiImportActorPoc.Contracts.Models.Import;

public sealed record ComponentImportPayload(
    string? Id,
    string Name,
    bool? IsTemplate,
    IReadOnlyList<ComponentImportPayload>? ChildComponents,
    IReadOnlyList<ActivityImportPayload>? Activities,
    IReadOnlyDictionary<string, string>? ExternalIds = null);
