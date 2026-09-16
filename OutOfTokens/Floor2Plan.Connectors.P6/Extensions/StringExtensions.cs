
using Floor2Plan.Connectors.P6.Api;
using System;
using System.Linq;

namespace Floor2Plan.Connectors.P6.Extensions
{
    internal static class StringExtensions
    {
        internal static string ParseSessionId(this string setCookieHeader)
        {
            return (setCookieHeader
                ?.Split(';')
                .Select(x => x.Trim())
                .FirstOrDefault(x => x.StartsWith($"{IP6RestApi.SessionCookieName}=", StringComparison.OrdinalIgnoreCase)))?[(IP6RestApi.SessionCookieName.Length + 1)..];
        }
    }
}
