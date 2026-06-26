using F2pPlatform.Host.Contracts.ApprovalQueue;
using Platform.Shared.Domain;

namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record ApprovalQueueRow(
    TaskId TaskId,
    AssignmentId AssignmentId,
    OrganisationId OrganisationId,
    AssignmentLabels Labels,
    decimal HoursWorkedInWindow,
    SubmissionCategory SubmissionCategory,
    ApprovalState ApprovalState,
    ApprovalProgressValues CurrentValues,
    ApprovalProgressValues LookbackBaseline,
    LastSubmission? LastSubmission);
