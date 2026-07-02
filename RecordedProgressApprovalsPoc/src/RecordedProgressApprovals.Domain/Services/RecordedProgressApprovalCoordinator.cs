using RecordedProgressApprovals.Domain.Enums;
using RecordedProgressApprovals.Domain.Models;
using RecordedProgressApprovals.Domain.Rules;
using RecordedProgressApprovals.Domain.ValueObjects;

namespace RecordedProgressApprovals.Domain.Services;

public static class RecordedProgressApprovalCoordinator
{
    public static RecordedProgressSubmission OpenSubmission(
        AssignmentId assignmentId,
        ProgressRevisionId progressRevisionId,
        RecordedProgressValues recordedValues,
        DateTimeOffset openedAtUtc) =>
        RecordedProgressSubmission.Open(assignmentId, progressRevisionId, recordedValues, openedAtUtc);

    public static RecordedProgressSubmission AdvanceStage(
        RecordedProgressSubmission submission,
        RecordedProgressStage targetStage,
        PersonId decidedBy,
        DateTimeOffset decidedAtUtc,
        IReadOnlyList<RecordedProgressStage>? pipeline = null,
        string? comment = null)
    {
        ArgumentNullException.ThrowIfNull(submission);

        IReadOnlyList<RecordedProgressStage> effectivePipeline = pipeline ?? RecordedProgressApprovalRules.DefaultPipeline;

        if (!RecordedProgressApprovalRules.CanAdvance(submission, targetStage, effectivePipeline))
        {
            throw new InvalidOperationException(
                $"Cannot advance submission '{submission.Id}' from '{submission.CurrentStage}' to '{targetStage}'.");
        }

        StageDecision decision = StageDecision.Approve(
            targetStage,
            decidedBy,
            decidedAtUtc,
            comment);

        submission.ApplyDecision(decision, targetStage);
        return submission;
    }

    public static RecordedProgressSubmission RejectAtStage(
        RecordedProgressSubmission submission,
        RecordedProgressStage atStage,
        PersonId decidedBy,
        DateTimeOffset decidedAtUtc,
        IReadOnlyList<RecordedProgressStage>? pipeline = null,
        string? comment = null)
    {
        ArgumentNullException.ThrowIfNull(submission);

        IReadOnlyList<RecordedProgressStage> effectivePipeline = pipeline ?? RecordedProgressApprovalRules.DefaultPipeline;

        if (submission.Status is not SubmissionStatus.Open)
        {
            throw new InvalidOperationException($"Submission '{submission.Id}' is not open.");
        }

        if (!atStage.RequiresHumanDecision())
        {
            throw new InvalidOperationException($"Stage '{atStage}' does not accept human rejection.");
        }

        RecordedProgressStage? expectedStage = RecordedProgressApprovalRules.GetNextStage(
            submission.CurrentStage,
            effectivePipeline);

        if (expectedStage != atStage)
        {
            throw new InvalidOperationException(
                $"Cannot reject at '{atStage}' while submission '{submission.Id}' is at '{submission.CurrentStage}'.");
        }

        StageDecision decision = StageDecision.Reject(atStage, decidedBy, decidedAtUtc, comment);
        submission.ApplyDecision(decision, RecordedProgressStage.Rejected);
        return submission;
    }

    public static RecordedProgressSubmission MarkExported(
        RecordedProgressSubmission submission,
        PersonId actingAsSystem,
        DateTimeOffset exportedAtUtc,
        IReadOnlyList<RecordedProgressStage>? pipeline = null)
    {
        ArgumentNullException.ThrowIfNull(submission);

        IReadOnlyList<RecordedProgressStage> effectivePipeline = pipeline ?? RecordedProgressApprovalRules.DefaultPipeline;

        if (!RecordedProgressApprovalRules.IsEligibleForErpExport(submission, effectivePipeline))
        {
            throw new InvalidOperationException(
                $"Submission '{submission.Id}' is not eligible for ERP export.");
        }

        return AdvanceStage(
            submission,
            RecordedProgressStage.Exported,
            actingAsSystem,
            exportedAtUtc,
            effectivePipeline);
    }
}
