using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Akka.Actors.Workers
{
    public sealed class PagedFetchOptions<TItem>
    {
        public required Func<int, Task<IReadOnlyList<TItem>>> FetchPage { get; init; }

        public required int PageSize { get; init; }

        public required Func<IReadOnlyList<TItem>, object> BuildSuccessReply { get; init; }

        public Func<Exception, object> BuildFailureReply { get; init; }

        public Func<TItem, object> DedupeKey { get; init; }

        public Func<IReadOnlyList<TItem>, IReadOnlyList<TItem>, IReadOnlyList<TItem>> MergePages { get; init; }
    }
}
