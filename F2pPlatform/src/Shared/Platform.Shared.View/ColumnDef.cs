namespace Platform.Shared.View;

public sealed record ColumnDef(
    string Id,
    string Label,
    ColumnSource Source,
    bool Visible,
    int Order,
    string? Format = null);
