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
            ToApprovalValues(entity));

    public static ApprovalRecord ToDomain(ApprovalRecordEntity entity) =>
        ApprovalRecord.Rehydrate(
            entity.Id,
            new TaskId(entity.TaskId),
            entity.ApprovalDay,
            new UserName(entity.ApprovedBy),
            entity.ApprovedAtUtc,
            ToApprovalValues(entity));

    public static void ApplyDomain(ActiveTaskEntity entity, ActiveTask task)
    {
        entity.Title = task.Title.Value;
        entity.ActivityCode = task.ActivityCode.Value;
        ApplyApprovalValues(entity, task.CurrentValues);
    }

    public static ApprovalRecordEntity ToEntity(ApprovalRecord record) =>
        new()
        {
            Id = record.Id,
            TaskId = record.TaskId.Value,
            ApprovalDay = record.ApprovalDay,
            ApprovedBy = record.ApprovedBy.Value,
            ApprovedAtUtc = record.ApprovedAtUtc,
            HoursToGo = record.ApprovedValues.HoursToGo,
            PlannedStart = record.ApprovedValues.PlannedStart,
            PlannedFinish = record.ApprovedValues.PlannedFinish,
            AssignedUser = record.ApprovedValues.AssignedUser.Value,
        };

    public static void ApplyDomain(ApprovalRecordEntity entity, ApprovalRecord record)
    {
        entity.ApprovedBy = record.ApprovedBy.Value;
        entity.ApprovedAtUtc = record.ApprovedAtUtc;
        ApplyApprovalValues(entity, record.ApprovedValues);
    }

    private static ApprovalValues ToApprovalValues(ActiveTaskEntity entity) =>
        new(
            entity.HoursToGo,
            entity.PlannedStart,
            entity.PlannedFinish,
            new UserName(entity.AssignedUser));

    private static ApprovalValues ToApprovalValues(ApprovalRecordEntity entity) =>
        new(
            entity.HoursToGo,
            entity.PlannedStart,
            entity.PlannedFinish,
            new UserName(entity.AssignedUser));

    private static void ApplyApprovalValues(ActiveTaskEntity entity, ApprovalValues values)
    {
        entity.HoursToGo = values.HoursToGo;
        entity.PlannedStart = values.PlannedStart;
        entity.PlannedFinish = values.PlannedFinish;
        entity.AssignedUser = values.AssignedUser.Value;
    }

    private static void ApplyApprovalValues(ApprovalRecordEntity entity, ApprovalValues values)
    {
        entity.HoursToGo = values.HoursToGo;
        entity.PlannedStart = values.PlannedStart;
        entity.PlannedFinish = values.PlannedFinish;
        entity.AssignedUser = values.AssignedUser.Value;
    }
}
