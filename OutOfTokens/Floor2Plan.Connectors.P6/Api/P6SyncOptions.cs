using Floor2Plan.Connectors.P6.Api.Models;
using System;

namespace Floor2Plan.Connectors.P6.Api
{
    public sealed class P6SyncOptions
    {
        public const string SectionName = "P6Sync";

        public int MaxConcurrency { get; set; } = 4;

        public int PageSize { get; set; } = IP6RestApi.CatalogPageSize;

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>When set, only these catalogs are fetched for every selected project. Omit for full sync.</summary>
        public P6EntityKind[] EntityKinds { get; set; }

        /// <summary>Optional P6 REST filter clause appended to every catalog fetch (e.g. delta since last sync).</summary>
        public string AdditionalFilter { get; set; }
    }
}
