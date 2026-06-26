using PlanningApprovals.Domain.Enums;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

public sealed class ForemanApprovalBatch
{
    private List<ApprovalPublicId> _requestPublicIds = [];

    private ForemanApprovalBatch()
    {
    }

    public ForemanApprovalBatch(
        ApprovalPublicId publicId,
        ProjectId projectId,
        PersonId foremanPersonId,
        ScopeDescription scopeDescription,
        DateTimeOffset openedAt)
    {
        PublicId = publicId;
        ProjectId = projectId;
        ForemanPersonId = foremanPersonId;
        ScopeDescription = scopeDescription;
        OpenedAt = openedAt;
        Status = ForemanApprovalBatchStatus.Open;
    }

    public int Id { get; private set; }

    public ApprovalPublicId PublicId { get; private init; }

    public ProjectId ProjectId { get; private init; }

    public PersonId ForemanPersonId { get; private init; }

    public ScopeDescription ScopeDescription { get; private init; }

    public ForemanApprovalBatchStatus Status { get; private set; }

    public DateTimeOffset OpenedAt { get; private init; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public IReadOnlyList<ApprovalPublicId> RequestPublicIds => _requestPublicIds;

    public static ForemanApprovalBatch Open(
        ProjectId projectId,
        PersonId foremanPersonId,
        ScopeDescription scopeDescription,
        DateTimeOffset openedAt) =>
        new(new ApprovalPublicId(Guid.NewGuid()), projectId, foremanPersonId, scopeDescription, openedAt);

    public void AddRequests(IEnumerable<ApprovalPublicId> requestPublicIds)
    {
        if (Status != ForemanApprovalBatchStatus.Open)
        {
            throw new InvalidOperationException("Cannot add requests to a closed batch.");
        }

        foreach (ApprovalPublicId requestPublicId in requestPublicIds)
        {
            if (!_requestPublicIds.Contains(requestPublicId))
            {
                _requestPublicIds.Add(requestPublicId);
            }
        }
    }

    public void Submit(DateTimeOffset submittedAt)
    {
        if (Status != ForemanApprovalBatchStatus.Open)
        {
            throw new InvalidOperationException("Batch is already closed.");
        }

        if (_requestPublicIds.Count == 0)
        {
            throw new InvalidOperationException("Cannot submit an empty batch.");
        }

        Status = ForemanApprovalBatchStatus.Submitted;
        SubmittedAt = submittedAt;
    }
}

