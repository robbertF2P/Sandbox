using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Infrastructure.Akka.Actors.Session;
using Infrastructure.Akka.Actors.Workers;
using System;
using System.Net.Http;
using System.Threading;

namespace Floor2Plan.Connectors.P6.Actors
{
    internal sealed class P6SessionGateBehavior : ISessionGateBehavior
    {
        private readonly IP6RestApi _api;
        private readonly P6AuthOptions _authOptions;
        private ISessionGateHost _host;

        public P6SessionGateBehavior(IP6RestApi api, P6AuthOptions authOptions)
        {
            _api = api;
            _authOptions = authOptions;
        }

        public TimeSpan IdleTimeout => _authOptions.SessionIdleTimeout;

        public void Attach(ISessionGateHost host)
        {
            _host = host;

            host.OnWork<FetchProjectCatalog>(work => host.BeginLogin(work));
            host.OnWork<BuildRawData>(work => host.BeginLogin(work));
            host.OnWork<RunSync>(work => host.BeginLogin(work));

            host.Receive<P6LoginSucceeded>(message =>
            {
                if (!host.IsLoggingIn)
                {
                    return;
                }

                host.CompleteLogin(message.SessionId);
            });

            host.Receive<P6LoginFailed>(message =>
            {
                if (!host.IsLoggingIn)
                {
                    return;
                }

                host.FailLogin(message.Reason);
            });

            host.Receive<Terminated>(_ => host.ScheduleIdleLogout(host.SessionContext));
            host.Receive<P6SyncOrchestratorActor.SyncFinished>(message => host.Parent.Tell(message));
            host.Receive<LogoutFailed>(message => host.Log.Error(message.Exception, "Failed to logout from P6"));
            host.Receive<LogoutCompleted>(message =>
            {
                using var response = message.Response;
                if (!response.IsSuccessStatusCode)
                {
                    host.Log.Warning(
                        "P6 logout failed with HTTP {0} {1}",
                        (int)response.StatusCode,
                        response.ReasonPhrase);
                }
            });
        }

        public Props CreateLoginWorker()
        {
            return P6LoginActor.Props(_api, _authOptions);
        }

        public object CreateLoginRequest()
        {
            return new P6LoginActor.LoginRequested();
        }

        public void Dispatch(object sessionContext, object work)
        {
            var sessionId = (string)sessionContext;
            var cookie = $"{IP6RestApi.SessionCookieName}={sessionId}";

            switch (work)
            {
                case FetchProjectCatalog catalog:
                    SpawnWorker(P6ProjectCatalogActor.Props(_api, cookie))
                        .Tell(new PagedFetchActor<P6ProjectRecord>.Start(catalog.ReplyTo));
                    break;
                case BuildRawData rawData:
                    SpawnWorker(P6RawDataBuilderActor.Props(_api))
                        .Tell(new P6RawDataBuilderActor.Build(cookie, rawData.ReplyTo));
                    break;
                case RunSync sync:
                    SpawnWorker(P6SyncOrchestratorActor.Props(_api, sync.Store, sync.SyncOptions))
                        .Tell(new P6SyncOrchestratorActor.Start(sync.ProjectIds, cookie, sync.ReplyTo));
                    break;
                default:
                    throw new InvalidOperationException("Unsupported P6 session command.");
            }
        }

        public void ReplyWorkFailed(object work, string reason)
        {
            var failure = new Status.Failure(new InvalidOperationException(reason));
            switch (work)
            {
                case FetchProjectCatalog catalog:
                    catalog.ReplyTo.Tell(failure);
                    break;
                case BuildRawData rawData:
                    rawData.ReplyTo.Tell(failure);
                    break;
                case RunSync sync:
                    sync.ReplyTo.Tell(failure);
                    break;
            }
        }

        public void Logout(object sessionContext)
        {
            var sessionId = (string)sessionContext;
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return;
            }

            _host.CancelIdleLogout();
            _host.SessionContext = null;
            var cookie = $"{IP6RestApi.SessionCookieName}={sessionId}";
            _ = _api.LogoutAsync(cookie, new ByteArrayContent([]), CancellationToken.None)
                .PipeTo(
                    _host.Self,
                    _host.Self,
                    response => new LogoutCompleted(response),
                    exception => new LogoutFailed(exception));
        }

        public bool SessionsEqual(object sessionContext, object sessionToken)
        {
            return string.Equals((string)sessionContext, (string)sessionToken, StringComparison.Ordinal);
        }

        private IActorRef SpawnWorker(Props props)
        {
            var worker = _host.ActorOf(props);
            _host.WatchWorker(worker);
            return worker;
        }

        private sealed record LogoutFailed(Exception Exception);

        private sealed record LogoutCompleted(HttpResponseMessage Response);

    }
}
