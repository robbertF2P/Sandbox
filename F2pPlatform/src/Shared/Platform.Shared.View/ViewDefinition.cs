namespace Platform.Shared.View;

public sealed record ViewDefinition(
    string ScreenId,
    IReadOnlyList<ColumnDef> Columns)
{
    public static ViewDefinition Empty(string screenId) => new(screenId, []);

    public ColumnDef? FindColumn(string columnId) =>
        Columns.FirstOrDefault(column => string.Equals(column.Id, columnId, StringComparison.Ordinal));

    public bool IsVisible(string columnId) => FindColumn(columnId)?.Visible ?? false;
}
