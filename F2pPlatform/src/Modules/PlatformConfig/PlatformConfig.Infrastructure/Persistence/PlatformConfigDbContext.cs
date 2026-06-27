using Microsoft.EntityFrameworkCore;
using PlatformConfig.Infrastructure.Persistence.Configurations;
using PlatformConfig.Infrastructure.Persistence.Entities;

namespace PlatformConfig.Infrastructure.Persistence;

public sealed class PlatformConfigDbContext(DbContextOptions<PlatformConfigDbContext> options) : DbContext(options)
{
    public const string SchemaName = "platform_config";

    public DbSet<TenantConfigurationEntity> TenantConfigurations => Set<TenantConfigurationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfiguration(new TenantConfigurationEntityConfiguration());
    }
}
