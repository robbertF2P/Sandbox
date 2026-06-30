using HourApprovals.Domain.Audit;
using HourApprovals.Domain.ValueObjects;
using Platform.Shared.Domain;

namespace HourApprovals.Domain.Models;

public sealed class ApprovalRecord : IRecordAudit
{
    private ApprovalRecord(
        Guid id,
        TaskId taskId,
        DateOnly approvalDay,
        UserName approvedBy,
        DateTimeOffset approvedAtUtc,
        ApprovalValues approvedValues)
    {
        Id = id;
        TaskId = taskId;
        ApprovalDay = approvalDay;
        ApprovedBy = approvedBy;
        ApprovedAtUtc = approvedAtUtc;
        ApprovedValues = approvedValues;
    }

    public Guid Id { get; }

    public TaskId TaskId { get; }

    public DateOnly ApprovalDay { get; }

    public UserName ApprovedBy { get; private set; }

    public DateTimeOffset ApprovedAtUtc { get; private set; }

    public ApprovalValues ApprovedValues { get; private set; }

    string IRecordAudit.CreatedBy => ApprovedBy.Value;

    DateTimeOffset IRecordAudit.CreatedAtUtc => ApprovedAtUtc;

    public static ApprovalRecord Create(
        TaskId taskId,
        DateOnly approvalDay,
        UserName approvedBy,
        DateTimeOffset approvedAtUtc,
        ApprovalValues approvedValues) =>
        new(
            Guid.NewGuid(),
            taskId,
            approvalDay,
            approvedBy,
            approvedAtUtc,
            approvedValues);

    public static ApprovalRecord Rehydrate(
        Guid id,
        TaskId taskId,
        DateOnly approvalDay,
        UserName approvedBy,
        DateTimeOffset approvedAtUtc,
        ApprovalValues approvedValues) =>
        new(id, taskId, approvalDay, approvedBy, approvedAtUtc, approvedValues);

    public void Update(UserName approvedBy, DateTimeOffset approvedAtUtc, ApprovalValues approvedValues)
    {
        ApprovedBy = approvedBy;
        ApprovedAtUtc = approvedAtUtc;
        ApprovedValues = approvedValues;
    }
}
