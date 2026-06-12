using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiImportActorPoc.Contracts.Values.Json;

public sealed class DurationDaysJsonConverter : JsonConverter<DurationDays>
{
    public override DurationDays Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DurationDays.From(reader.GetDecimal());

    public override void Write(Utf8JsonWriter writer, DurationDays value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);
}
