using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Configurations;

public sealed class ActivityEntityConfiguration : IEntityTypeConfiguration<ActivityEntity>
{
    public void Configure(EntityTypeBuilder<ActivityEntity> builder)
    {
        builder.ToTable("Activities");
        builder.HasKey(activity => activity.Id);
        builder.Property(activity => activity.Id).UseIdentityColumn();
        builder.Property(activity => activity.Name).HasMaxLength(256).IsRequired();
        builder.HasMany(activity => activity.Assignments)
            .WithOne(assignment => assignment.Activity)
            .HasForeignKey(assignment => assignment.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(activity => activity.OutgoingRelations)
            .WithOne(relation => relation.SourceActivity)
            .HasForeignKey(relation => relation.SourceActivityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
