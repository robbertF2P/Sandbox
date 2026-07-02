using RecordedProgressApprovals.Domain.Enums;
using RecordedProgressApprovals.Domain.Models;

namespace RecordedProgressApprovals.Domain.Rules;

public static class RecordedProgressApprovalRules
{
    public static IReadOnlyList<RecordedProgressStage> DefaultPipeline { get; } =
    [
        RecordedProgressStage.Recorded,
        RecordedProgressStage.Submitted,
        RecordedProgressStage.ForemanChecked,
        RecordedProgressStage.PlanningReviewed,
        RecordedProgressStage.Approved,
        RecordedProgressStage.Exported,
    ];

    public static IReadOnlyList<RecordedProgressStage> RequiredApprovalStages(
        IReadOnlyList<RecordedProgressStage> pipeline) =>
        pipeline
            .Where(stage => stage.RequiresHumanDecision())
            .ToList();

    public static RecordedProgressStage? GetNextStage(
        RecordedProgressStage currentStage,
        IReadOnlyList<RecordedProgressStage> pipeline)
    {
        for (int index = 0; index < pipeline.Count; index++)
        {
            if (pipeline[index] != currentStage)
            {
                continue;
            }

            if (index >= pipeline.Count - 1)
            {
                return null;
            }

            return pipeline[index + 1];
        }

        return null;
    }

    public static bool CanAdvance(
        RecordedProgressSubmission submission,
        RecordedProgressStage targetStage,
        IReadOnlyList<RecordedProgressStage> pipeline)
    {
        if (submission.Status is not SubmissionStatus.Open)
        {
            return false;
        }

        if (submission.CurrentStage.IsTerminal())
        {
            return false;
        }

        RecordedProgressStage? expectedNext = GetNextStage(submission.CurrentStage, pipeline);
        return expectedNext == targetStage;
    }

    public static bool HasApprovedDecision(
        RecordedProgressSubmission submission,
        RecordedProgressStage stage) =>
        submission.Decisions.Any(decision =>
            decision.Stage == stage && decision.Outcome == StageOutcome.Approved);

    public static bool IsEligibleForErpExport(
        RecordedProgressSubmission submission,
        IReadOnlyList<RecordedProgressStage> pipeline)
    {
        if (submission.Status is not SubmissionStatus.Open and not SubmissionStatus.Closed)
        {
            return false;
        }

        if (submission.CurrentStage is not RecordedProgressStage.Approved)
        {
            return false;
        }

        if (submission.Decisions.Any(decision => decision.Outcome == StageOutcome.Rejected))
        {
            return false;
        }

        foreach (RecordedProgressStage stage in RequiredApprovalStages(pipeline))
        {
            if (!HasApprovedDecision(submission, stage))
            {
                return false;
            }
        }

        return true;
    }
}
