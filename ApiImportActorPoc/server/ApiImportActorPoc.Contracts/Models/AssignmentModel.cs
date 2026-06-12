namespace ApiImportActorPoc.Contracts.Models;

public sealed record AssignmentModel(
    Guid Id,
    string PersonName,
    string? Description,
    IReadOnlyDictionary<string, string> ExternalIds);
