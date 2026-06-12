using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiImportActorPoc.Contracts.Values.Json;

public sealed class LagDaysJsonConverter : JsonConverter<LagDays>
{
    public override LagDays Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        LagDays.From(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, LagDays value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Value);
}
