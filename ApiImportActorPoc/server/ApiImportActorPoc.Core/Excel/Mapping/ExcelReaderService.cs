using ApiImportActorPoc.Core.Excel.Abstractions;

namespace ApiImportActorPoc.Core.Excel.Mapping;

public sealed class ExcelReaderService(IExcelWorkbookAccessor workbookAccessor) : IExcelReaderService
{
    public async Task<ExcelReadResult<T>> ReadAsync<T>(
        Stream stream,
        ExcelSheetProfile<T> profile,
        CancellationToken cancellationToken = default)
        where T : new()
    {
        IReadOnlyList<ExcelRowData> rows = await workbookAccessor.ReadSheetAsync(
            stream,
            profile.SheetName,
            profile.HeaderRowIndex,
            profile.DataStartRowIndex,
            cancellationToken);

        var mappedRows = new List<T>();
        var skippedReasons = new List<string>();

        foreach (ExcelRowData row in rows)
        {
            if (row.IsEmpty())
            {
                skippedReasons.Add($"Row {row.RowIndex + 1}: empty row.");
                continue;
            }

            if (profile.RowFilter is not null && !profile.RowFilter(row))
            {
                skippedReasons.Add($"Row {row.RowIndex + 1}: filtered out.");
                continue;
            }

            try
            {
                mappedRows.Add(profile.MapRow(row));
            }
            catch (Exception ex)
            {
                skippedReasons.Add($"Row {row.RowIndex + 1}: {ex.Message}");
            }
        }

        return new ExcelReadResult<T>(profile.SheetName, mappedRows, skippedReasons);
    }

    public async Task<IReadOnlyDictionary<string, ExcelReadResult<T>>> ReadManyAsync<T>(
        Stream stream,
        IReadOnlyList<ExcelSheetProfile<T>> profiles,
        CancellationToken cancellationToken = default)
        where T : new()
    {
        var results = new Dictionary<string, ExcelReadResult<T>>(StringComparer.OrdinalIgnoreCase);

        foreach (ExcelSheetProfile<T> profile in profiles)
        {
            await using MemoryStream sheetStream = CopyStream(stream);
            ExcelReadResult<T> result = await ReadAsync(sheetStream, profile, cancellationToken);
            results[profile.SheetName] = result;
        }

        return results;
    }

    private static MemoryStream CopyStream(Stream source)
    {
        if (source.CanSeek)
        {
            source.Position = 0;
        }

        var copy = new MemoryStream();
        source.CopyTo(copy);
        copy.Position = 0;

        if (source.CanSeek)
        {
            source.Position = 0;
        }

        return copy;
    }
}
