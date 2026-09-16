#nullable enable

using System.Text.Json.Serialization;

namespace Floor2Plan.Connectors.P6.Api.Models
{
    public sealed class P6WbsRecord : P6BaseRecord
    {
        [JsonPropertyName("ProjectObjectId")]
        public int? ProjectObjectId { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("WbsCode")]
        public string? WbsCode { get; set; }
    }
}
