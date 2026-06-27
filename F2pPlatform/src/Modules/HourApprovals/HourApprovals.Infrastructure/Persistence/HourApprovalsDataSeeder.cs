using HourApprovals.Domain.Models;
using HourApprovals.Domain.ValueObjects;
using HourApprovals.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Platform.Shared.Domain;

namespace HourApprovals.Infrastructure.Persistence;

internal static class HourApprovalsDataSeeder
{
    public static async Task SeedAsync(HourApprovalsDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.ActiveTasks.AnyAsync(cancellationToken))
        {
            return;
        }

        ActiveTaskEntity taskA = CreateTaskEntity(
            Guid.Parse("11111111-1111-1111-1111-111111111101"),
            "Hull 247 — Block 204 wiring",
            "ACT-204-WIR",
            new ApprovalValues(12.5m, 35m, 48m, new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 24)),
            isActiveForCurrentUser: true);

        ActiveTaskEntity taskB = CreateTaskEntity(
            Guid.Parse("11111111-1111-1111-1111-111111111102"),
            "Engine room ventilation",
            "ACT-ENG-VNT",
            new ApprovalValues(20m, 10m, 8m, new DateOnly(2026, 6, 12), new DateOnly(2026, 7, 1)),
            isActiveForCurrentUser: false);

        ActiveTaskEntity taskC = CreateTaskEntity(
            Guid.Parse("11111111-1111-1111-1111-111111111103"),
            "Deck coating inspection",
            "ACT-DCK-COT",
            new ApprovalValues(6m, 72m, 54m, new DateOnly(2026, 5, 28), new DateOnly(2026, 6, 18)),
            isActiveForCurrentUser: true);

        dbContext.ActiveTasks.AddRange(taskA, taskB, taskC);

        ApprovalRecord seededApproval = ApprovalRecord.Create(
            new TaskId(taskC.Id),
            new UserName("supervisor.demo"),
            DateTimeOffset.UtcNow.AddDays(-1),
            new ApprovalValues(
                taskC.HoursToGo,
                taskC.Progress,
                taskC.WorkedHours,
                taskC.PlannedStart,
                taskC.PlannedFinish));

        dbContext.ApprovalRecords.Add(HourApprovalsDomainMapper.ToEntity(seededApproval));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ActiveTaskEntity CreateTaskEntity(
        Guid id,
        string title,
        string activityCode,
        ApprovalValues values,
        bool isActiveForCurrentUser)
    {
        var entity = new ActiveTaskEntity
        {
            Id = id,
            Title = title,
            ActivityCode = activityCode,
            IsActiveForCurrentUser = isActiveForCurrentUser,
        };

        HourApprovalsDomainMapper.ApplyDomain(
            entity,
            ActiveTask.Create(
                new TaskId(id),
                new TaskTitle(title),
                new ActivityCode(activityCode),
                values,
                isActiveForCurrentUser));

        return entity;
    }
}
