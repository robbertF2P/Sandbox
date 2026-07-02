namespace HourApprovals.Infrastructure.Persistence.Entities;

public sealed class ApprovalRecordEntity
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public ActiveTaskEntity Task { get; set; } = null!;

    public DateOnly ApprovalDay { get; set; }

    public string ApprovedBy { get; set; } = string.Empty;

    public DateTimeOffset ApprovedAtUtc { get; set; }

    public decimal HoursToGo { get; set; }

    public DateOnly? PlannedStart { get; set; }

    public DateOnly? PlannedFinish { get; set; }

    public string AssignedUser { get; set; } = string.Empty;
}
