namespace RecordedProgressApprovals.Domain.Enums;

public enum RecordedProgressStage
{
    Recorded = 0,
    Submitted = 1,
    ForemanChecked = 2,
    PlanningReviewed = 3,
    Approved = 4,
    Exported = 5,
    Rejected = 6,
    ExportFailed = 7,
}
