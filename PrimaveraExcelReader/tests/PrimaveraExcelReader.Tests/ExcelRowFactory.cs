using PrimaveraExcelReader.Abstractions;

namespace PrimaveraExcelReader.Tests;

public static class ExcelRowFactory
{
    public static ExcelRowData FromCells(int rowIndex, params (string Header, string? Value)[] cells)
    {
        var cellsByHeader = cells.ToDictionary(
            cell => cell.Header,
            cell => cell.Value,
            StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<string?> cellsByIndex = cells.Select(cell => cell.Value).ToArray();
        return new ExcelRowData(rowIndex, cellsByHeader, cellsByIndex);
    }

    public static ExcelRowData FromHeaderRowAndValues(
        int rowIndex,
        IReadOnlyList<string> headers,
        IReadOnlyList<string?> values)
    {
        var cellsByHeader = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < headers.Count; index++)
        {
            cellsByHeader[headers[index]] = index < values.Count ? values[index] : null;
        }

        return new ExcelRowData(rowIndex, cellsByHeader, values);
    }
}
