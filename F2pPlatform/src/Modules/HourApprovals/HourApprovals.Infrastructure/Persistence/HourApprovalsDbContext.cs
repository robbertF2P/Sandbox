using HourApprovals.Infrastructure.Persistence.Configurations;
using HourApprovals.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HourApprovals.Infrastructure.Persistence;

public sealed class HourApprovalsDbContext(DbContextOptions<HourApprovalsDbContext> options) : DbContext(options)
{
    public const string SchemaName = "hour_approvals";

    public DbSet<ActiveTaskEntity> ActiveTasks => Set<ActiveTaskEntity>();

    public DbSet<ApprovalRecordEntity> ApprovalRecords => Set<ApprovalRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfiguration(new ActiveTaskEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovalRecordEntityConfiguration());
    }
}
