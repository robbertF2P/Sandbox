using F2pPlatform.Host.Contracts.ApprovalQueue;
using F2pPlatform.Host.Contracts.ApprovalQueue.Messages.Planning;
using Platform.Shared.Domain;

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
            "ACT-204-WIR",
            taskNumber: 312,
            locationPath: "21: Metal Shop / 521: Grinding",
            disciplineLabel: "521: Grinding",
            teamCount: 13,
            totalHoursBooked: 80m),
        CreateRow(
            Guid.Parse("11111111-1111-1111-1111-111111111102"),
            21,
            "21: Metal Shop",
            "Project",
            "Engine room ventilation",
            "ACT-ENG-VNT",
            taskNumber: 318,
            locationPath: "21: Metal Shop / 610: Ventilation",
            disciplineLabel: "610: Ventilation",
            teamCount: 12,
            totalHoursBooked: 64m),
        CreateRow(
            Guid.Parse("11111111-1111-1111-1111-111111111103"),
            21,
            "21: Metal Shop",
            "Aveva",
            "Deck coating inspection",
            "ACT-DCK-COT",
            taskNumber: 305,
            locationPath: "21: Metal Shop / 440: Coating",
            disciplineLabel: "440: Coating",
            teamCount: 21,
            totalHoursBooked: 54m),
    ];

    public static IReadOnlyList<PlanningAssignmentRow> ListAll() => All;

    private static PlanningAssignmentRow CreateRow(
        Guid id,
        int organisationId,
        string organisationLabel,
        string projectLabel,
        string title,
        string activityCode,
        int taskNumber,
        string locationPath,
        string disciplineLabel,
        int teamCount,
        decimal totalHoursBooked)
    {
        var taskId = new TaskId(id);
        return new PlanningAssignmentRow(
            taskId,
            new AssignmentId(id),
            new OrganisationId(organisationId),
            new AssignmentLabels(
                new TaskTitle(title),
                new ActivityCode(activityCode),
                new OrganisationLabel(organisationLabel),
                new ProjectLabel(projectLabel),
                new TaskNumber(taskNumber),
                locationPath,
                disciplineLabel,
                teamCount,
                totalHoursBooked),
            IsActiveAssignment: true);
    }
}
