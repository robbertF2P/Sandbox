using PrimaveraExcelReader.Abstractions;

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

    public ExcelRowMapResult<T> TryMapRow(ExcelRowData row)
    {
        var issues = new List<ExcelReadIssue>();
        var model = new T();

        foreach (ExcelColumnBinding<T> binding in ColumnBindings)
        {
            binding.TryApply(model, row, issues);
        }

        if (issues.Count > 0)
        {
            return ExcelRowMapResult<T>.Failure(issues);
        }

        if (AfterMap is not null)
        {
            try
            {
                model = AfterMap(model, row);
            }
            catch (Exception ex)
            {
                issues.Add(ExcelReadIssue.MappingError(row.RowIndex + 1, ex.Message));
                return ExcelRowMapResult<T>.Failure(issues);
            }
        }

        return ExcelRowMapResult<T>.Success(model);
    }
}
