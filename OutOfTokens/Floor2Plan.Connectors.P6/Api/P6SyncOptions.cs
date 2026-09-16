using System;

namespace Floor2Plan.Connectors.P6.Api
{
    public sealed class P6SyncOptions
    {
        public const string SectionName = "P6Sync";

        public int MaxConcurrency { get; set; } = 4;

        public int PageSize { get; set; } = IP6RestApi.CatalogPageSize;

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(5);
    }
}
