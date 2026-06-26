using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Infrastructure;

public sealed class PlanningApprovalsDbContext(DbContextOptions<PlanningApprovalsDbContext> options)
    : DbContext(options)
{
    public DbSet<AssignmentApprovalRequest> ApprovalRequests => Set<AssignmentApprovalRequest>();

    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();

    public DbSet<ApprovedPlanSnapshot> ApprovedPlanSnapshots => Set<ApprovedPlanSnapshot>();

    public DbSet<ForemanApprovalBatch> ApprovalBatches => Set<ForemanApprovalBatch>();

    public DbSet<AssignmentPlanningCheckpoint> PlanningCheckpoints => Set<AssignmentPlanningCheckpoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ValueConverter<ApprovalPublicId, Guid> publicIdConverter = new(
            id => id.Value,
            value => new ApprovalPublicId(value));

        ValueConverter<ProjectId, long> projectIdConverter = new(
            id => id.Value,
            value => new ProjectId(value));

        ValueConverter<AssignmentId, long> assignmentIdConverter = new(
            id => id.Value,
            value => new AssignmentId(value));

        ValueConverter<PersonId, long> personIdConverter = new(
            id => id.Value,
            value => new PersonId(value));

        ValueConverter<DecisionComment?, string?> commentConverter = new(
            comment => comment.HasValue ? comment.Value.Value : null,
            value => value == null ? null : new DecisionComment(value));

        modelBuilder.Entity<AssignmentApprovalRequest>(entity =>
        {
            entity.ToTable("assignment_approval_requests");
            entity.HasKey(request => request.Id);
            entity.HasIndex(request => request.PublicId).IsUnique();
            entity.HasIndex(request => new { request.ProjectId, request.Status, request.OpenedAt });
            entity.HasIndex(request => new { request.AssignmentId, request.Status });

            entity.Property(request => request.PublicId).HasConversion(publicIdConverter).IsRequired();
            entity.Property(request => request.ProjectId).HasConversion(projectIdConverter).IsRequired();
            entity.Property(request => request.AssignmentId).HasConversion(assignmentIdConverter).IsRequired();
            entity.Property(request => request.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(request => request.RequiredBecause).HasConversion<string>().HasMaxLength(32);
            entity.Property(request => request.OpenedByProcess)
                .HasConversion(process => process.Value, value => new ProcessName(value))
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(request => request.LastApprovedSnapshotId).HasConversion(publicIdConverter);

            ConfigureProgressRevision(entity.OwnsOne(request => request.ProgressRevision), assignmentIdConverter, "progress_");
            ConfigurePlanSnapshot(entity.OwnsOne(request => request.ProposedPlan), "plan_");
            entity.Property(request => request.LookbackCapturedAt).HasColumnName("lookback_captured_at");
            ConfigureProgressRevision(entity.OwnsOne(request => request.LookbackProgress), assignmentIdConverter, "lookback_progress_");
            ConfigurePlanSnapshot(entity.OwnsOne(request => request.LookbackPlan), "lookback_plan_");
        });

        modelBuilder.Entity<AssignmentPlanningCheckpoint>(entity =>
        {
            entity.ToTable("assignment_planning_checkpoints");
            entity.HasKey(checkpoint => checkpoint.Id);
            entity.HasIndex(checkpoint => checkpoint.PublicId).IsUnique();
            entity.HasIndex(checkpoint => new { checkpoint.AssignmentId, checkpoint.CapturedAt });

            entity.Property(checkpoint => checkpoint.PublicId).HasConversion(publicIdConverter).IsRequired();
            entity.Property(checkpoint => checkpoint.AssignmentId).HasConversion(assignmentIdConverter).IsRequired();
            entity.Property(checkpoint => checkpoint.CapturedAt).IsRequired();
            entity.Property(checkpoint => checkpoint.CaptureSource)
                .HasConversion(source => source.Value, value => new CaptureSource(value))
                .IsRequired()
                .HasMaxLength(64);

            ConfigureProgressRevision(entity.OwnsOne(checkpoint => checkpoint.ProgressRevision), assignmentIdConverter, "progress_");
            ConfigurePlanSnapshot(entity.OwnsOne(checkpoint => checkpoint.PlanSnapshot), "plan_");
        });

        modelBuilder.Entity<ApprovalDecision>(entity =>
        {
            entity.ToTable("approval_decisions");
            entity.HasKey(decision => decision.Id);
            entity.HasIndex(decision => decision.PublicId).IsUnique();
            entity.HasIndex(decision => new { decision.AssignmentId, decision.DecidedAt });
            entity.HasIndex(decision => decision.RequestPublicId);

            entity.Property(decision => decision.PublicId).HasConversion(publicIdConverter).IsRequired();
            entity.Property(decision => decision.RequestPublicId).HasConversion(publicIdConverter).IsRequired();
            entity.Property(decision => decision.AssignmentId).HasConversion(assignmentIdConverter).IsRequired();
            entity.Property(decision => decision.Decision).HasConversion<string>().HasMaxLength(16);
            entity.Property(decision => decision.DecidedByPersonId).HasConversion(personIdConverter).IsRequired();
            entity.Property(decision => decision.CorrelationId)
                .HasConversion(id => id.Value, value => new CorrelationId(value))
                .IsRequired()
                .HasMaxLength(64);
            entity.Property(decision => decision.Comment)
                .HasConversion(commentConverter)
                .HasMaxLength(2000);
            entity.Property(decision => decision.BatchPublicId).HasConversion(publicIdConverter);

            ConfigureProgressRevision(entity.OwnsOne(decision => decision.ProgressRevisionAtDecision), assignmentIdConverter, "decision_progress_");
            ConfigurePlanSnapshot(entity.OwnsOne(decision => decision.ProposedPlanAtDecision), "decision_plan_");
        });

        modelBuilder.Entity<ApprovedPlanSnapshot>(entity =>
        {
            entity.ToTable("approved_plan_snapshots");
            entity.HasKey(snapshot => snapshot.Id);
            entity.HasIndex(snapshot => snapshot.PublicId).IsUnique();
            entity.HasIndex(snapshot => new { snapshot.AssignmentId, snapshot.ApprovedAt });

            entity.Property(snapshot => snapshot.PublicId).HasConversion(publicIdConverter).IsRequired();
            entity.Property(snapshot => snapshot.DecisionPublicId).HasConversion(publicIdConverter).IsRequired();
            entity.Property(snapshot => snapshot.AssignmentId).HasConversion(assignmentIdConverter).IsRequired();
            entity.Property(snapshot => snapshot.ApprovedByPersonId).HasConversion(personIdConverter).IsRequired();

            ConfigureProgressRevision(entity.OwnsOne(snapshot => snapshot.ProgressRevision), assignmentIdConverter, "snapshot_progress_");
            ConfigurePlanSnapshot(entity.OwnsOne(snapshot => snapshot.PlanSnapshot), "snapshot_plan_");
        });

        modelBuilder.Entity<ForemanApprovalBatch>(entity =>
        {
            entity.ToTable("foreman_approval_batches");
            entity.HasKey(batch => batch.Id);
            entity.HasIndex(batch => batch.PublicId).IsUnique();

            entity.Property(batch => batch.PublicId).HasConversion(publicIdConverter).IsRequired();
            entity.Property(batch => batch.ProjectId).HasConversion(projectIdConverter).IsRequired();
            entity.Property(batch => batch.ForemanPersonId).HasConversion(personIdConverter).IsRequired();
            entity.Property(batch => batch.ScopeDescription)
                .HasConversion(scope => scope.Value, value => new ScopeDescription(value))
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(batch => batch.Status).HasConversion<string>().HasMaxLength(16);

            entity.Property<List<ApprovalPublicId>>("_requestPublicIds")
                .HasColumnName("request_public_ids")
                .HasConversion(
                    ids => string.Join('|', ids.Select(id => id.Value)),
                    value => string.IsNullOrEmpty(value)
                        ? new List<ApprovalPublicId>()
                        : value.Split('|', StringSplitOptions.RemoveEmptyEntries)
                            .Select(part => new ApprovalPublicId(Guid.Parse(part)))
                            .ToList());
        });
    }

    private static void ConfigureProgressRevision<T>(
        OwnedNavigationBuilder<T, ProgressRevisionRef> owned,
        ValueConverter<AssignmentId, long> assignmentIdConverter,
        string columnPrefix = "") where T : class
    {
        ValueConverter<ProgressRevisionId, long> revisionIdConverter = new(
            id => id.Value,
            value => new ProgressRevisionId(value));

        owned.Property(revision => revision.AssignmentId).HasColumnName($"{columnPrefix}assignment_id").HasConversion(assignmentIdConverter);
        owned.Property(revision => revision.RevisionId).HasColumnName($"{columnPrefix}revision_id").HasConversion(revisionIdConverter);
        owned.Property(revision => revision.RecordedAt).HasColumnName($"{columnPrefix}recorded_at");
        owned.Property(revision => revision.PercentComplete).HasColumnName($"{columnPrefix}percent_complete");
        owned.Property(revision => revision.BookedHours).HasColumnName($"{columnPrefix}booked_hours");
        owned.Property(revision => revision.Source)
            .HasColumnName($"{columnPrefix}source")
            .HasConversion(source => source.Value, value => new ProgressSource(value))
            .HasMaxLength(64);
        owned.Property(revision => revision.Fingerprint).HasColumnName($"{columnPrefix}fingerprint").HasMaxLength(64);
    }

    private static void ConfigurePlanSnapshot<T>(
        OwnedNavigationBuilder<T, PlanSnapshot> owned,
        string columnPrefix = "") where T : class
    {
        owned.Property(plan => plan.PlannedStart).HasColumnName($"{columnPrefix}planned_start");
        owned.Property(plan => plan.PlannedFinish).HasColumnName($"{columnPrefix}planned_finish");
        owned.Property(plan => plan.PlannedHours).HasColumnName($"{columnPrefix}planned_hours");
        owned.Property(plan => plan.ProfileFingerprint)
            .HasColumnName($"{columnPrefix}profile_fingerprint")
            .HasConversion(fingerprint => fingerprint.Value, value => new ProfileFingerprint(value))
            .HasMaxLength(64);
        owned.Property(plan => plan.CalculationRunId)
            .HasColumnName($"{columnPrefix}calculation_run_id")
            .HasConversion(runId => runId.Value, value => new CalculationRunId(value))
            .HasMaxLength(64);
        owned.Property(plan => plan.Fingerprint).HasColumnName($"{columnPrefix}fingerprint").HasMaxLength(64);
    }
}
