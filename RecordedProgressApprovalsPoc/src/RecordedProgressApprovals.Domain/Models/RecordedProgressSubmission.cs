using RecordedProgressApprovals.Domain.Enums;
using RecordedProgressApprovals.Domain.ValueObjects;

namespace RecordedProgressApprovals.Domain.Models;

public sealed class RecordedProgressSubmission
{
    private readonly List<StageDecision> _decisions = [];

    private RecordedProgressSubmission(
        SubmissionId id,
        AssignmentId assignmentId,
        ProgressRevisionId progressRevisionId,
        RecordedProgressValues recordedValues,
        RecordedProgressStage currentStage,
        SubmissionStatus status,
        DateTimeOffset openedAtUtc,
        DateTimeOffset? closedAtUtc)
    {
        Id = id;
        AssignmentId = assignmentId;
        ProgressRevisionId = progressRevisionId;
        RecordedValues = recordedValues;
        CurrentStage = currentStage;
        Status = status;
        OpenedAtUtc = openedAtUtc;
        ClosedAtUtc = closedAtUtc;
    }

    public SubmissionId Id { get; }

    public AssignmentId AssignmentId { get; }

    public ProgressRevisionId ProgressRevisionId { get; }

    public RecordedProgressValues RecordedValues { get; }

    public RecordedProgressStage CurrentStage { get; private set; }

    public SubmissionStatus Status { get; private set; }

    public DateTimeOffset OpenedAtUtc { get; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public IReadOnlyList<StageDecision> Decisions => _decisions;

    public static RecordedProgressSubmission Open(
        AssignmentId assignmentId,
        ProgressRevisionId progressRevisionId,
        RecordedProgressValues recordedValues,
        DateTimeOffset openedAtUtc) =>
        new(
            SubmissionId.New(),
            assignmentId,
            progressRevisionId,
            recordedValues,
            RecordedProgressStage.Recorded,
            SubmissionStatus.Open,
            openedAtUtc,
            closedAtUtc: null);

    public static RecordedProgressSubmission Rehydrate(
        SubmissionId id,
        AssignmentId assignmentId,
        ProgressRevisionId progressRevisionId,
        RecordedProgressValues recordedValues,
        RecordedProgressStage currentStage,
        SubmissionStatus status,
        DateTimeOffset openedAtUtc,
        DateTimeOffset? closedAtUtc,
        IReadOnlyList<StageDecision> decisions)
    {
        var submission = new RecordedProgressSubmission(
            id,
            assignmentId,
            progressRevisionId,
            recordedValues,
            currentStage,
            status,
            openedAtUtc,
            closedAtUtc);

        submission._decisions.AddRange(decisions);
        return submission;
    }

    internal void ApplyDecision(StageDecision decision, RecordedProgressStage nextStage)
    {
        _decisions.Add(decision);
        CurrentStage = nextStage;

        if (nextStage is RecordedProgressStage.Rejected
            or RecordedProgressStage.Exported
            or RecordedProgressStage.ExportFailed)
        {
            Close(decision.DecidedAtUtc);
        }
    }

    internal void Supersede(DateTimeOffset supersededAtUtc)
    {
        Status = SubmissionStatus.Superseded;
        ClosedAtUtc = supersededAtUtc;
    }

    private void Close(DateTimeOffset closedAtUtc)
    {
        Status = SubmissionStatus.Closed;
        ClosedAtUtc = closedAtUtc;
    }
}
