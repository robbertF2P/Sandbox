namespace ApiImportActorPoc.Core.Excel.Primavera.Models;

public sealed class PrimaveraTaskRow
{
    public string TaskId { get; set; } = string.Empty;

    public string ActivityId { get; set; } = string.Empty;

    public string TaskName { get; set; } = string.Empty;

    public string? ResourceName { get; set; }

    public string? BudgetedUnits { get; set; }

    public string? RemainingUnits { get; set; }

    public string? TradeCode { get; set; }
}
