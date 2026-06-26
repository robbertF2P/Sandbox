using PlanningApprovals.Domain.Models;

namespace PlanningApprovals.Domain.Services;

public sealed record ForemanDecisionResult(
    AssignmentApprovalRequest Request,
    ApprovalDecision Decision,
    ApprovedPlanSnapshot? Snapshot);
