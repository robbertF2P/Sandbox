using AkkaSignalRVuePoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AkkaSignalRVuePoc.Data.Configurations;

public sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("Organisations");

        builder.HasKey(organisation => organisation.Id);

        builder.Property(organisation => organisation.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(organisation => organisation.CreatedAt)
            .IsRequired();

        builder.HasIndex(organisation => organisation.Name);
    }
}
