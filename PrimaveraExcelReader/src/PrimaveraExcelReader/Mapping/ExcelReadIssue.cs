namespace PrimaveraExcelReader.Mapping;

public sealed record ExcelReadIssue(
    int RowNumber,
    string? FieldName,
    string? ColumnHeader,
    int? ColumnIndex,
    string? RawValue,
    string Message,
    ExcelReadIssueKind Kind)
{
    public static ExcelReadIssue ForSheet(string sheetName, string message, ExcelReadIssueKind kind)
    {
        return new ExcelReadIssue(0, null, sheetName, null, null, message, kind);
    }

    public static ExcelReadIssue EmptyRow(int rowNumber)
    {
        return new ExcelReadIssue(rowNumber, null, null, null, null, "Row is empty.", ExcelReadIssueKind.EmptyRow);
    }

    public static ExcelReadIssue FilteredOut(int rowNumber)
    {
        return new ExcelReadIssue(rowNumber, null, null, null, null, "Row was filtered out.", ExcelReadIssueKind.FilteredOut);
    }

    public static ExcelReadIssue RequiredValueMissing(int rowNumber, string columnHeader, int? columnIndex)
    {
        return new ExcelReadIssue(
            rowNumber,
            columnHeader,
            columnHeader,
            columnIndex,
            null,
            $"Required column '{columnHeader}' is missing or empty.",
            ExcelReadIssueKind.RequiredValueMissing);
    }

    public static ExcelReadIssue ParseError(
        int rowNumber,
        string fieldName,
        string columnHeader,
        int? columnIndex,
        string? rawValue,
        Type targetType)
    {
        string displayValue = rawValue is null ? "<null>" : $"'{rawValue}'";
        return new ExcelReadIssue(
            rowNumber,
            fieldName,
            columnHeader,
            columnIndex,
            rawValue,
            $"Could not parse {displayValue} into {targetType.Name} for '{columnHeader}'.",
            ExcelReadIssueKind.ParseError);
    }

    public static ExcelReadIssue MappingError(int rowNumber, string message)
    {
        return new ExcelReadIssue(rowNumber, null, null, null, null, message, ExcelReadIssueKind.MappingError);
    }
}
