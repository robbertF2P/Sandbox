using HourApprovals.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HourApprovals.Infrastructure.Persistence.Configurations;

internal sealed class ApprovalRecordEntityConfiguration : IEntityTypeConfiguration<ApprovalRecordEntity>
{
    public void Configure(EntityTypeBuilder<ApprovalRecordEntity> builder)
    {
        builder.ToTable("approval_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.ApprovedBy).HasMaxLength(256).IsRequired();
        builder.Property(record => record.AssignedUser).HasMaxLength(256).IsRequired();
        builder.Property(record => record.HoursToGo).HasPrecision(18, 4);

        builder.HasIndex(record => new { record.TaskId, record.ApprovalDay }).IsUnique();
    }
}
