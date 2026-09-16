using System.Text.Json.Serialization;

namespace Floor2Plan.Connectors.P6.Api.Models
{
    public sealed class P6RelationshipRecord : P6BaseRecord
    {
        [JsonPropertyName("PredecessorActivityObjectId")]
        public int? PredecessorActivityObjectId { get; set; }

        [JsonPropertyName("SuccessorActivityObjectId")]
        public int? SuccessorActivityObjectId { get; set; }

        [JsonPropertyName("PredecessorProjectObjectId")]
        public int? PredecessorProjectObjectId { get; set; }

        [JsonPropertyName("SuccessorProjectObjectId")]
        public int? SuccessorProjectObjectId { get; set; }
    }
}
