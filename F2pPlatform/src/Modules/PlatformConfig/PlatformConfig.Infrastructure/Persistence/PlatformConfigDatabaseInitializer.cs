using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Platform.ControlPlane.Contracts;
using PlatformConfig.Application.Ports;

namespace PlatformConfig.Infrastructure.Persistence;

public sealed class PlatformConfigDatabaseInitializer(
    PlatformConfigDbContext dbContext,
    ITenantConfigurationStore store,
    ITenantRuntimeContext runtimeContext,
    IConfiguration configuration,
    ILogger<PlatformConfigDatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var isSqlite = dbContext.Database.IsSqlite();

        if (isSqlite)
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        await LoadActiveTenantAsync(cancellationToken);
        logger.LogInformation("Platform config database initialized");
    }

    private async Task LoadActiveTenantAsync(CancellationToken cancellationToken)
    {
        if (runtimeContext.Current is not null)
        {
            return;
        }

        var configuredSlug = configuration["Tenant:Slug"];
        if (!string.IsNullOrWhiteSpace(configuredSlug))
        {
            TenantConfigurationPayload? bySlug = await store.GetBySlugAsync(configuredSlug, cancellationToken);
            if (bySlug is not null)
            {
                runtimeContext.SetCurrent(bySlug);
                return;
            }
        }

        IReadOnlyList<TenantConfigurationPayload> tenants = await store.ListAsync(cancellationToken);
        if (tenants.Count == 1)
        {
            runtimeContext.SetCurrent(tenants[0]);
        }
    }
}
