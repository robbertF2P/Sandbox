namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record ApprovalQueueFilter(
    IReadOnlyList<int> OrganisationIds,
    IReadOnlyList<SubmissionCategory> SubmissionCategories,
    string? Search);
