using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlanningApprovals.Domain.Models;

namespace PlanningApprovals.Infrastructure;

public sealed class PlanningApprovalsDbContext(DbContextOptions<PlanningApprovalsDbContext> options)
    : DbContext(options)
{
    public DbSet<AssignmentApprovalRequest> ApprovalRequests => Set<AssignmentApprovalRequest>();

    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();

    public DbSet<ApprovedPlanSnapshot> ApprovedPlanSnapshots => Set<ApprovedPlanSnapshot>();

    public DbSet<ForemanApprovalBatch> ApprovalBatches => Set<ForemanApprovalBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AssignmentApprovalRequest>(entity =>
        {
            entity.ToTable("assignment_approval_requests");
            entity.HasKey(request => request.Id);
            entity.HasIndex(request => request.PublicId).IsUnique();
            entity.HasIndex(request => new { request.ProjectId, request.Status, request.OpenedAt });
            entity.HasIndex(request => new { request.AssignmentId, request.Status });

            entity.Property(request => request.PublicId).IsRequired();
            entity.Property(request => request.ProjectId).IsRequired();
            entity.Property(request => request.AssignmentId).IsRequired();
            entity.Property(request => request.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(request => request.RequiredBecause).HasConversion<string>().HasMaxLength(32);
            entity.Property(request => request.OpenedByProcess).IsRequired().HasMaxLength(128);
            entity.Property(request => request.LastApprovedSnapshotId);

            ConfigureProgressRevision(entity.OwnsOne(request => request.ProgressRevision));
            ConfigurePlanSnapshot(entity.OwnsOne(request => request.ProposedPlan));
        });

        modelBuilder.Entity<ApprovalDecision>(entity =>
        {
            entity.ToTable("approval_decisions");
            entity.HasKey(decision => decision.Id);
            entity.HasIndex(decision => decision.PublicId).IsUnique();
            entity.HasIndex(decision => new { decision.AssignmentId, decision.DecidedAt });
            entity.HasIndex(decision => decision.RequestPublicId);

            entity.Property(decision => decision.PublicId).IsRequired();
            entity.Property(decision => decision.RequestPublicId).IsRequired();
            entity.Property(decision => decision.AssignmentId).IsRequired();
            entity.Property(decision => decision.Decision).HasConversion<string>().HasMaxLength(16);
            entity.Property(decision => decision.DecidedByPersonId).IsRequired();
            entity.Property(decision => decision.CorrelationId).IsRequired().HasMaxLength(64);
            entity.Property(decision => decision.Comment).HasMaxLength(2000);
            entity.Property(decision => decision.BatchPublicId);

            ConfigureProgressRevision(entity.OwnsOne(decision => decision.ProgressRevisionAtDecision));
            ConfigurePlanSnapshot(entity.OwnsOne(decision => decision.ProposedPlanAtDecision));
        });

        modelBuilder.Entity<ApprovedPlanSnapshot>(entity =>
        {
            entity.ToTable("approved_plan_snapshots");
            entity.HasKey(snapshot => snapshot.Id);
            entity.HasIndex(snapshot => snapshot.PublicId).IsUnique();
            entity.HasIndex(snapshot => new { snapshot.AssignmentId, snapshot.ApprovedAt });

            entity.Property(snapshot => snapshot.PublicId).IsRequired();
            entity.Property(snapshot => snapshot.DecisionPublicId).IsRequired();
            entity.Property(snapshot => snapshot.AssignmentId).IsRequired();
            entity.Property(snapshot => snapshot.ApprovedByPersonId).IsRequired();

            ConfigureProgressRevision(entity.OwnsOne(snapshot => snapshot.ProgressRevision));
            ConfigurePlanSnapshot(entity.OwnsOne(snapshot => snapshot.PlanSnapshot));
        });

        modelBuilder.Entity<ForemanApprovalBatch>(entity =>
        {
            entity.ToTable("foreman_approval_batches");
            entity.HasKey(batch => batch.Id);
            entity.HasIndex(batch => batch.PublicId).IsUnique();

            entity.Property(batch => batch.PublicId).IsRequired();
            entity.Property(batch => batch.ProjectId).IsRequired();
            entity.Property(batch => batch.ForemanPersonId).IsRequired();
            entity.Property(batch => batch.ScopeDescription).IsRequired().HasMaxLength(256);
            entity.Property(batch => batch.Status).HasConversion<string>().HasMaxLength(16);

            entity.Property<List<Guid>>("_requestPublicIds")
                .HasColumnName("request_public_ids")
                .HasConversion(
                    ids => string.Join('|', ids),
                    value => string.IsNullOrEmpty(value)
                        ? new List<Guid>()
                        : value.Split('|', StringSplitOptions.RemoveEmptyEntries)
                            .Select(Guid.Parse)
                            .ToList());
        });
    }

    private static void ConfigureProgressRevision<T>(
        OwnedNavigationBuilder<T, Domain.ValueObjects.ProgressRevisionRef> owned) where T : class
    {
        owned.Property(revision => revision.AssignmentId).HasColumnName("progress_assignment_id");
        owned.Property(revision => revision.RevisionId).HasColumnName("progress_revision_id");
        owned.Property(revision => revision.RecordedAt).HasColumnName("progress_recorded_at");
        owned.Property(revision => revision.PercentComplete).HasColumnName("progress_percent_complete");
        owned.Property(revision => revision.BookedHours).HasColumnName("progress_booked_hours");
        owned.Property(revision => revision.Source).HasColumnName("progress_source").HasMaxLength(64);
        owned.Property(revision => revision.Fingerprint).HasColumnName("progress_fingerprint").HasMaxLength(64);
    }

    private static void ConfigurePlanSnapshot<T>(
        OwnedNavigationBuilder<T, Domain.ValueObjects.PlanSnapshot> owned) where T : class
    {
        owned.Property(plan => plan.PlannedStart).HasColumnName("plan_planned_start");
        owned.Property(plan => plan.PlannedFinish).HasColumnName("plan_planned_finish");
        owned.Property(plan => plan.PlannedHours).HasColumnName("plan_planned_hours");
        owned.Property(plan => plan.ProfileFingerprint).HasColumnName("plan_profile_fingerprint").HasMaxLength(64);
        owned.Property(plan => plan.CalculationRunId).HasColumnName("plan_calculation_run_id").HasMaxLength(64);
        owned.Property(plan => plan.Fingerprint).HasColumnName("plan_fingerprint").HasMaxLength(64);
    }
}
