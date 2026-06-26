using Platform.Shared.Domain;

namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record AssignmentLabels(
    TaskTitle Title,
    ActivityCode ActivityCode,
    OrganisationLabel OrganisationLabel,
    ProjectLabel ProjectLabel,
    TaskNumber TaskNumber = default,
    string LocationPath = "",
    string DisciplineLabel = "",
    int TeamCount = 0,
    decimal TotalHoursBooked = 0m);
