namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record AssignmentLabels(
    string Title,
    string ActivityCode,
    string OrganisationLabel,
    string ProjectLabel);
