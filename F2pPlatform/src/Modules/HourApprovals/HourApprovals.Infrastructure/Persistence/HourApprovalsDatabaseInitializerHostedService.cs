using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HourApprovals.Infrastructure.Persistence;

internal sealed class HourApprovalsDatabaseInitializerHostedService(
    IServiceProvider serviceProvider,
    ILogger<HourApprovalsDatabaseInitializerHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        HourApprovalsDatabaseInitializer initializer =
            scope.ServiceProvider.GetRequiredService<HourApprovalsDatabaseInitializer>();

        try
        {
            await initializer.InitializeAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Hour approvals database initialization failed");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
