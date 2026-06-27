using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlatformConfig.Infrastructure.Persistence.Entities;

namespace PlatformConfig.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfigurationEntityConfiguration : IEntityTypeConfiguration<TenantConfigurationEntity>
{
    public void Configure(EntityTypeBuilder<TenantConfigurationEntity> builder)
    {
        builder.ToTable("tenant_configurations");
        builder.HasKey(entity => entity.TenantId);
        builder.Property(entity => entity.Slug).HasMaxLength(128).IsRequired();
        builder.HasIndex(entity => entity.Slug).IsUnique();
        builder.Property(entity => entity.PayloadJson).IsRequired();
        builder.Property(entity => entity.UpdatedAtUtc).IsRequired();
    }
}
