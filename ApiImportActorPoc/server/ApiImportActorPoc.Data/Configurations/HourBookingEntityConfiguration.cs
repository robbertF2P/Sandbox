using ApiImportActorPoc.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiImportActorPoc.Data.Configurations;

public sealed class HourBookingEntityConfiguration : IEntityTypeConfiguration<HourBookingEntity>
{
    public void Configure(EntityTypeBuilder<HourBookingEntity> builder)
    {
        builder.ToTable("HourBookings");
        builder.HasKey(booking => booking.Id);
        builder.Property(booking => booking.Id).UseIdentityColumn();
        builder.Property(booking => booking.Hours).HasPrecision(18, 2);
        builder.Property(booking => booking.Notes).HasMaxLength(512);
        builder.HasOne(booking => booking.Assignment)
            .WithMany(assignment => assignment.HourBookings)
            .HasForeignKey(booking => booking.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
