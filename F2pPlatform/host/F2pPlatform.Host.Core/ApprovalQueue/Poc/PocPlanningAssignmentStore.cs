using F2pPlatform.Host.Contracts.ApprovalQueue;
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
        CreateRow(
            Guid.Parse("11111111-1111-1111-1111-111111111101"),
            21,
            "21: Metal Shop",
            "NSMV Demo",
            "Hull 247 — Block 204 wiring",
            "ACT-204-WIR"),
        CreateRow(
            Guid.Parse("11111111-1111-1111-1111-111111111102"),
            21,
            "21: Metal Shop",
            "Project",
            "Engine room ventilation",
            "ACT-ENG-VNT"),
        CreateRow(
            Guid.Parse("11111111-1111-1111-1111-111111111103"),
            21,
            "21: Metal Shop",
            "Aveva",
            "Deck coating inspection",
            "ACT-DCK-COT"),
    ];

    public static IReadOnlyList<PlanningAssignmentRow> ListAll() => All;

    private static PlanningAssignmentRow CreateRow(
        Guid id,
        int organisationId,
        string organisationLabel,
        string projectLabel,
        string title,
        string activityCode)
    {
        var taskId = new TaskId(id);
        return new PlanningAssignmentRow(
            taskId,
            new AssignmentId(id),
            new OrganisationId(organisationId),
            new AssignmentLabels(title, activityCode, organisationLabel, projectLabel),
            IsActiveAssignment: true);
    }
}
