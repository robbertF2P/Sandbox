using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PlatformConfig.Infrastructure.Persistence;

public sealed class PlatformConfigDatabaseInitializerHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<PlatformConfigDatabaseInitializerHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<PlatformConfigDatabaseInitializer>();
        await initializer.InitializeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Platform config database initializer stopped");
        return Task.CompletedTask;
    }
}
