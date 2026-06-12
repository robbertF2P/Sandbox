namespace ApiImportActorPoc.Contracts.Models.Import;

public sealed record ActivityImportPayload(
    string? Id,
    string Name,
    IReadOnlyList<AssignmentImportPayload>? Assignments,
    IReadOnlyList<ActivityRelationImportPayload>? Relations,
    IReadOnlyDictionary<string, string>? ExternalIds = null);
