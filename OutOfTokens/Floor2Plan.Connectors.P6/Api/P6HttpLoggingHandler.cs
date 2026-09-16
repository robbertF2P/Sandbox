using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Floor2Plan.Connectors.P6.Api
{
    public sealed class P6HttpLoggingHandler : DelegatingHandler
    {
        private const string _redactedValue = "[redacted]";

        private readonly ILogger<P6HttpLoggingHandler> _logger;
        private readonly IOptions<P6ApiOptions> _options;

        public P6HttpLoggingHandler(
            ILogger<P6HttpLoggingHandler> logger,
            IOptions<P6ApiOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!_options.Value.LogHttpTraffic)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            _logger.LogInformation(
                "P6 HTTP request:{NewLine}{Request}",
                System.Environment.NewLine,
                await FormatRequestAsync(request, cancellationToken));

            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation(
                "P6 HTTP response:{NewLine}{Response}",
                System.Environment.NewLine,
                await FormatResponseAsync(response, cancellationToken));

            return response;
        }

        private async Task<string> FormatRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            builder.Append(request.Method);
            builder.Append(' ');
            builder.AppendLine(request.RequestUri?.ToString() ?? string.Empty);
            AppendHeaders(builder, request.Headers);

            if (request.Content == null)
            {
                return builder.ToString();
            }

            AppendHeaders(builder, request.Content.Headers);

            if (!_options.Value.LogHttpBodies)
            {
                return builder.ToString();
            }

            builder.AppendLine();
            builder.AppendLine(await request.Content.ReadAsStringAsync(cancellationToken));

            return builder.ToString();
        }

        private async Task<string> FormatResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();
            builder.Append((int)response.StatusCode);
            builder.Append(' ');
            builder.AppendLine(response.ReasonPhrase ?? response.StatusCode.ToString());
            AppendHeaders(builder, response.Headers);

            AppendHeaders(builder, response.Content.Headers);

            if (_options.Value.LogHttpBodies)
            {
                builder.AppendLine();
                builder.AppendLine(await response.Content.ReadAsStringAsync(cancellationToken));
            }

            return builder.ToString();
        }

        private static void AppendHeaders(StringBuilder builder, HttpHeaders headers)
        {
            foreach (var header in headers.OrderBy(header => header.Key, System.StringComparer.OrdinalIgnoreCase))
            {
                builder.Append(header.Key);
                builder.Append(": ");
                builder.AppendLine(IsSensitiveHeader(header.Key) ? _redactedValue : string.Join(",", header.Value));
            }
        }

        private static bool IsSensitiveHeader(string headerName)
        {
            return headerName.Equals("Authorization", System.StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals(IP6RestApi.AuthTokenHeaderName, System.StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals(IP6RestApi.CookieHeaderName, System.StringComparison.OrdinalIgnoreCase) ||
                   headerName.Equals("Set-Cookie", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
