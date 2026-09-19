using Contracts.Infrastructure.Connectors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Floor2Plan.Connectors.P6
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddP6Connector(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddP6ConnectorApiClient(configuration);
            services.AddTransient<IGenericConnector, P6Connector>();
            return services;
        }
    }
}
