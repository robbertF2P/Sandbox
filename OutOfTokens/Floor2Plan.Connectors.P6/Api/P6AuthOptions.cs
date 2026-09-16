using System;

namespace Floor2Plan.Connectors.P6.Api
{
    public sealed class P6AuthOptions
    {
        public const string SectionName = "P6Auth";

        public string Username { get; set; }

        public string Password { get; set; }

        public string DatabaseName { get; set; }

        public TimeSpan SessionIdleTimeout { get; set; } = TimeSpan.FromMinutes(5);
    }
}
