using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api;
using System;
using System.Net.Http;
using System.Threading;

namespace Floor2Plan.Connectors.P6.Actors;

/// <summary>
/// Owns the P6 REST session: login, logout, idle timeout, and dispatching authenticated work to child actors.
/// </summary>
public sealed class P6SessionActor : ReceiveActor, IWithUnboundedStash
{
    public IStash Stash { get; set; } = null!;
    public const string ActorName = "p6-session";

    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IP6RestApi _api;
    private readonly P6AuthOptions _authOptions;

    private string? _sessionId;
    private ICancelable? _logoutSchedule;
    private object? _loginPendingCommand;

    public P6SessionActor(IP6RestApi api, P6AuthOptions authOptions)
    {
        _api = api;
        _authOptions = authOptions;
        Become(Ready);
    }

    public static Props Props(IP6RestApi api, P6AuthOptions authOptions)
    {
        return Akka.Actor.Props.Create(() => new P6SessionActor(api, authOptions));
    }

    protected override void PostStop()
    {
        CancelIdleLogout();
        LogoutCurrentSession();
        base.PostStop();
    }

    private void Ready()
    {
        Receive<FetchProjectCatalog>(command => StartWork(command));
        Receive<BuildRawData>(command => StartWork(command));
        Receive<RunSync>(command => StartWork(command));
        RegisterCommonHandlers();
    }

    private void LoggingIn()
    {
        Receive<P6LoginSucceeded>(message =>
        {
            _sessionId = message.SessionId;
            _log.Info("P6 session established");
            var pending = _loginPendingCommand!;
            _loginPendingCommand = null;
            Become(Ready);
            Dispatch(pending);
            Stash.UnstashAll();
        });

        Receive<P6LoginFailed>(message =>
        {
            _log.Warning("Could not login to P6: {0}", message.Reason);
            var pending = _loginPendingCommand!;
            _loginPendingCommand = null;
            Become(Ready);
            ReplyLoginFailure(pending, message.Reason);
            Stash.UnstashAll();
        });

        Receive<FetchProjectCatalog>(_ => Stash.Stash());
        Receive<BuildRawData>(_ => Stash.Stash());
        Receive<RunSync>(_ => Stash.Stash());
        RegisterCommonHandlers();
    }

    private void RegisterCommonHandlers()
    {
        Receive<Terminated>(_ => ScheduleIdleLogout());
        Receive<P6SyncOrchestratorActor.SyncFinished>(message => Context.Parent.Tell(message));
        Receive<SessionIdleTimeoutElapsed>(message =>
        {
            if (string.Equals(message.SessionId, _sessionId, StringComparison.Ordinal))
            {
                LogoutCurrentSession();
            }
        });
        Receive<LogoutFailed>(message => _log.Error(message.Exception, "Failed to logout from P6"));
        Receive<LogoutCompleted>(message =>
        {
            using var response = message.Response;
            if (!response.IsSuccessStatusCode)
            {
                _log.Warning("P6 logout failed with HTTP {0} {1}", (int)response.StatusCode, response.ReasonPhrase);
            }
        });
    }

    private void StartWork(object command)
    {
        CancelIdleLogout();

        if (!string.IsNullOrWhiteSpace(_sessionId))
        {
            Dispatch(command);
            return;
        }

        if (_loginPendingCommand != null)
        {
            ReplyLoginFailure(command, "P6 login is already in progress.");
            return;
        }

        _loginPendingCommand = command;
        Become(LoggingIn);
        Context.ActorOf(P6LoginActor.Props(_api, _authOptions)).Tell(new P6LoginActor.LoginRequested());
    }

    private void Dispatch(object command)
    {
        var cookie = CreateSessionCookie();

        switch (command)
        {
            case FetchProjectCatalog catalog:
                SpawnWorker(P6ProjectCatalogActor.Props(_api))
                    .Tell(new P6ProjectCatalogActor.Fetch(cookie, catalog.ReplyTo));
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

    private IActorRef SpawnWorker(Props props)
    {
        var worker = Context.ActorOf(props);
        Context.Watch(worker);
        return worker;
    }

    private static void ReplyLoginFailure(object command, string reason)
    {
        var failure = new Status.Failure(new InvalidOperationException(reason));
        switch (command)
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

    private void LogoutCurrentSession()
    {
        if (string.IsNullOrWhiteSpace(_sessionId))
        {
            return;
        }

        CancelIdleLogout();
        var cookie = CreateSessionCookie();
        _sessionId = null;
        _ = _api.LogoutAsync(cookie, new ByteArrayContent([]), CancellationToken.None)
            .PipeTo(
                Self,
                Self,
                response => new LogoutCompleted(response),
                exception => new LogoutFailed(exception));
    }

    private void ScheduleIdleLogout()
    {
        if (string.IsNullOrWhiteSpace(_sessionId))
        {
            return;
        }

        CancelIdleLogout();
        _logoutSchedule = Context.System.Scheduler.ScheduleTellOnceCancelable(
            _authOptions.SessionIdleTimeout,
            Self,
            new SessionIdleTimeoutElapsed(_sessionId),
            Self);
    }

    private void CancelIdleLogout()
    {
        _logoutSchedule?.Cancel();
        _logoutSchedule = null;
    }

    private string CreateSessionCookie()
    {
        return $"{IP6RestApi.SessionCookieName}={_sessionId}";
    }

    private sealed record LogoutFailed(Exception Exception);

    private sealed record LogoutCompleted(HttpResponseMessage Response);

    private sealed record SessionIdleTimeoutElapsed(string SessionId);
}
