using Moq;
using PrimaveraExcelReader.Abstractions;
using PrimaveraExcelReader.Mapping;
using PrimaveraExcelReader.Npoi;

namespace PrimaveraExcelReader.Tests;

public static class ExcelReaderTestRunner
{
    private static readonly IExcelWorkbookAccessor DefaultAccessor = new NpoiExcelWorkbookAccessor();

    public static ExcelReaderService CreateService(IExcelWorkbookAccessor? accessor = null) =>
        new(accessor ?? DefaultAccessor, ExcelReaderTestLogging.CreateLogger<ExcelReaderService>());

    public static async Task<ExcelReadResult<T>> ReadSheetAsync<T>(
        TestSheetDefinition sheet,
        ExcelSheetProfile<T> profile,
        IExcelWorkbookAccessor? accessor = null,
        CancellationToken cancellationToken = default)
        where T : new()
    {
        accessor ??= DefaultAccessor;

        await using MemoryStream stream = new ExcelWorkbookTestBuilder()
            .AddSheet(sheet)
            .BuildStream();

        return await CreateService(accessor).ReadAsync(stream, profile, cancellationToken);
    }

    public static Task<ExcelReadResult<T>> ReadRowsAsync<T>(
        ExcelSheetProfile<T> profile,
        params ExcelRowData[] rows)
        where T : new()
    {
        Mock<IExcelWorkbookAccessor> accessorMock = ExcelReaderMoqExtensions.CreateWorkbookAccessorMock(
            profile.SheetName,
            rows);

        return CreateService(accessorMock.Object).ReadAsync(Stream.Null, profile);
    }
}
