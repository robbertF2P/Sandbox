using System.Text.Json;
using ControlPlane.Application.Ports;
using ControlPlane.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Platform.ControlPlane.Contracts;

namespace ControlPlane.Infrastructure.Persistence;

public sealed class EfTenantRepository(ControlPlaneDbContext dbContext) : ITenantRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TenantRecord?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == tenantId, cancellationToken);

        return entity is null ? null : ToRecord(entity);
    }

    public async Task<TenantRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);

        return entity is null ? null : ToRecord(entity);
    }

    public async Task<IReadOnlyList<TenantRecord>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Tenants
            .AsNoTracking()
            .OrderBy(t => t.Slug)
            .ToListAsync(cancellationToken);

        return entities.Select(ToRecord).ToArray();
    }

    public async Task<TenantRecord> AddAsync(TenantRecord tenant, CancellationToken cancellationToken)
    {
        dbContext.Tenants.Add(ToEntity(tenant));
        await dbContext.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task<TenantRecord> UpdateAsync(TenantRecord tenant, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Tenants
            .FirstOrDefaultAsync(t => t.TenantId == tenant.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{tenant.TenantId}' was not found.");

        Apply(entity, tenant);
        await dbContext.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    private static TenantRecord ToRecord(TenantEntity entity)
    {
        var integrationPacks = DeserializeList(entity.IntegrationPacksJson);
        var customizationPacks = DeserializeList(entity.CustomizationPacksJson);

        TenantDeploymentProfile deploymentProfile = entity.Mode switch
        {
            TenantDeploymentMode.LegacyHosted => TenantDeploymentProfile.CreateLegacyHosted(
                entity.DataTier,
                entity.Region,
                new LegacyHostedRuntimeProfile(
                    entity.LegacyBuildProfileId!,
                    entity.LegacyRuntimeUrl!,
                    entity.LegacyDatabaseConnectionRef!)),
            TenantDeploymentMode.Native => TenantDeploymentProfile.CreateNative(
                entity.DataTier,
                entity.Region,
                new NativeRuntimeProfile(
                    entity.NativeDatabaseConnectionRef!,
                    entity.NativeApiBaseUrl!)),
            _ => throw new InvalidOperationException($"Unsupported deployment mode '{entity.Mode}'.")
        };

        return new TenantRecord(
            entity.TenantId,
            entity.Slug,
            entity.DisplayName,
            entity.Status,
            deploymentProfile,
            new TenantPackEntitlements(integrationPacks, customizationPacks),
            new TenantMigrationState(
                entity.MigrationPhase,
                entity.MigrationTargetMode,
                entity.LastExportAt,
                entity.CutoverAt),
            new TenantBillingStub(entity.BillingTier, entity.SeatLimit),
            entity.CreatedAt,
            entity.ProvisionedAt,
            entity.LastSyncedToPlatformAt,
            entity.LastPlatformSyncError);
    }

    private static TenantEntity ToEntity(TenantRecord tenant)
    {
        var entity = new TenantEntity { TenantId = tenant.TenantId };
        Apply(entity, tenant);
        return entity;
    }

    private static void Apply(TenantEntity entity, TenantRecord tenant)
    {
        entity.Slug = tenant.Slug;
        entity.DisplayName = tenant.DisplayName;
        entity.Status = tenant.Status;
        entity.Mode = tenant.DeploymentProfile.Mode;
        entity.DataTier = tenant.DeploymentProfile.DataTier;
        entity.Region = tenant.DeploymentProfile.Region;
        entity.LegacyBuildProfileId = tenant.DeploymentProfile.Legacy?.BuildProfileId;
        entity.LegacyRuntimeUrl = tenant.DeploymentProfile.Legacy?.RuntimeUrl;
        entity.LegacyDatabaseConnectionRef = tenant.DeploymentProfile.Legacy?.DatabaseConnectionRef;
        entity.NativeDatabaseConnectionRef = tenant.DeploymentProfile.Native?.DatabaseConnectionRef;
        entity.NativeApiBaseUrl = tenant.DeploymentProfile.Native?.ApiBaseUrl;
        entity.IntegrationPacksJson = SerializeList(tenant.PackEntitlements.IntegrationPacks);
        entity.CustomizationPacksJson = SerializeList(tenant.PackEntitlements.CustomizationPacks);
        entity.MigrationPhase = tenant.Migration.Phase;
        entity.MigrationTargetMode = tenant.Migration.TargetMode;
        entity.LastExportAt = tenant.Migration.LastExportAt;
        entity.CutoverAt = tenant.Migration.CutoverAt;
        entity.BillingTier = tenant.Billing.Tier;
        entity.SeatLimit = tenant.Billing.SeatLimit;
        entity.CreatedAt = tenant.CreatedAt;
        entity.ProvisionedAt = tenant.ProvisionedAt;
        entity.LastSyncedToPlatformAt = tenant.LastSyncedToPlatformAt;
        entity.LastPlatformSyncError = tenant.LastPlatformSyncError;
    }

    private static IReadOnlyList<string> DeserializeList(string json) =>
        JsonSerializer.Deserialize<string[]>(json, JsonOptions) ?? [];

    private static string SerializeList(IReadOnlyList<string> values) =>
        JsonSerializer.Serialize(values, JsonOptions);
}
