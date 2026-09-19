using Contracts.Infrastructure.Connectors;
using Floor2Plan.Connectors.P6.Api;
using Floor2Plan.Connectors.P6.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;
using System;

namespace Floor2Plan.Connectors.P6
{
    /// <summary>
    /// Shared Refit client and P6 options registration used by both connector implementations.
    /// </summary>
    public static class AddP6ConnectorApiClientExtensions
    {
        public static IServiceCollection AddP6ConnectorApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection(P6ApiOptions.SectionName);
            services.Configure<P6ApiOptions>(options =>
            {
                options.BaseUrl = configurationSection[nameof(P6ApiOptions.BaseUrl)];
                options.LogHttpTraffic = configurationSection.GetValue<bool>(nameof(P6ApiOptions.LogHttpTraffic));
                options.LogHttpBodies = configurationSection.GetValue<bool>(nameof(P6ApiOptions.LogHttpBodies));

                var timeoutValue = configurationSection[nameof(P6ApiOptions.Timeout)];
                if (TimeSpan.TryParse(timeoutValue, out var timeout) && timeout > TimeSpan.Zero)
                {
                    options.Timeout = timeout;
                }
            });

            var authConfigurationSection = configuration.GetSection(P6AuthOptions.SectionName);
            services.Configure<P6AuthOptions>(options =>
            {
                options.Username = authConfigurationSection[nameof(P6AuthOptions.Username)];
                options.Password = authConfigurationSection[nameof(P6AuthOptions.Password)];
                options.DatabaseName = authConfigurationSection[nameof(P6AuthOptions.DatabaseName)];

                var sessionIdleTimeoutValue = authConfigurationSection[nameof(P6AuthOptions.SessionIdleTimeout)];
                if (TimeSpan.TryParse(sessionIdleTimeoutValue, out var sessionIdleTimeout) && sessionIdleTimeout > TimeSpan.Zero)
                {
                    options.SessionIdleTimeout = sessionIdleTimeout;
                }
            });
            services.Configure<P6SyncOptions>(options =>
            {
                var syncConfigurationSection = configuration.GetSection(P6SyncOptions.SectionName);
                options.MaxConcurrency = Math.Max(1, syncConfigurationSection.GetValue<int?>(nameof(P6SyncOptions.MaxConcurrency)) ?? options.MaxConcurrency);
                options.PageSize = Math.Max(1, syncConfigurationSection.GetValue<int?>(nameof(P6SyncOptions.PageSize)) ?? options.PageSize);

                var requestTimeoutValue = syncConfigurationSection[nameof(P6SyncOptions.RequestTimeout)];
                if (TimeSpan.TryParse(requestTimeoutValue, out var requestTimeout) && requestTimeout > TimeSpan.Zero)
                {
                    options.RequestTimeout = requestTimeout;
                }

                options.AdditionalFilter = syncConfigurationSection[nameof(P6SyncOptions.AdditionalFilter)];
                var entityKinds = syncConfigurationSection
                    .GetSection(nameof(P6SyncOptions.EntityKinds))
                    .Get<P6EntityKind[]>();
                if (entityKinds is { Length: > 0 })
                {
                    options.EntityKinds = entityKinds;
                }
            });
            services.AddTransient<P6HttpLoggingHandler>();

            services.AddRefitClient<IP6RestApi>()
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptions<P6ApiOptions>>().Value;
                    client.BaseAddress = TryCreateBaseUri(options.BaseUrl) ?? new Uri("http://dummy/");
                    client.Timeout = options.Timeout;
                })
                .AddHttpMessageHandler<P6HttpLoggingHandler>();

            return services;
        }

        private static Uri TryCreateBaseUri(string baseUrl)
        {
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                return new Uri(baseUri.GetLeftPart(UriPartial.Authority));
            }

            return null;
        }
    }
}
