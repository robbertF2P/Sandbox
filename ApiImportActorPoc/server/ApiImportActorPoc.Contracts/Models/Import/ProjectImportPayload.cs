namespace ApiImportActorPoc.Contracts.Models.Import;

public sealed record ProjectImportPayload(
    string Name,
    IReadOnlyList<ComponentImportPayload> Components,
    IReadOnlyDictionary<string, string>? ExternalIds = null);
