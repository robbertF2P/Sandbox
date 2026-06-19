namespace PrimaveraExcelReader.Mapping;

public sealed class ExcelColumnBinding<T>
{
    public ExcelColumnBinding(
        string headerName,
        Action<T, string?> setter,
        int? columnIndex = null,
        bool required = false,
        string? fieldName = null)
    {
        HeaderName = headerName;
        Setter = setter;
        ColumnIndex = columnIndex;
        Required = required;
        FieldName = fieldName ?? headerName;
    }

    public string HeaderName { get; }

    public string FieldName { get; }

    public int? ColumnIndex { get; }

    public bool Required { get; }

    public Action<T, string?> Setter { get; }

    public void TryApply(T target, Abstractions.ExcelRowData row, ICollection<ExcelReadIssue> issues)
    {
        string? value = ColumnIndex.HasValue
            ? row.GetByIndex(ColumnIndex.Value)
            : row.GetByHeader(HeaderName);

        if (Required && string.IsNullOrWhiteSpace(value))
        {
            issues.Add(ExcelReadIssue.RequiredValueMissing(row.RowIndex + 1, HeaderName, ColumnIndex));
            return;
        }

        try
        {
            Setter(target, value);
        }
        catch (ExcelCellParseException ex)
        {
            issues.Add(ExcelReadIssue.ParseError(
                row.RowIndex + 1,
                ex.FieldName,
                HeaderName,
                ColumnIndex,
                ex.RawValue,
                ex.TargetType));
        }
    }
}
