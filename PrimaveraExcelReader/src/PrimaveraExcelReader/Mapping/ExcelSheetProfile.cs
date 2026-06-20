using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.ImportPipeline;

namespace PrimaveraExcelReader.Mapping;

public sealed class ExcelSheetProfile<T> where T : new()
{
    public static ExcelSheetProfileBuilder<T> Configure() => ExcelSheetProfileBuilder<T>.Create();

    public required string SheetName { get; init; }

    public int HeaderRowIndex { get; init; }

    public int DataStartRowIndex { get; init; } = 1;

    public IReadOnlyList<ExcelColumnBinding<T>> ColumnBindings { get; init; } = [];

    public Func<ExcelRowData, bool>? RowFilter { get; init; }

    public Func<T, ExcelRowData, T>? AfterMap { get; init; }

    public ExcelRowMapResult<T> TryMapRow(ExcelRowData row) =>
        ImportPipelineRowMapping.TryMap(this, row);
}
