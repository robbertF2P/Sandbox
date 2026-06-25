namespace F2pPlatform.Host.Contracts.ApprovalQueue;

public sealed record ApprovalProgressValues(
    decimal HoursToGo,
    decimal Progress,
    decimal WorkedHours,
    DateOnly? PlannedStart,
    DateOnly? PlannedFinish);
