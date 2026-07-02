using HourApprovals.Domain.Models;
using Platform.Shared.Domain;

namespace HourApprovals.Application.Persistence;

public interface IHourApprovalsRepository
{
    Task<IReadOnlyList<ActiveTask>> ListTasksAsync(CancellationToken cancellationToken);

    Task<ActiveTask?> GetTaskAsync(TaskId taskId, CancellationToken cancellationToken);

    Task SaveTaskAsync(ActiveTask task, CancellationToken cancellationToken);

    Task UpsertDailyApprovalAsync(ApprovalRecord record, CancellationToken cancellationToken);

    Task<ApprovalRecord?> GetLatestApprovalAsync(TaskId taskId, CancellationToken cancellationToken);

    Task<ApprovalRecord?> GetApprovalForDayAsync(TaskId taskId, DateOnly approvalDay, CancellationToken cancellationToken);
}
