using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Configurations;

public sealed class ActivityRelationEntityConfiguration : IEntityTypeConfiguration<ActivityRelationEntity>
{
    public void Configure(EntityTypeBuilder<ActivityRelationEntity> builder)
    {
        builder.ToTable("ActivityRelations");
        builder.HasKey(relation => relation.Id);
        builder.Property(relation => relation.Id).UseIdentityColumn();
        builder.Property(relation => relation.RelationType).HasMaxLength(32).IsRequired();
        builder.Property(relation => relation.LagDays).HasDefaultValue(0);
        builder.HasOne(relation => relation.TargetActivity)
            .WithMany()
            .HasForeignKey(relation => relation.TargetActivityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
