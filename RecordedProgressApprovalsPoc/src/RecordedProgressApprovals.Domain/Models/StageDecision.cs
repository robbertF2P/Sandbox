using RecordedProgressApprovals.Domain.Enums;
using RecordedProgressApprovals.Domain.ValueObjects;

namespace RecordedProgressApprovals.Domain.Models;

public sealed class StageDecision
{
    private StageDecision(
        RecordedProgressStage stage,
        StageOutcome outcome,
        PersonId decidedBy,
        DateTimeOffset decidedAtUtc,
        string? comment)
    {
        Stage = stage;
        Outcome = outcome;
        DecidedBy = decidedBy;
        DecidedAtUtc = decidedAtUtc;
        Comment = comment;
    }

    public RecordedProgressStage Stage { get; }

    public StageOutcome Outcome { get; }

    public PersonId DecidedBy { get; }

    public DateTimeOffset DecidedAtUtc { get; }

    public string? Comment { get; }

    public static StageDecision Approve(
        RecordedProgressStage stage,
        PersonId decidedBy,
        DateTimeOffset decidedAtUtc,
        string? comment = null) =>
        new(stage, StageOutcome.Approved, decidedBy, decidedAtUtc, comment);

    public static StageDecision Reject(
        RecordedProgressStage stage,
        PersonId decidedBy,
        DateTimeOffset decidedAtUtc,
        string? comment = null) =>
        new(stage, StageOutcome.Rejected, decidedBy, decidedAtUtc, comment);
}
