using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Infrastructure;

public sealed class PlanningApprovalsDbContext(DbContextOptions<PlanningApprovalsDbContext> options)
    : DbContext(options)
{
    public DbSet<ActiveAssignment> ActiveAssignments => Set<ActiveAssignment>();

    public DbSet<AssignmentApprovalRecord> ApprovalRecords => Set<AssignmentApprovalRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ValueConverter<AssignmentId, long> assignmentIdConverter = new(
            id => id.Value,
            value => new AssignmentId(value));

        ValueConverter<PersonId, long> personIdConverter = new(
            id => id.Value,
            value => new PersonId(value));

        ValueConverter<AssignedUser, string> assignedUserConverter = new(
            user => user.Value,
            value => AssignedUser.From(value));

        modelBuilder.Entity<ActiveAssignment>(entity =>
        {
            entity.ToTable("active_assignments");
            entity.HasKey(assignment => assignment.Id);
            entity.Property(assignment => assignment.Id).HasConversion(assignmentIdConverter);
            entity.Property(assignment => assignment.Title).HasMaxLength(256).IsRequired();
            entity.Property(assignment => assignment.ActivityCode).HasMaxLength(64).IsRequired();
            ConfigureApprovalValues(entity.OwnsOne(assignment => assignment.CurrentValues), assignedUserConverter);
        });

        modelBuilder.Entity<AssignmentApprovalRecord>(entity =>
        {
            entity.ToTable("assignment_approval_records");
            entity.HasKey(record => record.Id);
            entity.HasIndex(record => new { record.AssignmentId, record.ApprovalDay }).IsUnique();
            entity.Property(record => record.AssignmentId).HasConversion(assignmentIdConverter);
            entity.Property(record => record.ApprovedBy).HasConversion(personIdConverter);
            ConfigureApprovalValues(entity.OwnsOne(record => record.ApprovedValues), assignedUserConverter);
        });
    }

    private static void ConfigureApprovalValues(
        OwnedNavigationBuilder<ActiveAssignment, ApprovalValues> owned,
        ValueConverter<AssignedUser, string> assignedUserConverter)
    {
        owned.Property(values => values.HoursToGo).HasColumnName("hours_to_go");
        owned.Property(values => values.PlannedStart).HasColumnName("planned_start");
        owned.Property(values => values.PlannedFinish).HasColumnName("planned_finish");
        owned.Property(values => values.AssignedUser)
            .HasColumnName("assigned_user")
            .HasConversion(assignedUserConverter)
            .HasMaxLength(256);
    }

    private static void ConfigureApprovalValues(
        OwnedNavigationBuilder<AssignmentApprovalRecord, ApprovalValues> owned,
        ValueConverter<AssignedUser, string> assignedUserConverter)
    {
        owned.Property(values => values.HoursToGo).HasColumnName("hours_to_go");
        owned.Property(values => values.PlannedStart).HasColumnName("planned_start");
        owned.Property(values => values.PlannedFinish).HasColumnName("planned_finish");
        owned.Property(values => values.AssignedUser)
            .HasColumnName("assigned_user")
            .HasConversion(assignedUserConverter)
            .HasMaxLength(256);
    }
}
