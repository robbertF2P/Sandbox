using PrimaveraExcelReader.Abstractions;

namespace PrimaveraExcelReader.Mapping;

public sealed class ExcelReaderService(IExcelWorkbookAccessor workbookAccessor) : IExcelReaderService
{
    public async Task<ExcelReadResult<T>> ReadAsync<T>(
        Stream stream,
        ExcelSheetProfile<T> profile,
        CancellationToken cancellationToken = default)
        where T : new()
    {
        var issues = new List<ExcelReadIssue>();
        IReadOnlyList<ExcelRowData> rows;

        try
        {
            rows = await workbookAccessor.ReadSheetAsync(
                stream,
                profile.SheetName,
                profile.HeaderRowIndex,
                profile.DataStartRowIndex,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            issues.Add(ExcelReadIssue.ForSheet(profile.SheetName, ex.Message, ClassifyWorkbookIssue(ex.Message)));
            return new ExcelReadResult<T>(profile.SheetName, [], issues);
        }

        var mappedRows = new List<T>();

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

        return new ExcelReadResult<T>(profile.SheetName, mappedRows, issues);
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

    private static ExcelReadIssueKind ClassifyWorkbookIssue(string message)
    {
        if (message.Contains("Sheet", StringComparison.OrdinalIgnoreCase)
            && message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return ExcelReadIssueKind.SheetNotFound;
        }

        if (message.Contains("Header row", StringComparison.OrdinalIgnoreCase))
        {
            return ExcelReadIssueKind.HeaderRowMissing;
        }

        return ExcelReadIssueKind.MappingError;
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
