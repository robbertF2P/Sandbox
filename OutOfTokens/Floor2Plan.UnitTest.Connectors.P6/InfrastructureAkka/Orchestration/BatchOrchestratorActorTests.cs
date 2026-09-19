using Akka.Actor;
using AwesomeAssertions;
using Infrastructure.Akka.Actors.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Akka.Tests.Orchestration
{
    public sealed class BatchOrchestratorActorTests : AkkaActorTestKit
    {
        public BatchOrchestratorActorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task Runs_all_items_and_replies_when_queue_is_drained()
        {
            var completed = new List<int>();

            var orchestrator = Sys.ActorOf(BatchOrchestratorActor<int>.Props(new BatchOrchestratorOptions<int>
            {
                MaxConcurrency = 2,
                CreateWorker = item => (
                    Props.Create(() => new ImmediateSuccessWorker(item)),
                    new ImmediateSuccessWorker.Start()),
                IsSuccess = (item, message) => message is ItemSucceeded succeeded && succeeded.Id == item,
                IsFailure = (_, _) => false,
                OnSuccess = (item, _, _) => completed.Add(item),
                OnFailure = (_, _, _) => { },
                BuildReply = context => new BatchDone(context.Errors.ToList())
            }));

            var probe = CreateTestProbe();
            orchestrator.Tell(new BatchOrchestratorActor<int>.Start(new[] { 1, 2, 3, 4, 5 }, probe.Ref));

            var result = await probe.ExpectMsgAsync<BatchDone>(TimeSpan.FromSeconds(5));
            result.Errors.Should().BeEmpty();
            completed.Should().BeEquivalentTo(new[] { 1, 2, 3, 4, 5 });
        }

        [Fact]
        public async Task Collects_failures_and_still_completes_batch()
        {
            var orchestrator = Sys.ActorOf(BatchOrchestratorActor<int>.Props(new BatchOrchestratorOptions<int>
            {
                MaxConcurrency = 2,
                CreateWorker = item => (
                    Props.Create(() => new ConditionalWorker(item)),
                    new ConditionalWorker.Start()),
                IsSuccess = (item, message) => message is ItemSucceeded succeeded && succeeded.Id == item,
                IsFailure = (item, message) => message is ItemFailed failed && failed.Id == item,
                OnSuccess = (_, _, _) => { },
                OnFailure = (_, message, context) =>
                {
                    var failed = (ItemFailed)message;
                    context.Errors.Add($"item {failed.Id} failed");
                },
                BuildReply = context => new BatchDone(context.Errors.ToList())
            }));

            var probe = CreateTestProbe();
            orchestrator.Tell(new BatchOrchestratorActor<int>.Start(new[] { 1, 2, 3 }, probe.Ref));

            var result = await probe.ExpectMsgAsync<BatchDone>(TimeSpan.FromSeconds(5));
            result.Errors.Should().Contain("item 2 failed");
            result.Errors.Should().HaveCount(1);
        }

        private sealed record BatchDone(IReadOnlyList<string> Errors);

        private sealed record ItemSucceeded(int Id);

        private sealed record ItemFailed(int Id);

        private sealed class ImmediateSuccessWorker : ReceiveActor
        {
            public ImmediateSuccessWorker(int id)
            {
                Receive<Start>(_ => Context.Parent.Tell(new ItemSucceeded(id)));
            }

            public sealed record Start();
        }

        private sealed class ConditionalWorker : ReceiveActor
        {
            public ConditionalWorker(int id)
            {
                Receive<Start>(_ =>
                {
                    if (id == 2)
                    {
                        Context.Parent.Tell(new ItemFailed(id));
                        return;
                    }

                    Context.Parent.Tell(new ItemSucceeded(id));
                });
            }

            public sealed record Start();
        }
    }
}
