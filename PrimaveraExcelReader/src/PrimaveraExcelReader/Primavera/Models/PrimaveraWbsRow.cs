namespace PrimaveraExcelReader.Primavera.Models;

public sealed class PrimaveraWbsRow
{
    public string WbsCode { get; set; } = string.Empty;

    public string WbsName { get; set; } = string.Empty;

    public string? ParentWbsCode { get; set; }

    public string? ProjectId { get; set; }
}
