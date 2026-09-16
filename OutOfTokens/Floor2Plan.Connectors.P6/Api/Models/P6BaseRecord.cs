using System.Text.Json.Serialization;

namespace Floor2Plan.Connectors.P6.Api.Models
{
    public class P6BaseRecord
    {
        [JsonPropertyName("ObjectId")]
        public int ObjectId { get; set; }
    }
}
