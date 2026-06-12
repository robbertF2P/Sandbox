using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiImportActorPoc.Contracts.Values.Json;

public sealed class ScheduleDateJsonConverter : JsonConverter<ScheduleDate>
{
    public override ScheduleDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        ScheduleDate.From(reader.GetString() ?? throw new JsonException("Schedule date is required."));

    public override void Write(Utf8JsonWriter writer, ScheduleDate value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
}
