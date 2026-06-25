using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Planning;

namespace F2pPlatform.Host.Core.ApprovalQueue.Poc;

/// <summary>
/// POC stand-in for Planning module data. Production: Planning read actor + Planning DB only.
/// No duplicate table — this is in-memory until Planning module exists.
/// </summary>
internal static class PocPlanningAssignmentStore
{
    private static readonly IReadOnlyList<PlanningAssignmentRow> All =
    [
        new PlanningAssignmentRow(
            TaskId: Guid.Parse("11111111-1111-1111-1111-111111111101"),
            AssignmentId: Guid.Parse("11111111-1111-1111-1111-111111111101"),
            OrganisationId: 21,
            OrganisationLabel: "21: Metal Shop",
            ProjectLabel: "NSMV Demo",
            Title: "Hull 247 — Block 204 wiring",
            ActivityCode: "ACT-204-WIR",
            IsActiveAssignment: true),
        new PlanningAssignmentRow(
            TaskId: Guid.Parse("11111111-1111-1111-1111-111111111102"),
            AssignmentId: Guid.Parse("11111111-1111-1111-1111-111111111102"),
            OrganisationId: 21,
            OrganisationLabel: "21: Metal Shop",
            ProjectLabel: "Project",
            Title: "Engine room ventilation",
            ActivityCode: "ACT-ENG-VNT",
            IsActiveAssignment: true),
        new PlanningAssignmentRow(
            TaskId: Guid.Parse("11111111-1111-1111-1111-111111111103"),
            AssignmentId: Guid.Parse("11111111-1111-1111-1111-111111111103"),
            OrganisationId: 21,
            OrganisationLabel: "21: Metal Shop",
            ProjectLabel: "Aveva",
            Title: "Deck coating inspection",
            ActivityCode: "ACT-DCK-COT",
            IsActiveAssignment: true),
    ];

    public static IReadOnlyList<PlanningAssignmentRow> ListAll() => All;
}
