using Akka.Actor;
using AwesomeAssertions;
using Infrastructure.Akka.Actors.Guards;
using System;

namespace Infrastructure.Akka.Tests.Guards
{
    public sealed class ExclusiveGateActorTests : AkkaActorTestKit
    {
        public ExclusiveGateActorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task Rejects_second_begin_until_finished()
        {
            var gate = Sys.ActorOf(ExclusiveGateActor.Props(new ExclusiveGateOptions
            {
                OnBegin = (work, replyTo) => replyTo.Tell(new Begun((int)work)),
                BuildRejection = work => new Rejected((int)work)
            }));

            var first = CreateTestProbe();
            var second = CreateTestProbe();

            gate.Tell(new ExclusiveGateActor.Begin(1, first.Ref));
            (await first.ExpectMsgAsync<Begun>(TimeSpan.FromSeconds(3))).Id.Should().Be(1);

            gate.Tell(new ExclusiveGateActor.Begin(2, second.Ref));
            (await second.ExpectMsgAsync<Rejected>(TimeSpan.FromSeconds(3))).Id.Should().Be(2);

            gate.Tell(new ExclusiveGateActor.Finished());

            var third = CreateTestProbe();
            gate.Tell(new ExclusiveGateActor.Begin(3, third.Ref));
            (await third.ExpectMsgAsync<Begun>(TimeSpan.FromSeconds(3))).Id.Should().Be(3);
        }

        private sealed record Begun(int Id);

        private sealed record Rejected(int Id);
    }
}
