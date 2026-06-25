using Akka.Actor;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Hours;
using HourApprovals.Application.Ports;
using HourApprovals.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace F2pPlatform.Host.Core.ApprovalQueue.Actors;

/// <summary>
/// Hours module read boundary. Reads submission snapshots from HourApprovals (real module DB / in-memory repo).
/// </summary>
public sealed class HoursApprovalReadActor : ReceiveActor
{
    private readonly IServiceScopeFactory _scopeFactory;

    public HoursApprovalReadActor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;

        ReceiveAsync<GetHourSubmissionSnapshots>(HandleAsync);
    }

    private async Task HandleAsync(GetHourSubmissionSnapshots message)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IHourApprovalsService hourApprovals = scope.ServiceProvider.GetRequiredService<IHourApprovalsService>();

        IReadOnlyList<TaskApprovalView> views = await hourApprovals.ListTasksAsync(
            ApprovalFilterStatus.All,
            CancellationToken.None);

        Dictionary<Guid, HourSubmissionSnapshot> snapshots = new();
        foreach (TaskApprovalView view in views)
        {
            if (!message.TaskIds.Contains(view.Task.Id))
            {
                continue;
            }

            snapshots[view.Task.Id] = new HourSubmissionSnapshot(
                view.Task.Id,
                view.State.ToString(),
                view.State == TaskApprovalState.Approved,
                view.Task.CurrentValues.HoursToGo,
                view.Task.CurrentValues.Progress,
                view.Task.CurrentValues.WorkedHours,
                view.Task.CurrentValues.PlannedStart?.ToString("yyyy-MM-dd"),
                view.Task.CurrentValues.PlannedFinish?.ToString("yyyy-MM-dd"),
                view.LastApproval?.ApprovedBy,
                view.LastApproval?.ApprovedAtUtc);
        }

        Sender.Tell(new GetHourSubmissionSnapshotsReply(snapshots));
    }

    public static Props Props(IServiceScopeFactory scopeFactory) =>
        Akka.Actor.Props.Create(() => new HoursApprovalReadActor(scopeFactory));
}
