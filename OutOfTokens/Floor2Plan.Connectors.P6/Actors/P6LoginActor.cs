using Akka.Actor;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Extensions;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Floor2Plan.Connectors.P6.Actors
{
    public sealed class P6LoginActor : ReceiveActor
    {
        private readonly P6AuthOptions _authOptions;

        public P6LoginActor(IP6RestApi P6RestApi, P6AuthOptions authOptions)
        {
            var api = P6RestApi;
            _authOptions = authOptions;

            Receive<LoginRequested>(message =>
            {
                if (string.IsNullOrWhiteSpace(_authOptions.Username) || string.IsNullOrWhiteSpace(_authOptions.Password))
                {
                    Sender.Tell(new SessionFailed(
                        $"Configure {P6AuthOptions.SectionName}:Username and {P6AuthOptions.SectionName}:Password before calling the P6 API."));
                    Context.Stop(Self);
                    return;
                }

                _ = api.LoginAsync(CreateAuthToken(), _authOptions.DatabaseName, CreateEmptyContent(), CancellationToken.None)
                    .PipeTo(
                        Self,
                        Sender,
                        response => new LoginResponseReceived(message, response),
                        exception => new LoginFailed(exception.Message));
            });

            Receive<LoginResponseReceived>(message =>
            {
                using var response = message.Response;
                if (!response.IsSuccessStatusCode)
                {
                    Sender.Tell(new SessionFailed(
                        $"P6 login failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase}."));
                    Context.Stop(Self);
                    return;
                }

                if (!TryGetSessionId(response, out var sessionId))
                {
                    Sender.Tell(new SessionFailed(
                        $"P6 login response did not include a valid {IP6RestApi.SessionCookieName} cookie."));
                    Context.Stop(Self);
                    return;
                }

                Sender.Tell(new SessionReceived(sessionId));
                Context.Stop(Self);
            });

            Receive<LoginFailed>(message =>
            {
                Sender.Tell(new SessionFailed(message.Reason));
                Context.Stop(Self);
            });
        }

        public static Props Props(IP6RestApi api, P6AuthOptions authOptions)
        {
            return Akka.Actor.Props.Create(() => new P6LoginActor(api, authOptions));
        }

        internal sealed record LoginRequested();

        private sealed record LoginResponseReceived(LoginRequested Request, HttpResponseMessage Response);

        private sealed record LoginFailed(string Reason);

        private string CreateAuthToken()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_authOptions.Username}:{_authOptions.Password}"));
        }

        private static HttpContent CreateEmptyContent()
        {
            return new ByteArrayContent(Array.Empty<byte>());
        }

        private static bool TryGetSessionId(HttpResponseMessage response, out string sessionId)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            {
                sessionId = null;
                return false;
            }

            sessionId = setCookieHeaders
                .Select(x =>x.ParseSessionId())
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            return !string.IsNullOrWhiteSpace(sessionId);
        }

        
    }
}
