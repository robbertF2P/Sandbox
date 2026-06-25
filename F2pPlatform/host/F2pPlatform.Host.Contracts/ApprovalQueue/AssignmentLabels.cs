using Platform.Shared.Domain;

namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record AssignmentLabels(
    string Title,
    ActivityCode ActivityCode,
    string OrganisationLabel,
    string ProjectLabel,
    int TaskNumber = 0,
    string LocationPath = "",
    string DisciplineLabel = "",
    int TeamCount = 0,
    decimal TotalHoursBooked = 0m);
