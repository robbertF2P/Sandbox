#nullable enable

using System.Text.Json.Serialization;

namespace Floor2Plan.Connectors.P6.Api.Models
{
    public sealed class P6ResourceRecord : P6BaseRecord
    {
        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Type")]
        public string? Type { get; set; }
    }
}
