using HourApprovals.Domain.Enums;
using HourApprovals.Domain.Models;
using HourApprovals.Domain.ValueObjects;
using Platform.Shared.Domain;
using Platform.Shared.View;

namespace HourApprovals.Application.Ports;

public interface IHourApprovalsRepository
{
    Task<IReadOnlyList<ActiveTask>> ListTasksAsync(CancellationToken cancellationToken);

    Task<ActiveTask?> GetTaskAsync(TaskId taskId, CancellationToken cancellationToken);

    Task SaveTaskAsync(ActiveTask task, CancellationToken cancellationToken);

    Task AppendApprovalRecordAsync(ApprovalRecord record, CancellationToken cancellationToken);

    Task<ApprovalRecord?> GetLatestApprovalAsync(TaskId taskId, CancellationToken cancellationToken);
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

    Task<TaskApprovalView?> GetTaskAsync(TaskId taskId, CancellationToken cancellationToken);

    Task<TaskApprovalView> SaveTaskAsync(
        TaskId taskId,
        ApprovalValues values,
        UserName actingUser,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken);

    Task<TaskApprovalView> ApproveTaskAsync(
        TaskId taskId,
        UserName actingUser,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken);

    Task<SubmitTasksResult> SubmitTasksAsync(
        IReadOnlyList<TaskId> taskIds,
        UserName actingUser,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken);
}

public sealed record SubmitTaskFailure(TaskId TaskId, string Error);

public sealed record SubmitTasksResult(
    IReadOnlyList<TaskApprovalView> Approved,
    IReadOnlyList<SubmitTaskFailure> Failures);

public sealed record HourApprovalsCapabilities(
    bool FeatureEnabled,
    string CustomizationPackId,
    ViewDefinition QueueView,
    bool CanApprove,
    IReadOnlyList<string> Permissions);
