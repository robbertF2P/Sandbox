namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record ApprovalQueueFilter(
    IReadOnlyList<OrganisationId> OrganisationIds,
    IReadOnlyList<SubmissionCategory> SubmissionCategories,
    string? Search);
