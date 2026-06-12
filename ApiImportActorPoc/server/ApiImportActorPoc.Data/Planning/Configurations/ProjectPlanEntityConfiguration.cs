using ApiImportActorPoc.Data.Conversions;
using ApiImportActorPoc.Data.Planning.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Planning.Configurations;

public sealed class ProjectPlanEntityConfiguration : IEntityTypeConfiguration<ProjectPlanEntity>
{
    public void Configure(EntityTypeBuilder<ProjectPlanEntity> builder)
    {
        builder.ToTable("ProjectPlans");
        builder.HasKey(plan => plan.ProjectId);
        builder.Property(plan => plan.PlannedStartDate).HasScheduleDateColumn();
        builder.Property(plan => plan.LastCalculatedAt).IsRequired();
        builder.HasOne(plan => plan.Project)
            .WithOne()
            .HasForeignKey<ProjectPlanEntity>(plan => plan.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
