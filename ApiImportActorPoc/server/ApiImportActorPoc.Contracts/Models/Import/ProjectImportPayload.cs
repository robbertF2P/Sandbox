namespace ApiImportActorPoc.Contracts.Models.Import;

public sealed record ProjectImportPayload(
    string Name,
    IReadOnlyList<ComponentImportPayload> Components);
