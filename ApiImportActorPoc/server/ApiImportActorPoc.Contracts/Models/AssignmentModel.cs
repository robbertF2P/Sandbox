using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models;

public sealed record AssignmentModel(
    int Id,
    PersonName PersonName,
    string? Description,
    Hours BudgetedHours,
    IReadOnlyDictionary<string, string> ExternalIds);
