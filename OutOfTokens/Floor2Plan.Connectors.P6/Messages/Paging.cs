using Floor2Plan.Connectors.P6.Actors;
using Floor2Plan.Connectors.P6.Api.Models;
using System.Collections.Generic;

namespace Floor2Plan.Connectors.P6.Messages
{
    public sealed record Paging : IP6Paging
    {
        public IEnumerable<P6ProjectRecord> Projects { get; set; }
        public int Offset { get; set; }
        public P6WorkflowKind Kind { get; set; }
    }
}