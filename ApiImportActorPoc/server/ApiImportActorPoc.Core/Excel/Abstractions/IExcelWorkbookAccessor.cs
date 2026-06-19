namespace ApiImportActorPoc.Core.Excel.Abstractions;

public interface IExcelWorkbookAccessor
{
    Task<IReadOnlyList<string>> GetSheetNamesAsync(Stream stream, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExcelRowData>> ReadSheetAsync(
        Stream stream,
        string sheetName,
        int headerRowIndex,
        int dataStartRowIndex,
        CancellationToken cancellationToken = default);
}
