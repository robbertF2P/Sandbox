using HourApprovals.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HourApprovals.Infrastructure.Persistence.Configurations;

internal sealed class ActiveTaskEntityConfiguration : IEntityTypeConfiguration<ActiveTaskEntity>
{
    public void Configure(EntityTypeBuilder<ActiveTaskEntity> builder)
    {
        builder.ToTable("active_tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Title).HasMaxLength(256).IsRequired();
        builder.Property(task => task.ActivityCode).HasMaxLength(64).IsRequired();
        builder.Property(task => task.AssignedUser).HasMaxLength(256).IsRequired();
        builder.Property(task => task.HoursToGo).HasPrecision(18, 4);

        builder.HasMany(task => task.ApprovalRecords)
            .WithOne(record => record.Task)
            .HasForeignKey(record => record.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
