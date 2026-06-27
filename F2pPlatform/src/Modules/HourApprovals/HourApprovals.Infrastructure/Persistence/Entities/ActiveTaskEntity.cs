namespace HourApprovals.Infrastructure.Persistence.Entities;

public sealed class ActiveTaskEntity
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ActivityCode { get; set; } = string.Empty;

    public decimal HoursToGo { get; set; }

    public decimal Progress { get; set; }

    public decimal WorkedHours { get; set; }

    public DateOnly? PlannedStart { get; set; }

    public DateOnly? PlannedFinish { get; set; }

    public bool IsActiveForCurrentUser { get; set; }

    public List<ApprovalRecordEntity> ApprovalRecords { get; set; } = [];
}
