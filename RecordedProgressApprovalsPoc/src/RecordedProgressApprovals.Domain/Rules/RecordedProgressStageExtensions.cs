using RecordedProgressApprovals.Domain.Enums;

namespace RecordedProgressApprovals.Domain.Rules;

public static class RecordedProgressStageExtensions
{
    public static bool IsTerminal(this RecordedProgressStage stage) =>
        stage is RecordedProgressStage.Rejected
            or RecordedProgressStage.Exported
            or RecordedProgressStage.ExportFailed;

    public static bool RequiresHumanDecision(this RecordedProgressStage stage) =>
        stage is RecordedProgressStage.Submitted
            or RecordedProgressStage.ForemanChecked
            or RecordedProgressStage.PlanningReviewed
            or RecordedProgressStage.Approved;
}
