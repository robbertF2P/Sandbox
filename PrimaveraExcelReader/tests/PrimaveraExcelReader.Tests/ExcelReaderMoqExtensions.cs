using Moq;
using PrimaveraExcelReader.Abstractions;

namespace PrimaveraExcelReader.Tests;

public static class ExcelReaderMoqExtensions
{
    public static Mock<IExcelWorkbookAccessor> CreateWorkbookAccessorMock(
        string sheetName,
        params ExcelRowData[] rows)
    {
        var mock = new Mock<IExcelWorkbookAccessor>();

        mock.Setup(accessor => accessor.GetSheetNamesAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([sheetName]);

        mock.Setup(accessor => accessor.ReadSheetAsync(
                It.IsAny<Stream>(),
                sheetName,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rows);

        return mock;
    }

    public static ExcelRowData CreateRow(int rowIndex, params (string Header, string? Value)[] cells)
    {
        var cellsByHeader = cells.ToDictionary(
            cell => cell.Header,
            cell => cell.Value,
            StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<string?> cellsByIndex = cells.Select(cell => cell.Value).ToArray();
        return new ExcelRowData(rowIndex, cellsByHeader, cellsByIndex);
    }
}
