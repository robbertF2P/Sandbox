using Akka.Actor;
using Akka.Event;
using System;

namespace Infrastructure.Akka.Actors.Session
{
    /// <summary>
    /// Owns a reusable session: login on demand, stash while authenticating, idle logout, and dispatch authenticated work.
    /// </summary>
    public sealed class SessionGateActor : ReceiveActor, IWithUnboundedStash, ISessionGateHost
    {
        private readonly ISessionGateBehavior _behavior;
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private object _sessionContext;
        private object _loginPendingCommand;
        private ICancelable _idleSchedule;

        public SessionGateActor(ISessionGateBehavior behavior)
        {
            _behavior = behavior;
            _behavior.Attach(this);
            RegisterIdleTimeoutHandler();
        }

        public IStash Stash { get; set; }

        IActorRef ISessionGateHost.Self => Self;

        IActorRef ISessionGateHost.Parent => Context.Parent;

        ILoggingAdapter ISessionGateHost.Log => _log;

        ActorSystem ISessionGateHost.ActorSystem => Context.System;

        object ISessionGateHost.SessionContext
        {
            get => _sessionContext;
            set => _sessionContext = value;
        }

        bool ISessionGateHost.IsLoggingIn => _loginPendingCommand != null;

        public static Props Props(ISessionGateBehavior behavior)
        {
            return global::Akka.Actor.Props.Create(() => new SessionGateActor(behavior));
        }

        protected override void PostStop()
        {
            CancelIdleLogoutInternal();
            if (_sessionContext != null)
            {
                _behavior.Logout(_sessionContext);
            }

            base.PostStop();
        }

        void ISessionGateHost.Receive<T>(Action<T> handler)
        {
            Receive(handler);
        }

        void ISessionGateHost.OnWork<T>(Action<T> whenReady)
        {
            Receive<T>(command =>
            {
                if (_loginPendingCommand != null)
                {
                    Stash.Stash();
                    return;
                }

                whenReady(command);
            });
        }

        void ISessionGateHost.Stash()
        {
            Stash.Stash();
        }

        void ISessionGateHost.UnstashAll()
        {
            Stash.UnstashAll();
        }

        void ISessionGateHost.CompleteLogin(object sessionContext)
        {
            _sessionContext = sessionContext;
            _log.Info("Session established");
            var pending = _loginPendingCommand;
            _loginPendingCommand = null;
            _behavior.Dispatch(_sessionContext, pending);
            UnstashAllInternal();
        }

        void ISessionGateHost.FailLogin(string reason)
        {
            _log.Warning("Session login failed: {0}", reason);
            var pending = _loginPendingCommand;
            _loginPendingCommand = null;
            _behavior.ReplyWorkFailed(pending, reason);
            UnstashAllInternal();
        }

        void ISessionGateHost.WatchWorker(IActorRef worker)
        {
            Context.Watch(worker);
        }

        void ISessionGateHost.ScheduleIdleLogout(object sessionToken)
        {
            if (_sessionContext == null)
            {
                return;
            }

            CancelIdleLogoutInternal();
            _idleSchedule = Context.System.Scheduler.ScheduleTellOnceCancelable(
                _behavior.IdleTimeout,
                Self,
                new SessionIdleTimeoutElapsed(sessionToken),
                Self);
        }

        void ISessionGateHost.CancelIdleLogout()
        {
            CancelIdleLogoutInternal();
        }

        private void CancelIdleLogoutInternal()
        {
            _idleSchedule?.Cancel();
            _idleSchedule = null;
        }

        private void UnstashAllInternal()
        {
            Stash.UnstashAll();
        }

        IActorRef ISessionGateHost.ActorOf(Props props)
        {
            return Context.ActorOf(props);
        }

        void ISessionGateHost.BeginLogin(object work)
        {
            CancelIdleLogoutInternal();

            if (_sessionContext != null)
            {
                _behavior.Dispatch(_sessionContext, work);
                return;
            }

            if (_loginPendingCommand != null)
            {
                _behavior.ReplyWorkFailed(work, "Login is already in progress.");
                return;
            }

            _loginPendingCommand = work;
            var loginWorker = Context.ActorOf(_behavior.CreateLoginWorker());
            loginWorker.Tell(_behavior.CreateLoginRequest());
        }

        private void RegisterIdleTimeoutHandler()
        {
            Receive<SessionIdleTimeoutElapsed>(message =>
            {
                if (_sessionContext != null && _behavior.SessionsEqual(_sessionContext, message.SessionToken))
                {
                    _behavior.Logout(_sessionContext);
                    _sessionContext = null;
                }
            });
        }

        private sealed record SessionIdleTimeoutElapsed(object SessionToken);
    }
}
