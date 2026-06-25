using Platform.Shared.Domain;

namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record AssignmentLabels(
    string Title,
    ActivityCode ActivityCode,
    string OrganisationLabel,
    string ProjectLabel);
