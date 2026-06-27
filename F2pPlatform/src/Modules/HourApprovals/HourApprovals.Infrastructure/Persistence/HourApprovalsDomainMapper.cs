using HourApprovals.Domain.Models;
using HourApprovals.Domain.ValueObjects;
using HourApprovals.Infrastructure.Persistence.Entities;
using Platform.Shared.Domain;

namespace HourApprovals.Infrastructure.Persistence;

internal static class HourApprovalsDomainMapper
{
    public static ActiveTask ToDomain(ActiveTaskEntity entity) =>
        ActiveTask.Create(
            new TaskId(entity.Id),
            new TaskTitle(entity.Title),
            new ActivityCode(entity.ActivityCode),
            ToApprovalValues(entity),
            entity.IsActiveForCurrentUser);

    public static ApprovalRecord ToDomain(ApprovalRecordEntity entity) =>
        ApprovalRecord.Rehydrate(
            entity.Id,
            new TaskId(entity.TaskId),
            new UserName(entity.ApprovedBy),
            entity.ApprovedAtUtc,
            ToApprovalValues(entity));

    public static void ApplyDomain(ActiveTaskEntity entity, ActiveTask task)
    {
        entity.Title = task.Title.Value;
        entity.ActivityCode = task.ActivityCode.Value;
        entity.IsActiveForCurrentUser = task.IsActiveForCurrentUser;
        ApplyApprovalValues(entity, task.CurrentValues);
    }

    public static ApprovalRecordEntity ToEntity(ApprovalRecord record) =>
        new()
        {
            Id = record.Id,
            TaskId = record.TaskId.Value,
            ApprovedBy = record.ApprovedBy.Value,
            ApprovedAtUtc = record.ApprovedAtUtc,
            HoursToGo = record.ApprovedValues.HoursToGo,
            Progress = record.ApprovedValues.Progress,
            WorkedHours = record.ApprovedValues.WorkedHours,
            PlannedStart = record.ApprovedValues.PlannedStart,
            PlannedFinish = record.ApprovedValues.PlannedFinish,
        };

    private static ApprovalValues ToApprovalValues(ActiveTaskEntity entity) =>
        new(
            entity.HoursToGo,
            entity.Progress,
            entity.WorkedHours,
            entity.PlannedStart,
            entity.PlannedFinish);

    private static ApprovalValues ToApprovalValues(ApprovalRecordEntity entity) =>
        new(
            entity.HoursToGo,
            entity.Progress,
            entity.WorkedHours,
            entity.PlannedStart,
            entity.PlannedFinish);

    private static void ApplyApprovalValues(ActiveTaskEntity entity, ApprovalValues values)
    {
        entity.HoursToGo = values.HoursToGo;
        entity.Progress = values.Progress;
        entity.WorkedHours = values.WorkedHours;
        entity.PlannedStart = values.PlannedStart;
        entity.PlannedFinish = values.PlannedFinish;
    }
}
