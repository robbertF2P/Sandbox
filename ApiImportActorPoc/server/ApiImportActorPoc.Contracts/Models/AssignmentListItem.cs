using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models;

public sealed record AssignmentListItem(
    int Id,
    int ProjectId,
    string ProjectName,
    string ComponentPath,
    string ActivityName,
    PersonName PersonName,
    Hours BudgetedHours,
    Hours HoursWorked);
