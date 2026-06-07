using EfCoreHoursNormalization.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfCoreHoursNormalization.Configurations;

public sealed class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("TimeEntries");
        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(entry => entry.Hours)
            .HasHoursColumn();
    }
}
