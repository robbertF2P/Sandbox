using AkkaSignalRVuePoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkkaSignalRVuePoc.Data.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(project => project.Id);

        builder.Property(project => project.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(project => project.Description)
            .HasMaxLength(2000);

        builder.Property(project => project.CreatedAt)
            .IsRequired();

        builder.HasOne(project => project.Organisation)
            .WithMany(organisation => organisation.Projects)
            .HasForeignKey(project => project.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(project => project.OrganisationId);
        builder.HasIndex(project => project.Name);
    }
}
