using Akka.Actor;
using AwesomeAssertions;
using Infrastructure.Akka.Actors.State;
using System;
using System.Linq;

namespace Infrastructure.Akka.Tests.State
{
    public sealed class KeyedAccumulatorActorTests : AkkaActorTestKit
    {
        public KeyedAccumulatorActorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task Clears_appends_and_queries_by_key()
        {
            var actor = Sys.ActorOf(KeyedAccumulatorActor<string, int>.Props());

            actor.Tell(new KeyedAccumulatorActor<string, int>.Append("projects", new[] { 1, 2 }));
            actor.Tell(new KeyedAccumulatorActor<string, int>.Append("projects", new[] { 3 }));
            actor.Tell(new KeyedAccumulatorActor<string, int>.Append("wbs", new[] { 10 }));

            var projectsProbe = CreateTestProbe();
            actor.Tell(new KeyedAccumulatorActor<string, int>.Get("projects"), projectsProbe.Ref);
            var projects = await projectsProbe.ExpectMsgAsync<KeyedAccumulatorActor<string, int>.GetResult>(
                TimeSpan.FromSeconds(3));
            projects.Items.Should().Equal(1, 2, 3);

            var countsProbe = CreateTestProbe();
            actor.Tell(new KeyedAccumulatorActor<string, int>.GetCounts(), countsProbe.Ref);
            var counts = await countsProbe.ExpectMsgAsync<KeyedAccumulatorActor<string, int>.CountsResult>(
                TimeSpan.FromSeconds(3));
            counts.Counts["projects"].Should().Be(3);
            counts.Counts["wbs"].Should().Be(1);

            actor.Tell(new KeyedAccumulatorActor<string, int>.Clear());

            var afterClearProbe = CreateTestProbe();
            actor.Tell(new KeyedAccumulatorActor<string, int>.Get("projects"), afterClearProbe.Ref);
            var afterClear = await afterClearProbe.ExpectMsgAsync<KeyedAccumulatorActor<string, int>.GetResult>(
                TimeSpan.FromSeconds(3));
            afterClear.Items.Should().BeEmpty();
        }
    }
}
