namespace ApiImportActorPoc.Contracts.Models;

public sealed record ProgressSummary(
    decimal BudgetedHours,
    decimal HoursWorked)
{
    public decimal PercentComplete => BudgetedHours <= 0
        ? (HoursWorked > 0 ? 100 : 0)
        : Math.Round(HoursWorked / BudgetedHours * 100, 1);
}
