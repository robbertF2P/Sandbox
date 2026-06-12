namespace ApiImportActorPoc.Contracts.Models.Import;

public sealed record AssignmentImportPayload(
    string? Id,
    string PersonName,
    string? Description,
    IReadOnlyDictionary<string, string>? ExternalIds = null);
