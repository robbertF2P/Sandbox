using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

public sealed class AssignmentApprovalRecord
{
    private AssignmentApprovalRecord()
    {
        ApprovedValues = new ApprovalValues(0m, null, null, AssignedUser.From("unknown"));
    }

    private AssignmentApprovalRecord(
        Guid id,
        AssignmentId assignmentId,
        DateOnly approvalDay,
        PersonId approvedBy,
        DateTimeOffset approvedAtUtc,
        ApprovalValues approvedValues)
    {
        Id = id;
        AssignmentId = assignmentId;
        ApprovalDay = approvalDay;
        ApprovedBy = approvedBy;
        ApprovedAtUtc = approvedAtUtc;
        ApprovedValues = approvedValues;
    }

    public Guid Id { get; private set; }

    public AssignmentId AssignmentId { get; private set; }

    public DateOnly ApprovalDay { get; private set; }

    public PersonId ApprovedBy { get; private set; }

    public DateTimeOffset ApprovedAtUtc { get; private set; }

    public ApprovalValues ApprovedValues { get; private set; }

    public static AssignmentApprovalRecord Create(
        AssignmentId assignmentId,
        DateOnly approvalDay,
        PersonId approvedBy,
        DateTimeOffset approvedAtUtc,
        ApprovalValues approvedValues) =>
        new(
            Guid.NewGuid(),
            assignmentId,
            approvalDay,
            approvedBy,
            approvedAtUtc,
            approvedValues);

    public void Update(PersonId approvedBy, DateTimeOffset approvedAtUtc, ApprovalValues approvedValues)
    {
        ApprovedBy = approvedBy;
        ApprovedAtUtc = approvedAtUtc;
        ApprovedValues = approvedValues;
    }
}
