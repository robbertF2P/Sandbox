using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Configurations;

public sealed class EntityExternalIdEntityConfiguration : IEntityTypeConfiguration<EntityExternalIdEntity>
{
    public void Configure(EntityTypeBuilder<EntityExternalIdEntity> builder)
    {
        builder.ToTable("EntityExternalIds");
        builder.HasKey(externalId => externalId.Id);
        builder.Property(externalId => externalId.Id).UseIdentityColumn();
        builder.Property(externalId => externalId.System).HasMaxLength(128).IsRequired();
        builder.Property(externalId => externalId.Value).HasMaxLength(256).IsRequired();
        builder.Property(externalId => externalId.EntityKind).HasConversion<string>().HasMaxLength(32);
        builder.HasIndex(externalId => new { externalId.System, externalId.Value }).IsUnique();
        builder.HasIndex(externalId => externalId.InternalEntityId);
    }
}
