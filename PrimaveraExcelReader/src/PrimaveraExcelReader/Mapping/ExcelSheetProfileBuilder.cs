using System.Linq.Expressions;
using PrimaveraExcelReader.Abstractions;

namespace PrimaveraExcelReader.Mapping;

public sealed class ExcelSheetProfileBuilder<T> where T : new()
{
    private readonly List<ExcelColumnBinding<T>> _columnBindings = [];
    private string? _sheetName;
    private int _headerRowIndex;
    private int _dataStartRowIndex = 1;
    private Func<ExcelRowData, bool>? _rowFilter;
    private Func<T, ExcelRowData, T>? _afterMap;

    public static ExcelSheetProfileBuilder<T> Create() => new();

    public ExcelSheetProfileBuilder<T> Sheet(string sheetName)
    {
        _sheetName = sheetName;
        return this;
    }

    public ExcelSheetProfileBuilder<T> HeaderRow(int headerRowIndex)
    {
        _headerRowIndex = headerRowIndex;
        return this;
    }

    public ExcelSheetProfileBuilder<T> DataStartsAt(int dataStartRowIndex)
    {
        _dataStartRowIndex = dataStartRowIndex;
        return this;
    }

    public ExcelColumnBindingBuilder<T> Map<TProperty>(Expression<Func<T, TProperty>> property)
    {
        return new ExcelColumnBindingBuilder<T>(
            this,
            ExpressionBindingHelper.CreateSetter(property),
            ExpressionBindingHelper.GetPropertyName(property));
    }

    public ExcelColumnBindingBuilder<T> MapOptional(Expression<Func<T, string?>> property) => Map(property);

    public ExcelSheetProfileBuilder<T> Where(Func<ExcelRowData, bool> rowFilter)
    {
        _rowFilter = rowFilter;
        return this;
    }

    public ExcelSheetProfileBuilder<T> AfterMap(Func<T, ExcelRowData, T> afterMap)
    {
        _afterMap = afterMap;
        return this;
    }

    public ExcelSheetProfileBuilder<T> Bind(
        string headerName,
        Action<T, string?> setter,
        bool required = false,
        int? columnIndex = null)
    {
        return AddBinding(headerName, setter, columnIndex, required);
    }

    internal ExcelSheetProfileBuilder<T> AddBinding(
        string headerName,
        Action<T, string?> setter,
        int? columnIndex,
        bool required)
    {
        _columnBindings.Add(new ExcelColumnBinding<T>(headerName, setter, columnIndex, required));
        return this;
    }

    public ExcelSheetProfile<T> Build()
    {
        if (string.IsNullOrWhiteSpace(_sheetName))
        {
            throw new InvalidOperationException("Sheet name is required. Call Sheet(name) before Build().");
        }

        return new ExcelSheetProfile<T>
        {
            SheetName = _sheetName,
            HeaderRowIndex = _headerRowIndex,
            DataStartRowIndex = _dataStartRowIndex,
            ColumnBindings = _columnBindings,
            RowFilter = _rowFilter,
            AfterMap = _afterMap
        };
    }
}
