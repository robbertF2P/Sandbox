namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record ApprovalQueueRow(
    Guid TaskId,
    Guid AssignmentId,
    string Title,
    string ActivityCode,
    string OrganisationLabel,
    string ProjectLabel,
    decimal HoursWorkedInWindow,
    SubmissionCategory SubmissionCategory,
    string ApprovalState,
    bool IsApproved,
    decimal HoursToGo,
    decimal Progress,
    decimal WorkedHours,
    string? PlannedStart,
    string? PlannedFinish,
    string? LastSubmittedBy,
    DateTimeOffset? LastSubmittedAtUtc);
