using Platform.Shared.Domain;

namespace HourApprovals.Domain.ValueObjects;

public sealed record ApprovalValues(
    decimal HoursToGo,
    DateOnly? PlannedStart,
    DateOnly? PlannedFinish,
    UserName AssignedUser)
{
    public bool Matches(ApprovalValues? other)
    {
        if (other is null)
        {
            return false;
        }

        return HoursToGo == other.HoursToGo
            && PlannedStart == other.PlannedStart
            && PlannedFinish == other.PlannedFinish
            && AssignedUser == other.AssignedUser;
    }
}
