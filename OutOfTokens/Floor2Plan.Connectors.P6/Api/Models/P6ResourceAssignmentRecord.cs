using System.Text.Json.Serialization;

namespace Floor2Plan.Connectors.P6.Api.Models
{
    public sealed class P6ResourceAssignmentRecord : P6BaseRecord
    {
        [JsonPropertyName("ActivityObjectId")]
        public int? ActivityObjectId { get; set; }

        [JsonPropertyName("ResourceObjectId")]
        public int? ResourceObjectId { get; set; }

        [JsonPropertyName("Units")]
        public double? Units { get; set; }
    }
}
