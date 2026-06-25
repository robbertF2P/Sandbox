using HourApprovals.Domain.Enums;
using HourApprovals.Domain.Models;
using HourApprovals.Domain.ValueObjects;

namespace HourApprovals.Application.Ports;

public interface IHourApprovalsRepository
{
    Task<IReadOnlyList<ActiveTask>> ListTasksAsync(CancellationToken cancellationToken);

    Task<ActiveTask?> GetTaskAsync(Guid taskId, CancellationToken cancellationToken);

    Task SaveTaskAsync(ActiveTask task, CancellationToken cancellationToken);

    Task AppendApprovalRecordAsync(ApprovalRecord record, CancellationToken cancellationToken);

    Task<ApprovalRecord?> GetLatestApprovalAsync(Guid taskId, CancellationToken cancellationToken);
}

public sealed record TaskApprovalView(
    ActiveTask Task,
    ApprovalRecord? LastApproval,
    TaskApprovalState State);

public interface IHourApprovalsService
{
    Task<HourApprovalsCapabilities> GetCapabilitiesAsync(
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskApprovalView>> ListTasksAsync(
        ApprovalFilterStatus filter,
        CancellationToken cancellationToken);

    Task<TaskApprovalView?> GetTaskAsync(Guid taskId, CancellationToken cancellationToken);

    Task<TaskApprovalView> SaveTaskAsync(
        Guid taskId,
        ApprovalValues values,
        string actingUser,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken);

    Task<TaskApprovalView> ApproveTaskAsync(
        Guid taskId,
        string actingUser,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken);

    Task<SubmitTasksResult> SubmitTasksAsync(
        IReadOnlyList<Guid> taskIds,
        string actingUser,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken);
}

public sealed record SubmitTaskFailure(Guid TaskId, string Error);

public sealed record SubmitTasksResult(
    IReadOnlyList<TaskApprovalView> Approved,
    IReadOnlyList<SubmitTaskFailure> Failures);

public sealed record HourApprovalsCapabilities(
    bool FeatureEnabled,
    string CustomizationPackId,
    HourApprovalsDisplaySettings DisplaySettings,
    bool CanApprove,
    IReadOnlyList<string> Permissions);
