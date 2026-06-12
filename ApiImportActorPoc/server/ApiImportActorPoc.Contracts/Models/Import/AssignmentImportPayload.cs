namespace ApiImportActorPoc.Contracts.Models.Import;

public sealed record AssignmentImportPayload(
    string? Id,
    string PersonName,
    string? Description);
