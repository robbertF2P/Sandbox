using PrimaveraExcelReader.Abstractions;

namespace PrimaveraExcelReader.Mapping;

public sealed class ExcelSheetProfile<T> where T : new()
{
    public required string SheetName { get; init; }

    public int HeaderRowIndex { get; init; }

    public int DataStartRowIndex { get; init; } = 1;

    public IReadOnlyList<ExcelColumnBinding<T>> ColumnBindings { get; init; } = [];

    public Func<ExcelRowData, bool>? RowFilter { get; init; }

    public Func<T, ExcelRowData, T>? AfterMap { get; init; }

    public T MapRow(ExcelRowData row)
    {
        var model = new T();

        foreach (ExcelColumnBinding<T> binding in ColumnBindings)
        {
            binding.Apply(model, row);
        }

        if (AfterMap is not null)
        {
            return AfterMap(model, row);
        }

        return model;
    }
}
