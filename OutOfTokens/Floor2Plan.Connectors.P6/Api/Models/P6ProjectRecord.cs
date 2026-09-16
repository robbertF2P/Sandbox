using System.Text.Json.Serialization;

namespace Floor2Plan.Connectors.P6.Api.Models
{
    public sealed class P6ProjectRecord : P6BaseRecord
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }

        [JsonPropertyName("LastUpdateDate")]
        public string LastUpdateDate { get; set; }

        [JsonPropertyName("DataDate")]
        public string DataDate { get; set; }

        [JsonPropertyName("SummaryActivityCount")]
        public int? SummaryActivityCount { get; set; }

        [JsonPropertyName("SummaryCompletedActivityCount")]
        public int? SummaryCompletedActivityCount { get; set; }

        [JsonPropertyName("SummaryInProgressActivityCount")]
        public int? SummaryInProgressActivityCount { get; set; }

        [JsonPropertyName("SummaryNotStartedActivityCount")]
        public int? SummaryNotStartedActivityCount { get; set; }

        /// <summary>
        /// Actual activity count fetched directly from the /activity endpoint for this project, used because
        /// SummaryActivityCount is only populated once P6 has run its "Summarize Project" job and can be null
        /// or stale otherwise. Only populated for the raw-data connectivity-check download.
        /// </summary>
        [JsonPropertyName("ComputedActivityCount")]
        public int? ComputedActivityCount { get; set; }
    }
}
