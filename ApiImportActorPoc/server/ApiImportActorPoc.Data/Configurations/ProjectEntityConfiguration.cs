using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Configurations;

public sealed class ProjectEntityConfiguration : IEntityTypeConfiguration<ProjectEntity>
{
    public void Configure(EntityTypeBuilder<ProjectEntity> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Id).UseIdentityColumn();
        builder.Property(project => project.Name).HasMaxLength(256).IsRequired();
        builder.HasMany(project => project.Components)
            .WithOne(component => component.Project)
            .HasForeignKey(component => component.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
