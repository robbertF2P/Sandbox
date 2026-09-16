using Floor2Plan.Connectors.P6.Api.Models;
using System.Collections.Generic;

namespace Floor2Plan.Connectors.P6.Messages
{
    public sealed record P6Projects(IEnumerable<P6ProjectRecord> Projects);
}
