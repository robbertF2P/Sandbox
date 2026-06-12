namespace ApiImportActorPoc.Contracts.Models;

public sealed record AssignmentListItem(
    int Id,
    int ProjectId,
    string ProjectName,
    string ComponentPath,
    string ActivityName,
    string PersonName,
    decimal BudgetedHours,
    decimal HoursWorked);
