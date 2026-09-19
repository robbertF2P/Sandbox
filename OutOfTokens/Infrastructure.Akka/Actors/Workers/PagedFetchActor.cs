using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Akka.Actors.Workers
{
    /// <summary>
    /// Pages through a source until the last page is detected, then replies once with the accumulated result.
    /// </summary>
    public sealed class PagedFetchActor<TItem> : ReplyTargetWorkerActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly PagedFetchOptions<TItem> _options;

        public PagedFetchActor(PagedFetchOptions<TItem> options)
        {
            _options = options;
            Receive<Start>(StartFetch);
        }

        public static Props Props(PagedFetchOptions<TItem> options)
        {
            return global::Akka.Actor.Props.Create(() => new PagedFetchActor<TItem>(options));
        }

        public sealed record Start(IActorRef ReplyTo);

        private sealed record PageReceived(int Offset, List<TItem> Accumulated, IReadOnlyList<TItem> Page);

        private sealed record PageFailed(Exception Exception);

        private void StartFetch(Start message)
        {
            Begin(message.ReplyTo);
            Become(Paging);
            FetchPage(offset: 0, accumulated: []);
        }

        private void Paging()
        {
            Receive<PageReceived>(message =>
            {
                var previousCount = message.Accumulated.Count;
                var accumulated = Merge(message.Accumulated, message.Page);
                var addedCount = accumulated.Count - previousCount;
                if (addedCount > 0 && _options.OnItemsAdded != null)
                {
                    _options.OnItemsAdded(accumulated.Skip(previousCount).ToList());
                }

                if (message.Page.Count < _options.PageSize
                    || (_options.DedupeKey != null && addedCount == 0))
                {
                    Complete(_options.BuildSuccessReply(accumulated));
                    return;
                }

                FetchPage(message.Offset + _options.PageSize, accumulated);
            });

            Receive<PageFailed>(message =>
            {
                _log.Error(message.Exception, "Paged fetch failed");
                Complete(BuildFailureReply(message.Exception));
            });
        }

        private void FetchPage(int offset, List<TItem> accumulated)
        {
            _ = _options.FetchPage(offset)
                .PipeTo(
                    Self,
                    Self,
                    page => new PageReceived(offset, accumulated, page),
                    exception => new PageFailed(exception));
        }

        private object BuildFailureReply(Exception exception)
        {
            return _options.BuildFailureReply != null
                ? _options.BuildFailureReply(exception)
                : new Status.Failure(exception);
        }

        private List<TItem> Merge(List<TItem> accumulated, IReadOnlyList<TItem> page)
        {
            if (_options.MergePages != null)
            {
                return _options.MergePages(accumulated, page).ToList();
            }

            if (_options.DedupeKey == null)
            {
                accumulated.AddRange(page);
                return accumulated;
            }

            var seen = new HashSet<object>();
            foreach (var item in accumulated)
            {
                seen.Add(_options.DedupeKey(item));
            }

            foreach (var item in page)
            {
                if (seen.Add(_options.DedupeKey(item)))
                {
                    accumulated.Add(item);
                }
            }

            return accumulated;
        }
    }
}
