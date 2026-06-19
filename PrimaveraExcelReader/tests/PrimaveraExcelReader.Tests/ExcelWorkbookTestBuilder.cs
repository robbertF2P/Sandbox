using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace PrimaveraExcelReader.Tests;

public sealed class ExcelWorkbookTestBuilder : IDisposable
{
    private readonly XSSFWorkbook _workbook = new();
    private bool _disposed;

    public ExcelWorkbookTestBuilder AddSheet(string sheetName, params string[][] rows)
    {
        ISheet sheet = _workbook.CreateSheet(sheetName);

        for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            IRow row = sheet.CreateRow(rowIndex);
            string[] values = rows[rowIndex];

            for (int columnIndex = 0; columnIndex < values.Length; columnIndex++)
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
