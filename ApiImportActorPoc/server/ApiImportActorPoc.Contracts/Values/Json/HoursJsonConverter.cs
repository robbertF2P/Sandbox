using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiImportActorPoc.Contracts.Values.Json;

public sealed class HoursJsonConverter : JsonConverter<Hours>
{
    public override Hours Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Hours.From(reader.GetDecimal());

    public override void Write(Utf8JsonWriter writer, Hours value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);
}
