using F2pPlatform.Host.Contracts.ApprovalQueue;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Hours;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Planning;
using F2pPlatform.Host.Core.ApprovalQueue;

namespace HourApprovals.Unit.Tests;

[Trait("Module", "HourApprovals")]
[Trait("Tier", "Unit")]
public sealed class ApprovalQueueComposerShould
{
    private static readonly Guid TaskA = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid TaskB = Guid.Parse("11111111-1111-1111-1111-111111111102");

  [Fact]
    public void ResolveCategory_WorkedOn_WhenHoursInWindow()
    {
        SubmissionCategory category = ApprovalQueueComposer.ResolveCategory(
            hoursInWindow: 2m,
            isActiveAssignment: true,
            lastSubmittedAtUtc: DateTimeOffset.UtcNow);

        Assert.Equal(SubmissionCategory.WorkedOn, category);
    }

    [Fact]
    public void ResolveCategory_OtherActive_WhenActiveButNoHours()
    {
        SubmissionCategory category = ApprovalQueueComposer.ResolveCategory(
            hoursInWindow: 0m,
            isActiveAssignment: true,
            lastSubmittedAtUtc: DateTimeOffset.UtcNow);

        Assert.Equal(SubmissionCategory.OtherActive, category);
    }

    [Fact]
    public void Compose_FiltersNeverSubmitted()
    {
        var assignments = new List<PlanningAssignmentRow>
        {
            CreateAssignment(TaskA, "Wiring"),
            CreateAssignment(TaskB, "Ventilation"),
        };

        var hours = new Dictionary<Guid, decimal>
        {
            [TaskA] = 4.5m,
            [TaskB] = 0m,
        };

        var snapshots = new Dictionary<Guid, HourSubmissionSnapshot>
        {
            [TaskA] = CreateSnapshot(TaskA, lastSubmitted: null),
            [TaskB] = CreateSnapshot(TaskB, lastSubmitted: null),
        };

        var filter = new ApprovalQueueFilter([], [SubmissionCategory.NeverSubmitted], null);

        IReadOnlyList<ApprovalQueueRow> rows = ApprovalQueueComposer.Compose(
            assignments,
            hours,
            snapshots,
            filter);

        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.Null(row.LastSubmittedAtUtc));
    }

    [Fact]
    public void Compose_FiltersWorkedOnOnly()
    {
        var assignments = new List<PlanningAssignmentRow>
        {
            CreateAssignment(TaskA, "Wiring"),
            CreateAssignment(TaskB, "Ventilation"),
        };

        var hours = new Dictionary<Guid, decimal>
        {
            [TaskA] = 4.5m,
            [TaskB] = 0m,
        };

        var snapshots = new Dictionary<Guid, HourSubmissionSnapshot>
        {
            [TaskA] = CreateSnapshot(TaskA, lastSubmitted: null),
            [TaskB] = CreateSnapshot(TaskB, lastSubmitted: null),
        };

        var filter = new ApprovalQueueFilter([], [SubmissionCategory.WorkedOn], null);

        IReadOnlyList<ApprovalQueueRow> rows = ApprovalQueueComposer.Compose(
            assignments,
            hours,
            snapshots,
            filter);

        Assert.Single(rows);
        Assert.Equal(TaskA, rows[0].TaskId);
        Assert.Equal(SubmissionCategory.WorkedOn, rows[0].SubmissionCategory);
    }

    private static PlanningAssignmentRow CreateAssignment(Guid taskId, string title) =>
        new(
            taskId,
            taskId,
            21,
            "21: Metal Shop",
            "NSMV Demo",
            title,
            "ACT",
            IsActiveAssignment: true);

    private static HourSubmissionSnapshot CreateSnapshot(Guid taskId, DateTimeOffset? lastSubmitted) =>
        new(
            taskId,
            "NotApproved",
            false,
            10m,
            20m,
            30m,
            null,
            null,
            lastSubmitted.HasValue ? "supervisor.demo" : null,
            lastSubmitted);
}
