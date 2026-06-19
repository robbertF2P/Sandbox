using ApiImportActorPoc.Core.Excel.Abstractions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ApiImportActorPoc.Core.Excel.Npoi;

public sealed class NpoiExcelWorkbookAccessor : IExcelWorkbookAccessor
{
    public Task<IReadOnlyList<string>> GetSheetNamesAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using IWorkbook workbook = OpenWorkbook(stream);
        var sheetNames = new List<string>();

        for (int index = 0; index < workbook.NumberOfSheets; index++)
        {
            sheetNames.Add(workbook.GetSheetName(index));
        }

        return Task.FromResult<IReadOnlyList<string>>(sheetNames);
    }

    public Task<IReadOnlyList<ExcelRowData>> ReadSheetAsync(
        Stream stream,
        string sheetName,
        int headerRowIndex,
        int dataStartRowIndex,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using IWorkbook workbook = OpenWorkbook(stream);
        ISheet? sheet = workbook.GetSheet(sheetName)
            ?? throw new InvalidOperationException($"Sheet '{sheetName}' was not found in the workbook.");

        IRow headerRow = sheet.GetRow(headerRowIndex)
            ?? throw new InvalidOperationException(
                $"Header row {headerRowIndex + 1} was not found on sheet '{sheetName}'.");

        IReadOnlyList<string> headers = ReadHeaders(headerRow);
        var rows = new List<ExcelRowData>();

        for (int rowIndex = dataStartRowIndex; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IRow? row = sheet.GetRow(rowIndex);
            if (row is null)
            {
                continue;
            }

            IReadOnlyList<string?> cellsByIndex = ReadCells(row, headers.Count);
            var cellsByHeader = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (int columnIndex = 0; columnIndex < headers.Count; columnIndex++)
            {
                cellsByHeader[headers[columnIndex]] = cellsByIndex[columnIndex];
            }

            rows.Add(new ExcelRowData(rowIndex, cellsByHeader, cellsByIndex));
        }

        return Task.FromResult<IReadOnlyList<ExcelRowData>>(rows);
    }

    private static IWorkbook OpenWorkbook(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return new XSSFWorkbook(stream);
    }

    private static IReadOnlyList<string> ReadHeaders(IRow headerRow)
    {
        var headers = new List<string>();

        for (int columnIndex = 0; columnIndex < headerRow.LastCellNum; columnIndex++)
        {
            ICell? cell = headerRow.GetCell(columnIndex);
            string header = cell?.ToString()?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(header))
            {
                header = $"Column{columnIndex + 1}";
            }

            headers.Add(header);
        }

        return headers;
    }

    private static IReadOnlyList<string?> ReadCells(IRow row, int columnCount)
    {
        var cells = new List<string?>(columnCount);

        for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            ICell? cell = row.GetCell(columnIndex);
            cells.Add(cell?.ToString()?.Trim());
        }

        return cells;
    }
}
