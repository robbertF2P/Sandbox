namespace ApiImportActorPoc.Contracts.Models;

public sealed record AssignmentModel(
    int Id,
    string PersonName,
    string? Description,
    decimal BudgetedHours,
    IReadOnlyDictionary<string, string> ExternalIds);
