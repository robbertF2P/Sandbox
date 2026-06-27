using F2pPlatform.Host.Contracts.ApprovalQueue;
using Platform.Shared.Domain;

namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record ApprovalQueueFilter(
    IReadOnlyList<OrganisationId> OrganisationIds,
    IReadOnlyList<SubmissionCategory> SubmissionCategories,
    string? Search,
    TimeRangePreset TimeRange = TimeRangePreset.CurrentWeek);
