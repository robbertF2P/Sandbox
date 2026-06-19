namespace PrimaveraExcelReader.Mapping;

public sealed record ExcelReadResult<T>(
    string SheetName,
    IReadOnlyList<T> Rows,
    IReadOnlyList<string> SkippedRowReasons);
