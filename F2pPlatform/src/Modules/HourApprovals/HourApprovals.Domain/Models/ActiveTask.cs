using HourApprovals.Domain.ValueObjects;
using Platform.Shared.Domain;

namespace HourApprovals.Domain.Models;

public sealed class ActiveTask
{
    private ActiveTask(
        TaskId id,
        string title,
        ActivityCode activityCode,
        ApprovalValues currentValues,
        bool isActiveForCurrentUser)
    {
        Id = id;
        Title = title;
        ActivityCode = activityCode;
        CurrentValues = currentValues;
        IsActiveForCurrentUser = isActiveForCurrentUser;
    }

    public TaskId Id { get; }

    public string Title { get; }

    public ActivityCode ActivityCode { get; }

    public ApprovalValues CurrentValues { get; private set; }

    public bool IsActiveForCurrentUser { get; private set; }

    public static ActiveTask Create(
        TaskId id,
        string title,
        ActivityCode activityCode,
        ApprovalValues currentValues,
        bool isActiveForCurrentUser) =>
        new(id, title, activityCode, currentValues, isActiveForCurrentUser);

    public void UpdateValues(ApprovalValues values, bool isActiveForCurrentUser)
    {
        CurrentValues = values;
        IsActiveForCurrentUser = isActiveForCurrentUser;
    }
}
