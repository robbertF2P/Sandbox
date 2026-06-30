using HourApprovals.Domain.ValueObjects;
using Platform.Shared.Domain;

namespace HourApprovals.Domain.Models;

public sealed class ActiveTask
{
    private ActiveTask(
        TaskId id,
        TaskTitle title,
        ActivityCode activityCode,
        ApprovalValues currentValues)
    {
        Id = id;
        Title = title;
        ActivityCode = activityCode;
        CurrentValues = currentValues;
    }

    public TaskId Id { get; }

    public TaskTitle Title { get; }

    public ActivityCode ActivityCode { get; }

    public ApprovalValues CurrentValues { get; private set; }

    public static ActiveTask Create(
        TaskId id,
        TaskTitle title,
        ActivityCode activityCode,
        ApprovalValues currentValues) =>
        new(id, title, activityCode, currentValues);

    public void UpdateValues(ApprovalValues values) => CurrentValues = values;
}
