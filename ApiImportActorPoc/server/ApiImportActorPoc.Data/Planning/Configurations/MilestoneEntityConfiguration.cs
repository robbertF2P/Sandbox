using ApiImportActorPoc.Data.Conversions;
using ApiImportActorPoc.Data.Planning.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Planning.Configurations;

public sealed class MilestoneEntityConfiguration : IEntityTypeConfiguration<MilestoneEntity>
{
    public void Configure(EntityTypeBuilder<MilestoneEntity> builder)
    {
        builder.ToTable("Milestones");
        builder.HasKey(milestone => milestone.Id);
        builder.Property(milestone => milestone.Id).UseIdentityColumn();
        builder.Property(milestone => milestone.Name).HasMaxLength(256).IsRequired();
        builder.Property(milestone => milestone.TargetDate).HasScheduleDateColumn();
        builder.HasOne(milestone => milestone.Project)
            .WithMany()
            .HasForeignKey(milestone => milestone.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(milestone => milestone.Activity)
            .WithMany()
            .HasForeignKey(milestone => milestone.ActivityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
