using RecordedProgressApprovals.Domain.Enums;
using RecordedProgressApprovals.Domain.Models;
using RecordedProgressApprovals.Domain.Rules;
using RecordedProgressApprovals.Domain.Services;
using RecordedProgressApprovals.Domain.ValueObjects;
using RecordedProgressApprovals.Tests.Support;

namespace RecordedProgressApprovals.Tests.Domain;

public sealed class RecordedProgressApprovalCoordinatorShould
{
    [Fact]
    public void FullPipeline_ApprovesAllStages_ThenAllowsErpExport()
    {
        RecordedProgressSubmission submission = RecordedProgressApprovalCoordinator.OpenSubmission(
            RecordedProgressScenario.WeldingAssignmentId,
            RecordedProgressScenario.FirstRevisionId,
            RecordedProgressScenario.WeldingProgress(),
            RecordedProgressScenario.Today);

        Advance(submission, RecordedProgressStage.Submitted, RecordedProgressScenario.WorkerPersonId, hours: 0);
        Advance(submission, RecordedProgressStage.ForemanChecked, RecordedProgressScenario.ForemanPersonId, hours: 1);
        Advance(submission, RecordedProgressStage.PlanningReviewed, RecordedProgressScenario.PlannerPersonId, hours: 2);
        Advance(submission, RecordedProgressStage.Approved, RecordedProgressScenario.ControllerPersonId, hours: 3);

        Assert.Equal(RecordedProgressStage.Approved, submission.CurrentStage);
        Assert.True(RecordedProgressApprovalRules.IsEligibleForErpExport(
            submission,
            RecordedProgressApprovalRules.DefaultPipeline));

        RecordedProgressSubmission exported = RecordedProgressApprovalCoordinator.MarkExported(
            submission,
            RecordedProgressScenario.SystemPersonId,
            RecordedProgressScenario.Today.AddHours(4));

        Assert.Equal(RecordedProgressStage.Exported, exported.CurrentStage);
        Assert.Equal(SubmissionStatus.Closed, exported.Status);
    }

    [Fact]
    public void RejectAtStage_BlocksErpExport()
    {
        RecordedProgressSubmission submission = RecordedProgressApprovalCoordinator.OpenSubmission(
            RecordedProgressScenario.WeldingAssignmentId,
            RecordedProgressScenario.FirstRevisionId,
            RecordedProgressScenario.WeldingProgress(),
            RecordedProgressScenario.Today);

        Advance(submission, RecordedProgressStage.Submitted, RecordedProgressScenario.WorkerPersonId, hours: 0);

        RecordedProgressApprovalCoordinator.RejectAtStage(
            submission,
            RecordedProgressStage.ForemanChecked,
            RecordedProgressScenario.ForemanPersonId,
            RecordedProgressScenario.Today.AddHours(1),
            comment: "Hours do not match punch data");

        Assert.Equal(RecordedProgressStage.Rejected, submission.CurrentStage);
        Assert.Equal(SubmissionStatus.Closed, submission.Status);
        Assert.False(RecordedProgressApprovalRules.IsEligibleForErpExport(
            submission,
            RecordedProgressApprovalRules.DefaultPipeline));
    }

    [Fact]
    public void AdvanceStage_Throws_WhenSkippingStages()
    {
        RecordedProgressSubmission submission = RecordedProgressApprovalCoordinator.OpenSubmission(
            RecordedProgressScenario.WeldingAssignmentId,
            RecordedProgressScenario.FirstRevisionId,
            RecordedProgressScenario.WeldingProgress(),
            RecordedProgressScenario.Today);

        Assert.Throws<InvalidOperationException>(() =>
            RecordedProgressApprovalCoordinator.AdvanceStage(
                submission,
                RecordedProgressStage.ForemanChecked,
                RecordedProgressScenario.ForemanPersonId,
                RecordedProgressScenario.Today.AddHours(1)));
    }

    [Fact]
    public void MarkExported_Throws_WhenNotFullyApproved()
    {
        RecordedProgressSubmission submission = RecordedProgressApprovalCoordinator.OpenSubmission(
            RecordedProgressScenario.WeldingAssignmentId,
            RecordedProgressScenario.FirstRevisionId,
            RecordedProgressScenario.WeldingProgress(),
            RecordedProgressScenario.Today);

        Advance(submission, RecordedProgressStage.Submitted, RecordedProgressScenario.WorkerPersonId, hours: 0);

        Assert.Throws<InvalidOperationException>(() =>
            RecordedProgressApprovalCoordinator.MarkExported(
                submission,
                RecordedProgressScenario.SystemPersonId,
                RecordedProgressScenario.Today.AddHours(2)));
    }

    private static void Advance(
        RecordedProgressSubmission submission,
        RecordedProgressStage targetStage,
        PersonId actor,
        int hours)
    {
        RecordedProgressApprovalCoordinator.AdvanceStage(
            submission,
            targetStage,
            actor,
            RecordedProgressScenario.Today.AddHours(hours));
    }
}
