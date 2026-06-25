using Akka.Actor;
using F2pPlatform.Host.Contracts.ApprovalQueue;
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

        HashSet<TaskId> requestedTaskIds = message.TaskIds.ToHashSet();
        Dictionary<TaskId, HourSubmissionSnapshot> snapshots = new();

        foreach (TaskApprovalView view in views)
        {
            var taskId = new TaskId(view.Task.Id.Value);
            if (!requestedTaskIds.Contains(taskId))
            {
                continue;
            }

            snapshots[taskId] = HourSubmissionSnapshotMapper.ToSnapshot(view);
        }

        Sender.Tell(new GetHourSubmissionSnapshotsReply(snapshots));
    }

    public static Props Props(IServiceScopeFactory scopeFactory) =>
        Akka.Actor.Props.Create(() => new HoursApprovalReadActor(scopeFactory));
}
