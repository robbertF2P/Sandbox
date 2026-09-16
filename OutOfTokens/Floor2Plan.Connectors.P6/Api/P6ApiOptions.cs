using System;

namespace Floor2Plan.Connectors.P6.Api
{
    public sealed class P6ApiOptions
    {
        public const string SectionName = "P6:Api";

        public string BaseUrl { get; set; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        public bool LogHttpTraffic { get; set; }

        public bool LogHttpBodies { get; set; }
    }
}
