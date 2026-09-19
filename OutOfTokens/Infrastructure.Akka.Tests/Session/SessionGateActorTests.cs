using Akka.Actor;
using AwesomeAssertions;
using Infrastructure.Akka.Actors.Session;
using System;
using System.Collections.Generic;

namespace Infrastructure.Akka.Tests.Session
{
    public sealed class SessionGateActorTests : AkkaActorTestKit
    {
        public SessionGateActorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task Logs_in_on_first_work_and_dispatches_with_session()
        {
            var dispatched = new List<string>();
            var gate = Sys.ActorOf(SessionGateActor.Props(new TestSessionGateBehavior(dispatched, instantLogin: true)));

            var probe = CreateTestProbe();
            gate.Tell(new WorkCommand("fetch-catalog", probe.Ref));

            var result = await probe.ExpectMsgAsync<WorkCompleted>(TimeSpan.FromSeconds(3));
            result.SessionId.Should().Be("session-1");
            result.Command.Should().Be("fetch-catalog");
            dispatched.Should().Equal("fetch-catalog");
        }

        [Fact]
        public async Task Stashes_second_work_while_login_is_in_progress()
        {
            var dispatched = new List<string>();
            var gate = Sys.ActorOf(SessionGateActor.Props(new TestSessionGateBehavior(dispatched, instantLogin: false)));

            var first = CreateTestProbe();
            var second = CreateTestProbe();

            gate.Tell(new WorkCommand("first", first.Ref));
            gate.Tell(new WorkCommand("second", second.Ref));

            (await first.ExpectMsgAsync<WorkCompleted>(TimeSpan.FromSeconds(3))).Command.Should().Be("first");
            (await second.ExpectMsgAsync<WorkCompleted>(TimeSpan.FromSeconds(3))).Command.Should().Be("second");
            dispatched.Should().Equal("first", "second");
        }

        private sealed record WorkCommand(string Name, IActorRef ReplyTo);

        private sealed record WorkCompleted(string SessionId, string Command);

        private sealed record LoginRequested();

        private sealed record LoginSucceeded(string SessionId);

        private sealed class TestSessionGateBehavior : ISessionGateBehavior
        {
            private readonly List<string> _dispatched;
            private readonly bool _instantLogin;

            public TestSessionGateBehavior(List<string> dispatched, bool instantLogin)
            {
                _dispatched = dispatched;
                _instantLogin = instantLogin;
            }

            public TimeSpan IdleTimeout => TimeSpan.FromMinutes(5);

            public void Attach(ISessionGateHost host)
            {
                host.OnWork<WorkCommand>(work => host.BeginLogin(work));
                host.Receive<LoginSucceeded>(message =>
                {
                    if (host.IsLoggingIn)
                    {
                        host.CompleteLogin(message.SessionId);
                    }
                });
            }

            public Props CreateLoginWorker()
            {
                return _instantLogin
                    ? Props.Create(() => new InstantLoginWorker())
                    : Props.Create(() => new DelayedLoginWorker(TimeSpan.FromMilliseconds(100)));
            }

            public object CreateLoginRequest()
            {
                return new LoginRequested();
            }

            public void Dispatch(object sessionContext, object work)
            {
                var command = (WorkCommand)work;
                _dispatched.Add(command.Name);
                var sessionId = (string)sessionContext;
                command.ReplyTo.Tell(new WorkCompleted(sessionId, command.Name));
            }

            public void ReplyWorkFailed(object work, string reason)
            {
                var command = (WorkCommand)work;
                command.ReplyTo.Tell(new Status.Failure(new InvalidOperationException(reason)));
            }

            public void Logout(object sessionContext)
            {
            }

            public bool SessionsEqual(object sessionContext, object sessionToken)
            {
                return Equals(sessionContext, sessionToken);
            }
        }

        private sealed class InstantLoginWorker : ReceiveActor
        {
            public InstantLoginWorker()
            {
                Receive<LoginRequested>(_ => Sender.Tell(new LoginSucceeded("session-1")));
            }
        }

        private sealed class DelayedLoginWorker : ReceiveActor
        {
            public DelayedLoginWorker(TimeSpan delay)
            {
                Receive<LoginRequested>(_ =>
                {
                    Context.System.Scheduler.ScheduleTellOnce(
                        delay,
                        Sender,
                        new LoginSucceeded("session-1"),
                        Self);
                });
            }
        }
    }
}
