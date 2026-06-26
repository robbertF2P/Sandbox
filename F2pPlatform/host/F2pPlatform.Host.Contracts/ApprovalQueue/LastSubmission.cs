using Platform.Shared.Domain;

namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record LastSubmission(
    UserName SubmittedBy,
    DateTimeOffset SubmittedAtUtc,
    ApprovalProgressValues? ApprovedValues = null);
