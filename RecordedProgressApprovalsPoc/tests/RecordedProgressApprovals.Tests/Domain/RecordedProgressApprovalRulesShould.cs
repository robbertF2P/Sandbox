using RecordedProgressApprovals.Domain.Enums;
using RecordedProgressApprovals.Domain.Models;
using RecordedProgressApprovals.Domain.Rules;
using RecordedProgressApprovals.Tests.Support;

namespace RecordedProgressApprovals.Tests.Domain;

public sealed class RecordedProgressApprovalRulesShould
{
    [Fact]
    public void IsEligibleForErpExport_IsFalse_UntilAllStagesApproved()
    {
        RecordedProgressSubmission submission = RecordedProgressSubmission.Open(
            RecordedProgressScenario.WeldingAssignmentId,
            RecordedProgressScenario.FirstRevisionId,
            RecordedProgressScenario.WeldingProgress(),
            RecordedProgressScenario.Today);

        Assert.False(RecordedProgressApprovalRules.IsEligibleForErpExport(
            submission,
            RecordedProgressApprovalRules.DefaultPipeline));
    }

    [Fact]
    public void GetNextStage_FollowsDefaultPipeline()
    {
        RecordedProgressStage? next = RecordedProgressApprovalRules.GetNextStage(
            RecordedProgressStage.Submitted,
            RecordedProgressApprovalRules.DefaultPipeline);

        Assert.Equal(RecordedProgressStage.ForemanChecked, next);
    }

    [Fact]
    public void RequiredApprovalStages_ExcludesRecordedAndExported()
    {
        IReadOnlyList<RecordedProgressStage> required =
            RecordedProgressApprovalRules.RequiredApprovalStages(RecordedProgressApprovalRules.DefaultPipeline);

        Assert.Contains(RecordedProgressStage.Submitted, required);
        Assert.Contains(RecordedProgressStage.Approved, required);
        Assert.DoesNotContain(RecordedProgressStage.Recorded, required);
        Assert.DoesNotContain(RecordedProgressStage.Exported, required);
    }
}
