using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;

namespace PrimaveraExcelReader.Tests;

public static class RowMappingPipeline
{
    public static (IReadOnlyList<T> Rows, IReadOnlyList<ExcelReadIssue> Issues) MapRows<T>(
        ExcelSheetProfile<T> profile,
        IEnumerable<ExcelRowData> rows)
        where T : new()
    {
        var mappedRows = new List<T>();
        var issues = new List<ExcelReadIssue>();

        foreach (ExcelRowData row in rows)
        {
            if (row.IsEmpty())
            {
                issues.Add(ExcelReadIssue.EmptyRow(row.RowIndex + 1));
                continue;
            }

            if (profile.RowFilter is not null && !profile.RowFilter(row))
            {
                issues.Add(ExcelReadIssue.FilteredOut(row.RowIndex + 1));
                continue;
            }

            ExcelRowMapResult<T> mapResult = profile.TryMapRow(row);
            if (mapResult.IsSuccess)
            {
                mappedRows.Add(mapResult.Row!);
                continue;
            }

            issues.AddRange(mapResult.Issues);
        }

        return (mappedRows, issues);
    }
}
