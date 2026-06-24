using System.Text.Json;
using ControlPlane.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Platform.ControlPlane.Contracts;

namespace ControlPlane.Infrastructure.Persistence.Configurations;

internal sealed class TenantEntityConfiguration : IEntityTypeConfiguration<TenantEntity>
{
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.TenantId);

        builder.Property(t => t.Slug)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.Property(t => t.DisplayName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(t => t.Region)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(t => t.LegacyBuildProfileId).HasMaxLength(128);
        builder.Property(t => t.LegacyRuntimeUrl).HasMaxLength(512);
        builder.Property(t => t.LegacyDatabaseConnectionRef).HasMaxLength(256);
        builder.Property(t => t.NativeDatabaseConnectionRef).HasMaxLength(256);
        builder.Property(t => t.NativeApiBaseUrl).HasMaxLength(512);
        builder.Property(t => t.BillingTier).HasMaxLength(64).IsRequired();
        builder.Property(t => t.IntegrationPacksJson).IsRequired();
        builder.Property(t => t.CustomizationPacksJson).IsRequired();
        builder.Property(t => t.LastPlatformSyncError).HasMaxLength(2048);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(t => t.Mode)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(t => t.DataTier)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(t => t.MigrationPhase)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(t => t.MigrationTargetMode)
            .HasConversion<string>()
            .HasMaxLength(32);
    }
}
