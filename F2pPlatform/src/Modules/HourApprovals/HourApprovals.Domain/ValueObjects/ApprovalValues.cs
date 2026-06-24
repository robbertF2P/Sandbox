namespace HourApprovals.Domain.ValueObjects;

public sealed record ApprovalValues(
    decimal HoursToGo,
    decimal Progress,
    decimal WorkedHours,
    DateOnly? PlannedStart,
    DateOnly? PlannedFinish)
{
    public bool Matches(ApprovalValues? other)
    {
        if (other is null)
        {
            return false;
        }

        return HoursToGo == other.HoursToGo
            && Progress == other.Progress
            && WorkedHours == other.WorkedHours
            && PlannedStart == other.PlannedStart
            && PlannedFinish == other.PlannedFinish;
    }
}
