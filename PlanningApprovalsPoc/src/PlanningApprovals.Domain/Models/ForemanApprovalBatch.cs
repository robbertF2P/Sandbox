namespace PlanningApprovals.Domain.Models;

public sealed class ForemanApprovalBatch
{
    private List<Guid> _requestPublicIds = [];

    private ForemanApprovalBatch()
    {
    }

    public ForemanApprovalBatch(
        Guid publicId,
        long projectId,
        long foremanPersonId,
        string scopeDescription,
        DateTimeOffset openedAt)
    {
        if (string.IsNullOrWhiteSpace(scopeDescription))
        {
            throw new ArgumentException("Scope description is required.", nameof(scopeDescription));
        }

        PublicId = publicId;
        ProjectId = projectId;
        ForemanPersonId = foremanPersonId;
        ScopeDescription = scopeDescription;
        OpenedAt = openedAt;
        Status = ForemanApprovalBatchStatus.Open;
    }

    public int Id { get; private set; }

    public Guid PublicId { get; private init; }

    public long ProjectId { get; private init; }

    public long ForemanPersonId { get; private init; }

    public string ScopeDescription { get; private init; } = string.Empty;

    public ForemanApprovalBatchStatus Status { get; private set; }

    public DateTimeOffset OpenedAt { get; private init; }

    public DateTimeOffset? SubmittedAt { get; private set; }

    public IReadOnlyList<Guid> RequestPublicIds => _requestPublicIds;

    public static ForemanApprovalBatch Open(
        long projectId,
        long foremanPersonId,
        string scopeDescription,
        DateTimeOffset openedAt) =>
        new(Guid.NewGuid(), projectId, foremanPersonId, scopeDescription, openedAt);

    public void AddRequests(IEnumerable<Guid> requestPublicIds)
    {
        if (Status != ForemanApprovalBatchStatus.Open)
        {
            throw new InvalidOperationException("Cannot add requests to a closed batch.");
        }

        foreach (Guid requestPublicId in requestPublicIds)
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

public enum ForemanApprovalBatchStatus
{
    Open = 0,
    Submitted = 1,
}
