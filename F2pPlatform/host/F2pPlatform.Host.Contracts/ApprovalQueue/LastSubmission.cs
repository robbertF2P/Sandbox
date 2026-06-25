namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record LastSubmission(string SubmittedBy, DateTimeOffset SubmittedAtUtc);
