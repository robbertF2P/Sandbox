#nullable enable

using System.Text.Json.Serialization;

namespace Floor2Plan.Connectors.P6.Api.Models
{
    public sealed class P6ActivityRecord : P6BaseRecord
    {
        [JsonPropertyName("Id")]
        public string? Id { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("ProjectObjectId")]
        public int? ProjectObjectId { get; set; }

        [JsonPropertyName("StartDate")]
        public string? StartDate { get; set; }

        [JsonPropertyName("FinishDate")]
        public string? FinishDate { get; set; }
    }
}
