using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.Rules;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Domain.Services;

public static class PlanningApprovalCoordinator
{
    public static AssignmentApprovalRecord RecordForemanApproval(
        ActiveAssignment assignment,
        PersonId foremanPersonId,
        DateTimeOffset approvedAtUtc,
        AssignmentApprovalRecord? existingForDay)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        DateOnly approvalDay = PlanningApprovalRules.ResolveApprovalDay(approvedAtUtc);

        if (existingForDay is null)
        {
            return AssignmentApprovalRecord.Create(
                assignment.Id,
                approvalDay,
                foremanPersonId,
                approvedAtUtc,
                assignment.CurrentValues);
        }

        existingForDay.Update(foremanPersonId, approvedAtUtc, assignment.CurrentValues);
        return existingForDay;
    }
}
