using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Services;
using PlanningApprovals.Tests.Support;

namespace PlanningApprovals.Tests.Domain;

public sealed class PlanningApprovalCoordinatorShould
{
    [Fact]
    public void RecordForemanApproval_CreatesRecord_ForSelectedAssignment()
    {
        ActiveAssignment assignment = PlanningApprovalScenario.WeldingAssignment();
        DateTimeOffset approvedAtUtc = PlanningApprovalScenario.Today;

        AssignmentApprovalRecord record = PlanningApprovalCoordinator.RecordForemanApproval(
            assignment,
            PlanningApprovalScenario.ForemanPersonId,
            approvedAtUtc,
            existingForDay: null);

        Assert.Equal(assignment.Id, record.AssignmentId);
        Assert.Equal(PlanningApprovalScenario.ForemanPersonId, record.ApprovedBy);
        Assert.True(record.ApprovedValues.Matches(assignment.CurrentValues));
    }

    [Fact]
    public void RecordForemanApproval_UpdatesSameDayRecord_WhenApprovedAgain()
    {
        ActiveAssignment assignment = PlanningApprovalScenario.WeldingAssignment();
        DateTimeOffset firstApproval = PlanningApprovalScenario.Today;
        DateTimeOffset secondApproval = firstApproval.AddHours(2);

        AssignmentApprovalRecord first = PlanningApprovalCoordinator.RecordForemanApproval(
            assignment,
            PlanningApprovalScenario.ForemanPersonId,
            firstApproval,
            existingForDay: null);

        assignment.UpdateValues(assignment.CurrentValues with { HoursToGo = 10m });

        AssignmentApprovalRecord second = PlanningApprovalCoordinator.RecordForemanApproval(
            assignment,
            PlanningApprovalScenario.ForemanPersonId,
            secondApproval,
            existingForDay: first);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(10m, second.ApprovedValues.HoursToGo);
        Assert.True(second.ApprovedAtUtc >= first.ApprovedAtUtc);
    }
}
