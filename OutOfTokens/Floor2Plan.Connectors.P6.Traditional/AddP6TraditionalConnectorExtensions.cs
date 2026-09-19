using Contracts.Infrastructure.Connectors;
using Floor2Plan.Connectors.P6;
using Floor2Plan.Connectors.P6.Traditional.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Floor2Plan.Connectors.P6.Traditional
{
    public static class AddP6TraditionalConnectorExtensions
    {
        /// <summary>
        /// Registers the traditional OOP P6 connector alongside the shared Refit client and options.
        /// Does not register the actor-based <see cref="P6Connector"/> — call <see cref="ServiceCollectionExtensions.AddP6Connector"/> for that.
        /// </summary>
        public static IServiceCollection AddP6TraditionalConnector(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddP6ConnectorApiClient(configuration);

            services.AddSingleton<P6RawDataStore>();
            services.AddSingleton<P6SessionService>();
            services.AddSingleton<P6PagedCatalogReader>();
            services.AddSingleton<P6SyncProgressReporter>();
            services.AddSingleton<P6SyncService>();
            services.AddSingleton<P6RawDataExporter>();
            services.AddTransient<IGenericConnector, P6TraditionalConnector>();

            return services;
        }
    }
}
