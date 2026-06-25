using Akka.Actor;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Hours;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Planning;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Timekeeping;
using Platform.Shared.Domain;
using Platform.Shared.Domain;

namespace F2pPlatform.Host.Core.ApprovalQueue;

public interface IApprovalQueueFacade
{
    Task<GetApprovalQueueReply> QueryAsync(GetApprovalQueue query, CancellationToken cancellationToken);
}

/// <summary>
/// HTTP/facade boundary: the only place that uses Ask. Composes module read actors — no database here.
/// </summary>
public sealed class ApprovalQueueFacade : IApprovalQueueFacade
{
    private static readonly TimeSpan AskTimeout = TimeSpan.FromSeconds(10);

    private readonly IActorRef _planningReadActor;
    private readonly IActorRef _timekeepingReadActor;
    private readonly IActorRef _hoursReadActor;

    public ApprovalQueueFacade(
        IActorRef planningReadActor,
        IActorRef timekeepingReadActor,
        IActorRef hoursReadActor)
    {
        _planningReadActor = planningReadActor;
        _timekeepingReadActor = timekeepingReadActor;
        _hoursReadActor = hoursReadActor;
    }

    public async Task<GetApprovalQueueReply> QueryAsync(
        GetApprovalQueue query,
        CancellationToken cancellationToken)
    {
        var assignments = await _planningReadActor.Ask<GetPlanningAssignmentsReply>(
            new GetPlanningAssignments(query.Filter),
            AskTimeout,
            cancellationToken);

        IReadOnlyList<AssignmentId> assignmentIds = assignments.Rows
            .Select(row => row.AssignmentId)
            .ToList();
        IReadOnlyList<TaskId> taskIds = assignments.Rows
            .Select(row => row.TaskId)
            .ToList();

        Task<GetHoursInWindowReply> hoursTask = _timekeepingReadActor.Ask<GetHoursInWindowReply>(
            new GetHoursInWindow(assignmentIds, TimeRangePreset.SinceLastSubmission),
            AskTimeout,
            cancellationToken);

        Task<GetHourSubmissionSnapshotsReply> submissionsTask = _hoursReadActor.Ask<GetHourSubmissionSnapshotsReply>(
            new GetHourSubmissionSnapshots(taskIds),
            AskTimeout,
            cancellationToken);

        await Task.WhenAll(hoursTask, submissionsTask);

        var rows = ApprovalQueueComposer.Compose(
            assignments.Rows,
            hoursTask.Result.HoursByAssignmentId,
            submissionsTask.Result.SnapshotsByTaskId,
            query.Filter);

        return new GetApprovalQueueReply(rows);
    }
}
