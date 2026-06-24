using HourApprovals.Domain.ValueObjects;

namespace HourApprovals.Domain.Models;

public sealed class ActiveTask
{
    private ActiveTask(
        Guid id,
        string title,
        string activityCode,
        ApprovalValues currentValues,
        bool isActiveForCurrentUser)
    {
        Id = id;
        Title = title;
        ActivityCode = activityCode;
        CurrentValues = currentValues;
        IsActiveForCurrentUser = isActiveForCurrentUser;
    }

    public Guid Id { get; }

    public string Title { get; }

    public string ActivityCode { get; }

    public ApprovalValues CurrentValues { get; private set; }

    public bool IsActiveForCurrentUser { get; private set; }

    public static ActiveTask Create(
        Guid id,
        string title,
        string activityCode,
        ApprovalValues currentValues,
        bool isActiveForCurrentUser) =>
        new(id, title, activityCode, currentValues, isActiveForCurrentUser);

    public void UpdateValues(ApprovalValues values, bool isActiveForCurrentUser)
    {
        CurrentValues = values;
        IsActiveForCurrentUser = isActiveForCurrentUser;
    }
}
