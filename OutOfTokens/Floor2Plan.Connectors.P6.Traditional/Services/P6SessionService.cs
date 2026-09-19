using Floor2Plan.Connectors.P6.Api;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Traditional.Services
{
    /// <summary>
    /// Logs in to P6 and returns a session cookie for the duration of a scope.
    /// </summary>
    public sealed class P6SessionService
    {
        private readonly IP6RestApi _api;
        private readonly P6AuthOptions _authOptions;

        public P6SessionService(IP6RestApi api, IOptions<P6AuthOptions> authOptions)
        {
            _api = api;
            _authOptions = authOptions.Value;
        }

        public async Task<P6Session> OpenSessionAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_authOptions.Username) || string.IsNullOrWhiteSpace(_authOptions.Password))
            {
                throw new InvalidOperationException(
                    $"Configure {P6AuthOptions.SectionName}:Username and {P6AuthOptions.SectionName}:Password before calling the P6 API.");
            }

            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_authOptions.Username}:{_authOptions.Password}"));
            using var response = await _api.LoginAsync(
                authToken,
                _authOptions.DatabaseName,
                new ByteArrayContent([]),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"P6 login failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase}.");
            }

            if (!TryGetSessionId(response, out var sessionId))
            {
                throw new InvalidOperationException(
                    $"P6 login response did not include a valid {IP6RestApi.SessionCookieName} cookie.");
            }

            return new P6Session(_api, $"{IP6RestApi.SessionCookieName}={sessionId}");
        }

        private static bool TryGetSessionId(HttpResponseMessage response, out string sessionId)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            {
                sessionId = null;
                return false;
            }

            sessionId = setCookieHeaders
                .Select(ParseSessionId)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            return !string.IsNullOrWhiteSpace(sessionId);
        }

        private static string ParseSessionId(string setCookieHeader)
        {
            var part = setCookieHeader
                ?.Split(';')
                .Select(x => x.Trim())
                .FirstOrDefault(x => x.StartsWith($"{IP6RestApi.SessionCookieName}=", StringComparison.OrdinalIgnoreCase));

            return part?[(IP6RestApi.SessionCookieName.Length + 1)..];
        }
    }

    public sealed class P6Session : IAsyncDisposable
    {
        private readonly IP6RestApi _api;

        public P6Session(IP6RestApi api, string cookie)
        {
            _api = api;
            Cookie = cookie;
        }

        public string Cookie { get; }

        public async ValueTask DisposeAsync()
        {
            try
            {
                using var response = await _api.LogoutAsync(Cookie, new ByteArrayContent([]), CancellationToken.None);
                _ = response.IsSuccessStatusCode;
            }
            catch
            {
                // Best-effort logout; sync result is already decided.
            }
        }
    }
}
