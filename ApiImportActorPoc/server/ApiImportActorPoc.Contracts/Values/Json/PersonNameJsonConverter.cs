using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiImportActorPoc.Contracts.Values.Json;

public sealed class PersonNameJsonConverter : JsonConverter<PersonName>
{
    public override PersonName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        PersonName.From(reader.GetString());

    public override void Write(Utf8JsonWriter writer, PersonName value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
