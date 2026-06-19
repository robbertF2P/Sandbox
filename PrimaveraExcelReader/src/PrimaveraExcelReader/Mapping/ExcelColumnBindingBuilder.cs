namespace PrimaveraExcelReader.Mapping;

public sealed class ExcelColumnBindingBuilder<T> where T : new()
{
    private readonly ExcelSheetProfileBuilder<T> _profileBuilder;
    private readonly Action<T, string?> _setter;
    private readonly string _propertyName;

    internal ExcelColumnBindingBuilder(
        ExcelSheetProfileBuilder<T> profileBuilder,
        Action<T, string?> setter,
        string propertyName)
    {
        _profileBuilder = profileBuilder;
        _setter = setter;
        _propertyName = propertyName;
    }

    public ExcelSheetProfileBuilder<T> From(string headerName, bool required = false)
    {
        return _profileBuilder.AddBinding(headerName, _setter, columnIndex: null, required, _propertyName);
    }

    public ExcelSheetProfileBuilder<T> AtColumn(int columnIndex, string? headerName = null, bool required = false)
    {
        return _profileBuilder.AddBinding(
            headerName ?? _propertyName,
            _setter,
            columnIndex: columnIndex,
            required: required,
            fieldName: _propertyName);
    }
}
