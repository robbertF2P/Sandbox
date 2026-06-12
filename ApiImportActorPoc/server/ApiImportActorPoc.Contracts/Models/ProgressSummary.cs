using ApiImportActorPoc.Contracts.Values;

namespace ApiImportActorPoc.Contracts.Models;

public sealed record ProgressSummary(
    Hours BudgetedHours,
    Hours HoursWorked)
{
    public decimal PercentComplete => BudgetedHours.Value <= 0
        ? (HoursWorked.Value > 0 ? 100 : 0)
        : Math.Round(HoursWorked.Value / BudgetedHours.Value * 100, 1);
}
