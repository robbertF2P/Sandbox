using PlanningApprovals.Domain.Models;
using PlanningApprovals.Domain.ValueObjects;

namespace PlanningApprovals.Tests.Support;

public static class PlanningApprovalScenario
{
    public static readonly PersonId ForemanPersonId = new(9001);
    public static readonly AssignmentId AssignmentWelding = new(101);
    public static readonly AssignmentId AssignmentFitting = new(102);
    public static readonly DateTimeOffset Today = new(2026, 6, 24, 8, 0, 0, TimeSpan.Zero);

    public static ApprovalValues Values(
        decimal hoursToGo,
        DateOnly? plannedStart,
        DateOnly? plannedFinish,
        string assignedUser) =>
        new(hoursToGo, plannedStart, plannedFinish, AssignedUser.From(assignedUser));

    public static ActiveAssignment WeldingAssignment() =>
        ActiveAssignment.Create(
            AssignmentWelding,
            "Block 204 — hull wiring",
            "ACT-204-WIR",
            Values(12.5m, new DateOnly(2026, 6, 10), new DateOnly(2026, 6, 24), "j.doe"));

    public static ActiveAssignment FittingAssignment() =>
        ActiveAssignment.Create(
            AssignmentFitting,
            "Engine room ventilation",
            "ACT-ENG-VNT",
            Values(20m, new DateOnly(2026, 6, 12), new DateOnly(2026, 7, 1), "m.smith"));
}
