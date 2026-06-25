using System.Collections.Concurrent;
using HourApprovals.Application.Ports;
using HourApprovals.Domain.Models;
using HourApprovals.Domain.ValueObjects;

namespace HourApprovals.Infrastructure;

internal sealed class InMemoryHourApprovalsRepository : IHourApprovalsRepository
{
    private readonly ConcurrentDictionary<TaskId, ActiveTask> _tasks = new();
    private readonly ConcurrentDictionary<TaskId, List<ApprovalRecord>> _approvals = new();

    public InMemoryHourApprovalsRepository()
    {
        Seed();
    }

    public Task<IReadOnlyList<ActiveTask>> ListTasksAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ActiveTask>>(_tasks.Values.OrderBy(task => task.Title).ToList());

    public Task<ActiveTask?> GetTaskAsync(TaskId taskId, CancellationToken cancellationToken) =>
        Task.FromResult(_tasks.TryGetValue(taskId, out ActiveTask? task) ? task : null);

    public Task SaveTaskAsync(ActiveTask task, CancellationToken cancellationToken)
    {
        _tasks[task.Id] = task;
        return Task.CompletedTask;
    }

    public Task AppendApprovalRecordAsync(ApprovalRecord record, CancellationToken cancellationToken)
    {
        List<ApprovalRecord> records = _approvals.GetOrAdd(record.TaskId, _ => []);
        lock (records)
        {
            records.Add(record);
        }

        return Task.CompletedTask;
    }

    public Task<ApprovalRecord?> GetLatestApprovalAsync(TaskId taskId, CancellationToken cancellationToken)
    {
        if (!_approvals.TryGetValue(taskId, out List<ApprovalRecord>? records))
        {
            return Task.FromResult<ApprovalRecord?>(null);
        }

        lock (records)
        {
            return Task.FromResult(records.OrderByDescending(record => record.ApprovedAtUtc).FirstOrDefault());
        }
    }

    private void Seed()
    {
        var taskA = ActiveTask.Create(
            new TaskId(Guid.Parse("11111111-1111-1111-1111-111111111101")),
            "Hull 247 — Block 204 wiring",
            "ACT-204-WIR",
            new ApprovalValues(12.5m, 35m, 48m, new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 24)),
            isActiveForCurrentUser: true);

        var taskB = ActiveTask.Create(
            new TaskId(Guid.Parse("11111111-1111-1111-1111-111111111102")),
            "Engine room ventilation",
            "ACT-ENG-VNT",
            new ApprovalValues(20m, 10m, 8m, new DateOnly(2026, 6, 12), new DateOnly(2026, 7, 1)),
            isActiveForCurrentUser: false);

        var taskC = ActiveTask.Create(
            new TaskId(Guid.Parse("11111111-1111-1111-1111-111111111103")),
            "Deck coating inspection",
            "ACT-DCK-COT",
            new ApprovalValues(6m, 72m, 54m, new DateOnly(2026, 5, 28), new DateOnly(2026, 6, 18)),
            isActiveForCurrentUser: true);

        _tasks[taskA.Id] = taskA;
        _tasks[taskB.Id] = taskB;
        _tasks[taskC.Id] = taskC;

        ApprovalRecord seededApproval = ApprovalRecord.Create(
            taskC.Id,
            "supervisor.demo",
            DateTimeOffset.UtcNow.AddDays(-1),
            taskC.CurrentValues);

        _approvals[taskC.Id] = [seededApproval];
    }
}
