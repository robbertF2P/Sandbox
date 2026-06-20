namespace ImportPipeline.Domain;

/// <summary>
/// Domain abstraction over a parsed spreadsheet row.
/// Keeps domain logic independent of NPOI / Excel I/O.
/// </summary>
public sealed class ImportRow
{
    private readonly IReadOnlyDictionary<string, string?> _cells;

    public ImportRow(IReadOnlyDictionary<string, string?> cells) => _cells = cells;

    /// Convenience factory for tests: ImportRow.From(("Status", "DONE"), ("Project ID", "P001"))
    public static ImportRow From(params (string Column, string? Value)[] cells) =>
        new(cells.ToDictionary(
            c => c.Column,
            c => c.Value,
            StringComparer.OrdinalIgnoreCase));

    public string? GetCell(string column) =>
        _cells.TryGetValue(column, out var value) ? value : null;

    public bool HasColumn(string column) =>
        _cells.ContainsKey(column);

    public IReadOnlyCollection<string> ColumnNames => _cells.Keys.ToList();
}
