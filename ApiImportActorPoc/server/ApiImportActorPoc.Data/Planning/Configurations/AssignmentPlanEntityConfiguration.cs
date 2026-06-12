using ApiImportActorPoc.Data.Conversions;
using ApiImportActorPoc.Data.Planning.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Planning.Configurations;

public sealed class AssignmentPlanEntityConfiguration : IEntityTypeConfiguration<AssignmentPlanEntity>
{
    public void Configure(EntityTypeBuilder<AssignmentPlanEntity> builder)
    {
        builder.ToTable("AssignmentPlans");
        builder.HasKey(plan => plan.AssignmentId);
        builder.Property(plan => plan.DurationDays).HasDurationDaysColumn();
        builder.HasOne(plan => plan.Assignment)
            .WithOne()
            .HasForeignKey<AssignmentPlanEntity>(plan => plan.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
