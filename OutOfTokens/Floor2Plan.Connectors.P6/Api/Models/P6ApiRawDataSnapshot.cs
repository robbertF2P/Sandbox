#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Floor2Plan.Connectors.P6.Api.Models
{
    public sealed class P6ApiRawDataSnapshot
    {
        [JsonPropertyName("source")]
        public string Source { get; set; } = "p6-eppm-project-catalog";

        [JsonPropertyName("generatedUtc")]
        public string GeneratedUtc { get; set; } = DateTimeOffset.UtcNow.ToString("O");

        [JsonPropertyName("projects")]
        public List<P6ProjectRecord> Projects { get; set; } = [];
    }
}
