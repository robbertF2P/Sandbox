namespace Platform.Shared.View;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ColumnSource
{
    Core,
    Extension,
    Computed,
}
