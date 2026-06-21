using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PrimaveraExcelReader.Abstractions;

namespace PrimaveraExcelReader.Mapping;

public sealed class ExcelReaderService(
    IExcelWorkbookAccessor workbookAccessor,
    ILogger<ExcelReaderService>? logger = null) : IExcelReaderService
{
    private readonly ILogger<ExcelReaderService> _logger = logger ?? NullLogger<ExcelReaderService>.Instance;
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
            _logger.LogWarning(
                ex,
                "Failed to read sheet {SheetName}: {Message}",
                profile.SheetName,
                ex.Message);
            issues.Add(ExcelReadIssue.ForSheet(profile.SheetName, ex.Message, ClassifyWorkbookIssue(ex.Message)));
            return new ExcelReadResult<T>(profile.SheetName, [], issues);
        }

        _logger.LogInformation(
            "Read {RowCount} data rows from sheet {SheetName}",
            rows.Count,
            profile.SheetName);

        var mappedRows = new List<T>();

        foreach (ExcelRowData row in rows)
        {
            if (row.IsEmpty())
            {
                _logger.LogDebug("Skipping empty row {RowNumber} on sheet {SheetName}", row.RowIndex + 1, profile.SheetName);
                issues.Add(ExcelReadIssue.EmptyRow(row.RowIndex + 1));
                continue;
            }

            if (profile.RowFilter is not null && !profile.RowFilter(row))
            {
                _logger.LogDebug("Filtered out row {RowNumber} on sheet {SheetName}", row.RowIndex + 1, profile.SheetName);
                issues.Add(ExcelReadIssue.FilteredOut(row.RowIndex + 1));
                continue;
            }

            ExcelRowMapResult<T> mapResult = profile.TryMapRow(row, _logger);
            if (mapResult.IsSuccess)
            {
                mappedRows.Add(mapResult.Row!);
                continue;
            }

            _logger.LogWarning(
                "Row {RowNumber} on sheet {SheetName} produced {IssueCount} mapping issue(s)",
                row.RowIndex + 1,
                profile.SheetName,
                mapResult.Issues.Count);
            issues.AddRange(mapResult.Issues);
        }

        _logger.LogInformation(
            "Mapped sheet {SheetName}: {MappedRowCount} succeeded, {IssueCount} issue(s)",
            profile.SheetName,
            mappedRows.Count,
            issues.Count);

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
