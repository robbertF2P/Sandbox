using HourApprovals.Domain.Audit;
using HourApprovals.Domain.ValueObjects;

namespace HourApprovals.Domain.Models;

public sealed class ApprovalRecord : IRecordAudit
{
    private ApprovalRecord(
        Guid id,
        Guid taskId,
        string approvedBy,
        DateTimeOffset approvedAtUtc,
        ApprovalValues approvedValues)
    {
        Id = id;
        TaskId = taskId;
        ApprovedBy = approvedBy;
        ApprovedAtUtc = approvedAtUtc;
        ApprovedValues = approvedValues;
    }

    public Guid Id { get; }

    public Guid TaskId { get; }

    public string ApprovedBy { get; }

    public DateTimeOffset ApprovedAtUtc { get; }

    public ApprovalValues ApprovedValues { get; }

    string IRecordAudit.CreatedBy => ApprovedBy;

    DateTimeOffset IRecordAudit.CreatedAtUtc => ApprovedAtUtc;

    public static ApprovalRecord Create(
        Guid taskId,
        string approvedBy,
        DateTimeOffset approvedAtUtc,
        ApprovalValues approvedValues) =>
        new(
            Guid.NewGuid(),
            taskId,
            approvedBy,
            approvedAtUtc,
            approvedValues);
}
