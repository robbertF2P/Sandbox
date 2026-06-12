namespace ApiImportActorPoc.Contracts.Models;

public sealed record AssignmentModel(
    int Id,
    string PersonName,
    string? Description,
    IReadOnlyDictionary<string, string> ExternalIds);
