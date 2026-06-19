using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace PrimaveraExcelReader.Tests;

public sealed class ExcelWorkbookTestBuilder : IDisposable
{
    private readonly XSSFWorkbook _workbook = new();
    private bool _disposed;

    public ExcelWorkbookTestBuilder AddSheet(string sheetName, params string[][] rows) =>
        AddSheet(new TestSheetDefinition(sheetName, rows));

    public ExcelWorkbookTestBuilder AddSheet(TestSheetDefinition sheet)
    {
        ISheet worksheet = _workbook.CreateSheet(sheet.Name);

        for (int rowIndex = 0; rowIndex < sheet.Rows.Count; rowIndex++)
        {
            IRow row = worksheet.CreateRow(rowIndex);
            IReadOnlyList<string> values = sheet.Rows[rowIndex];

            for (int columnIndex = 0; columnIndex < values.Count; columnIndex++)
            {
                row.CreateCell(columnIndex).SetCellValue(values[columnIndex]);
            }
        }

        return this;
    }

    public MemoryStream BuildStream()
    {
        var stream = new MemoryStream();
        _workbook.Write(stream, leaveOpen: true);
        stream.Position = 0;
        return stream;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _workbook.Dispose();
        _disposed = true;
    }
}
