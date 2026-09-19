using Akka.Actor;
using AwesomeAssertions;
using Infrastructure.Akka.Actors.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Akka.Tests.Workers
{
    public sealed class PagedFetchActorTests : AkkaActorTestKit
    {
        public PagedFetchActorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task Fetches_all_pages_until_partial_page()
        {
            var pages = new Dictionary<int, IReadOnlyList<int>>
            {
                [0] = new[] { 1, 2 },
                [2] = new[] { 3, 4 },
                [4] = new[] { 5 }
            };

            var actor = Sys.ActorOf(PagedFetchActor<int>.Props(new PagedFetchOptions<int>
            {
                PageSize = 2,
                FetchPage = offset => Task.FromResult(pages[offset]),
                BuildSuccessReply = items => new FetchCompleted(items.ToList())
            }));

            var probe = CreateTestProbe();
            actor.Tell(new PagedFetchActor<int>.Start(probe.Ref));

            var result = await probe.ExpectMsgAsync<FetchCompleted>(TimeSpan.FromSeconds(3));
            result.Items.Should().Equal(1, 2, 3, 4, 5);
        }

        [Fact]
        public async Task Stops_when_dedupe_adds_no_new_items()
        {
            var callCount = 0;
            var actor = Sys.ActorOf(PagedFetchActor<int>.Props(new PagedFetchOptions<int>
            {
                PageSize = 2,
                DedupeKey = item => item,
                FetchPage = _ =>
                {
                    callCount++;
                    return Task.FromResult<IReadOnlyList<int>>(new[] { 1, 2 });
                },
                BuildSuccessReply = items => new FetchCompleted(items.ToList())
            }));

            var probe = CreateTestProbe();
            actor.Tell(new PagedFetchActor<int>.Start(probe.Ref));

            var result = await probe.ExpectMsgAsync<FetchCompleted>(TimeSpan.FromSeconds(3));
            result.Items.Should().Equal(1, 2);
            callCount.Should().Be(2);
        }

        [Fact]
        public async Task Replies_with_failure_when_fetch_throws()
        {
            var actor = Sys.ActorOf(PagedFetchActor<int>.Props(new PagedFetchOptions<int>
            {
                PageSize = 10,
                FetchPage = _ => Task.FromException<IReadOnlyList<int>>(new InvalidOperationException("network down")),
                BuildSuccessReply = items => new FetchCompleted(items.ToList())
            }));

            var probe = CreateTestProbe();
            actor.Tell(new PagedFetchActor<int>.Start(probe.Ref));

            var failure = await probe.ExpectMsgAsync<Status.Failure>(TimeSpan.FromSeconds(3));
            failure.Cause.Message.Should().Be("network down");
        }

        [Fact]
        public async Task Invokes_on_items_added_for_each_page()
        {
            var added = new List<int>();
            var actor = Sys.ActorOf(PagedFetchActor<int>.Props(new PagedFetchOptions<int>
            {
                PageSize = 2,
                FetchPage = offset => Task.FromResult<IReadOnlyList<int>>(
                    offset == 0 ? new[] { 1, 2 } : new[] { 3 }),
                OnItemsAdded = items => added.AddRange(items),
                BuildSuccessReply = items => new FetchCompleted(items.ToList())
            }));

            var probe = CreateTestProbe();
            actor.Tell(new PagedFetchActor<int>.Start(probe.Ref));
            await probe.ExpectMsgAsync<FetchCompleted>(TimeSpan.FromSeconds(3));

            added.Should().Equal(1, 2, 3);
        }

        private sealed record FetchCompleted(List<int> Items);
    }
}
