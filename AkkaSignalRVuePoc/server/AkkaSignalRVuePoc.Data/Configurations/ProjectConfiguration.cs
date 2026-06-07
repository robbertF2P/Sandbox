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

        builder.Property(project => project.EstimatedHours)
            .HasHoursColumn();

        builder.HasOne(project => project.Organisation)
            .WithMany(organisation => organisation.Projects)
            .HasForeignKey(project => project.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(project => project.OrganisationId);
        builder.HasIndex(project => project.Name);

        builder.HasData(
            new Project
            {
                Id = CatalogSeedData.CustomerPortalProjectId,
                OrganisationId = CatalogSeedData.AcmeOrganisationId,
                Name = "Customer Portal",
                Description = "Public-facing web application",
                CreatedAt = CatalogSeedData.SeedCreatedAt
            },
            new Project
            {
                Id = CatalogSeedData.AkkaPocProjectId,
                OrganisationId = CatalogSeedData.DrivenItOrganisationId,
                Name = "Akka SignalR POC",
                Description = "Demonstration of Akka.NET, SignalR, and Vue",
                CreatedAt = CatalogSeedData.SeedCreatedAt
            });
    }
}
