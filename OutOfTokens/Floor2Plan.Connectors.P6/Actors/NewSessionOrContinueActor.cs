using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Messages;
using System;

namespace Floor2Plan.Connectors.P6.Actors
{
    internal class NewSessionOrContinueActor:ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly P6AuthOptions _authOptions;
        private readonly IP6RestApi _api;

        private string _sessionId = null;
        
        internal record Continue(string SessionId, IP6Request Payload, IActorRef originalSender);
        public NewSessionOrContinueActor(IP6RestApi api, P6AuthOptions authOptions)
        {
            _api = api;
            _authOptions = authOptions;
            Become(Idle);
        }

        private void Idle()
        {
            Receive<IP6Request>(msg =>
            {
                if (!string.IsNullOrEmpty(_sessionId))
                {
                    _log.Info("Session available, continue with it");
                    Sender.Tell(new Continue(_sessionId, msg, Sender));
                }
                else
                {
                    _log.Info("No Session available, going to login first");
                    var loginActor = Context.ActorOf(P6LoginActor.Props(_api, _authOptions));
                    loginActor.Tell(new P6LoginActor.LoginRequested());
                    Become(() => LoggingIn(Sender, msg));
                }
            });
        }

        private void LoggingIn(IActorRef originalSender, IP6Request payload)
        {
            Receive<P6Actor.SessionReceived>(msg =>
            {
                _sessionId = msg.SessionId;
                originalSender.Tell(new Continue(_sessionId, payload, payload.OriginalSender));
                Become(Idle);
            });

            // we probably don't need this handler
            Receive<IP6Request>(msg =>
            {
                if (string.IsNullOrEmpty(_sessionId))
                {
                    _log.Info("No session available, and already busy with login");
                    return;
                }

                _log.Info("Session available, continue with it");
                Sender.Tell(new Continue(_sessionId, payload, payload.OriginalSender));
            });
        }

        public static Props Props(IP6RestApi api, P6AuthOptions authOptions)
        {
            return Akka.Actor.Props.Create(() => new NewSessionOrContinueActor(api, authOptions));
        }
    }
}
