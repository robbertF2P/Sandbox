using HourApprovals.Domain.Audit;
using HourApprovals.Domain.ValueObjects;
using Platform.Shared.Domain;

namespace HourApprovals.Domain.Models;

public sealed class ApprovalRecord : IRecordAudit
{
    private ApprovalRecord(
        Guid id,
        TaskId taskId,
        UserName approvedBy,
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

    public TaskId TaskId { get; }

    public UserName ApprovedBy { get; }

    public DateTimeOffset ApprovedAtUtc { get; }

    public ApprovalValues ApprovedValues { get; }

    string IRecordAudit.CreatedBy => ApprovedBy.Value;

    DateTimeOffset IRecordAudit.CreatedAtUtc => ApprovedAtUtc;

    public static ApprovalRecord Create(
        TaskId taskId,
        UserName approvedBy,
        DateTimeOffset approvedAtUtc,
        ApprovalValues approvedValues) =>
        new(
            Guid.NewGuid(),
            taskId,
            approvedBy,
            approvedAtUtc,
            approvedValues);

    public static ApprovalRecord Rehydrate(
        Guid id,
        TaskId taskId,
        UserName approvedBy,
        DateTimeOffset approvedAtUtc,
        ApprovalValues approvedValues) =>
        new(id, taskId, approvedBy, approvedAtUtc, approvedValues);
}
