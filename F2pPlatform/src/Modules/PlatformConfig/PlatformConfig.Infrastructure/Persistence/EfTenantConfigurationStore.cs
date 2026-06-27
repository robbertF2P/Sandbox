using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Platform.ControlPlane.Contracts;
using PlatformConfig.Application.Ports;
using PlatformConfig.Infrastructure.Persistence.Entities;

namespace PlatformConfig.Infrastructure.Persistence;

internal sealed class EfTenantConfigurationStore(
    PlatformConfigDbContext dbContext,
    ITenantRuntimeContext runtimeContext) : ITenantConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task UpsertAsync(TenantConfigurationPayload configuration, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var slug = configuration.Slug.Trim().ToLowerInvariant();
        TenantConfigurationEntity? entity = await dbContext.TenantConfigurations
            .FirstOrDefaultAsync(tenant => tenant.TenantId == configuration.TenantId, cancellationToken);

        if (entity is null)
        {
            entity = new TenantConfigurationEntity { TenantId = configuration.TenantId };
            dbContext.TenantConfigurations.Add(entity);
        }

        entity.Slug = slug;
        entity.PayloadJson = JsonSerializer.Serialize(configuration, JsonOptions);
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        runtimeContext.SetCurrent(configuration);
    }

    public async Task<TenantConfigurationPayload?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        TenantConfigurationEntity? entity = await dbContext.TenantConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(tenant => tenant.Slug == slug.Trim().ToLowerInvariant(), cancellationToken);

        return entity is null ? null : Deserialize(entity);
    }

    public async Task<IReadOnlyList<TenantConfigurationPayload>> ListAsync(CancellationToken cancellationToken)
    {
        List<TenantConfigurationEntity> entities = await dbContext.TenantConfigurations
            .AsNoTracking()
            .OrderBy(tenant => tenant.Slug)
            .ToListAsync(cancellationToken);

        return entities.Select(Deserialize).ToList();
    }

    private static TenantConfigurationPayload Deserialize(TenantConfigurationEntity entity) =>
        JsonSerializer.Deserialize<TenantConfigurationPayload>(entity.PayloadJson, JsonOptions)
        ?? throw new InvalidOperationException($"Tenant configuration payload for '{entity.Slug}' is invalid.");
}
