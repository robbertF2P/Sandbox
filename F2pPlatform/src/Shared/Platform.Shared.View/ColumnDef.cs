namespace Platform.Shared.View;

public sealed record ColumnDef(
    string Id,
    string LabelKey,
    ColumnSource Source,
    bool Visible,
    int Order,
    string? Format = null);
