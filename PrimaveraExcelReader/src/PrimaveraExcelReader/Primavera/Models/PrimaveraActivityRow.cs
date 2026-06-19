namespace PrimaveraExcelReader.Primavera.Models;

public sealed class PrimaveraActivityRow
{
    public string ActivityId { get; set; } = string.Empty;

    public string ActivityName { get; set; } = string.Empty;

    public string WbsCode { get; set; } = string.Empty;

    public string? Status { get; set; }

    public DateOnly? PlannedStart { get; set; }

    public DateOnly? PlannedFinish { get; set; }

    public decimal? DurationHours { get; set; }
}
