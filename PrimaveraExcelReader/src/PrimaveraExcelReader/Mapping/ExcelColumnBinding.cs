namespace PrimaveraExcelReader.Mapping;

public sealed class ExcelColumnBinding<T>
{
    public ExcelColumnBinding(
        string headerName,
        Action<T, string?> setter,
        int? columnIndex = null,
        bool required = false)
    {
        HeaderName = headerName;
        Setter = setter;
        ColumnIndex = columnIndex;
        Required = required;
    }

    public string HeaderName { get; }

    public int? ColumnIndex { get; }

    public bool Required { get; }

    public Action<T, string?> Setter { get; }

    public void Apply(T target, Abstractions.ExcelRowData row)
    {
        string? value = ColumnIndex.HasValue
            ? row.GetByIndex(ColumnIndex.Value)
            : row.GetByHeader(HeaderName);

        if (Required && string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required column '{HeaderName}' is missing or empty on row {row.RowIndex + 1}.");
        }

        Setter(target, value);
    }
}
