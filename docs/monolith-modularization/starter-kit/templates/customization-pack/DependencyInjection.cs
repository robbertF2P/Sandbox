using <Context>.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace <Context>.Packs.<Client>;

public static class DependencyInjection
{
    public static IServiceCollection Add<Client><Context>Pack(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<I<Context>CustomizationPack, <Client><Context>Pack>();
        return services;
    }
}
