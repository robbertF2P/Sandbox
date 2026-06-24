using HourApprovals.Application.Ports;
using HourApprovals.Domain.Enums;
using HourApprovals.Domain.Models;
using HourApprovals.Domain.Rules;
using HourApprovals.Domain.ValueObjects;

namespace HourApprovals.Application;

public sealed class HourApprovalsService : IHourApprovalsService
{
    private readonly IHourApprovalsRepository _repository;
    private readonly IHourApprovalsFeatureGate _featureGate;
    private readonly IHourApprovalsCustomizationPack _customizationPack;

    public HourApprovalsService(
        IHourApprovalsRepository repository,
        IHourApprovalsFeatureGate featureGate,
        IHourApprovalsCustomizationPack customizationPack)
    {
        _repository = repository;
        _featureGate = featureGate;
        _customizationPack = customizationPack;
    }

    public Task<HourApprovalsCapabilities> GetCapabilitiesAsync(
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken)
    {
        EnsureFeatureEnabled();

        var capabilities = new HourApprovalsCapabilities(
            _featureGate.IsEnabled,
            _customizationPack.PackId,
            _customizationPack.DisplaySettings,
            HourApprovalRules.CanApprove(permissions),
            permissions.ToList());

        return Task.FromResult(capabilities);
    }

    public async Task<IReadOnlyList<TaskApprovalView>> ListTasksAsync(
        ApprovalFilterStatus filter,
        CancellationToken cancellationToken)
    {
        EnsureFeatureEnabled();

        IReadOnlyList<ActiveTask> tasks = await _repository.ListTasksAsync(cancellationToken);
        List<TaskApprovalView> views = [];

        foreach (ActiveTask task in tasks)
        {
            TaskApprovalView view = await BuildViewAsync(task, cancellationToken);
            if (HourApprovalRules.MatchesFilter(view.State, filter))
            {
                views.Add(view);
            }
        }

        return views;
    }

    public async Task<TaskApprovalView?> GetTaskAsync(Guid taskId, CancellationToken cancellationToken)
    {
        EnsureFeatureEnabled();

        ActiveTask? task = await _repository.GetTaskAsync(taskId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        return await BuildViewAsync(task, cancellationToken);
    }

    public async Task<TaskApprovalView> SaveTaskAsync(
        Guid taskId,
        ApprovalValues values,
        string actingUser,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken)
    {
        EnsureFeatureEnabled();

        if (!HourApprovalRules.CanApprove(permissions))
        {
            throw new UnauthorizedAccessException(
                $"User '{actingUser}' lacks permission {HourApprovalRules.ApproveHoursProgressPermission}.");
        }

        ActiveTask task = await RequireTaskAsync(taskId, cancellationToken);
        ApprovalRecord? previousApproval = await _repository.GetLatestApprovalAsync(taskId, cancellationToken);
        bool wasApproved = HourApprovalRules.ResolveState(task.CurrentValues, previousApproval)
            == TaskApprovalState.Approved;

        task.UpdateValues(values, isActiveForCurrentUser: false);
        await _repository.SaveTaskAsync(task, cancellationToken);

        ApprovalRecord record = ApprovalRecord.Create(
            taskId,
            actingUser,
            DateTimeOffset.UtcNow,
            values);

        await _repository.AppendApprovalRecordAsync(record, cancellationToken);

        if (wasApproved)
        {
            task.UpdateValues(values, isActiveForCurrentUser: false);
        }

        return await BuildViewAsync(task, cancellationToken);
    }

    public async Task<TaskApprovalView> ApproveTaskAsync(
        Guid taskId,
        string actingUser,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken)
    {
        EnsureFeatureEnabled();

        if (!HourApprovalRules.CanApprove(permissions))
        {
            throw new UnauthorizedAccessException(
                $"User '{actingUser}' lacks permission {HourApprovalRules.ApproveHoursProgressPermission}.");
        }

        ActiveTask task = await RequireTaskAsync(taskId, cancellationToken);

        ApprovalRecord record = ApprovalRecord.Create(
            taskId,
            actingUser,
            DateTimeOffset.UtcNow,
            task.CurrentValues);

        await _repository.AppendApprovalRecordAsync(record, cancellationToken);
        return await BuildViewAsync(task, cancellationToken);
    }

    private async Task<ActiveTask> RequireTaskAsync(Guid taskId, CancellationToken cancellationToken)
    {
        ActiveTask? task = await _repository.GetTaskAsync(taskId, cancellationToken);
        return task ?? throw new KeyNotFoundException($"Task '{taskId}' was not found.");
    }

    private async Task<TaskApprovalView> BuildViewAsync(ActiveTask task, CancellationToken cancellationToken)
    {
        ApprovalRecord? lastApproval = await _repository.GetLatestApprovalAsync(task.Id, cancellationToken);
        TaskApprovalState state = HourApprovalRules.ResolveState(task.CurrentValues, lastApproval);
        return new TaskApprovalView(task, lastApproval, state);
    }

    private void EnsureFeatureEnabled()
    {
        if (!_featureGate.IsEnabled)
        {
            throw new InvalidOperationException("Hour approvals feature is disabled for this tenant.");
        }
    }
}
