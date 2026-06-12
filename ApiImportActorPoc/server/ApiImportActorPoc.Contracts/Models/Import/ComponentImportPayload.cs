namespace ApiImportActorPoc.Contracts.Models.Import;

public sealed record ComponentImportPayload(
    string? Id,
    string Name,
    IReadOnlyList<ComponentImportPayload>? ChildComponents,
    IReadOnlyList<ActivityImportPayload>? Activities);
