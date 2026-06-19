namespace PrimaveraExcelReader.Primavera.Models;

public sealed class PrimaveraActivityRow
{
    public string ActivityId { get; set; } = string.Empty;

    public string ActivityName { get; set; } = string.Empty;

    public string WbsCode { get; set; } = string.Empty;

    public string? Status { get; set; }

    public string? PlannedStart { get; set; }

    public string? PlannedFinish { get; set; }

    public string? DurationHours { get; set; }
}
