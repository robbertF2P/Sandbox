using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Models;

public sealed class ActiveAssignment
{
    private ActiveAssignment()
    {
        Title = string.Empty;
        ActivityCode = string.Empty;
        CurrentValues = new ApprovalValues(0m, null, null, AssignedUser.From("unknown"));
    }

    private ActiveAssignment(
        AssignmentId id,
        string title,
        string activityCode,
        ApprovalValues currentValues)
    {
        Id = id;
        Title = title;
        ActivityCode = activityCode;
        CurrentValues = currentValues;
    }

    public AssignmentId Id { get; private set; }

    public string Title { get; private set; }

    public string ActivityCode { get; private set; }

    public ApprovalValues CurrentValues { get; private set; }

    public static ActiveAssignment Create(
        AssignmentId id,
        string title,
        string activityCode,
        ApprovalValues currentValues) =>
        new(id, title, activityCode, currentValues);

    public void UpdateValues(ApprovalValues values) => CurrentValues = values;
}
