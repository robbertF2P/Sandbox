namespace ApiImportActorPoc.Core.Excel.Abstractions;

public sealed class ExcelRowData
{
    private readonly IReadOnlyDictionary<string, string?> _cellsByHeader;
    private readonly IReadOnlyList<string?> _cellsByIndex;

    public ExcelRowData(
        int rowIndex,
        IReadOnlyDictionary<string, string?> cellsByHeader,
        IReadOnlyList<string?> cellsByIndex)
    {
        RowIndex = rowIndex;
        _cellsByHeader = cellsByHeader;
        _cellsByIndex = cellsByIndex;
    }

    public int RowIndex { get; }

    public string? GetByHeader(string headerName)
    {
        if (_cellsByHeader.TryGetValue(headerName, out string? value))
        {
            return value;
        }

        foreach (KeyValuePair<string, string?> pair in _cellsByHeader)
        {
            if (string.Equals(pair.Key, headerName, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return null;
    }

    public string? GetByIndex(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= _cellsByIndex.Count)
        {
            return null;
        }

        return _cellsByIndex[columnIndex];
    }

    public bool IsEmpty()
    {
        return _cellsByIndex.All(cell => string.IsNullOrWhiteSpace(cell));
    }
}
