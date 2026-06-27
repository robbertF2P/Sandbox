using HourApprovals.Application.Ports;
using Platform.Shared.View;

namespace HourApprovals.Infrastructure;

public sealed class DefaultHourApprovalsPack : IHourApprovalsCustomizationPack
{
    public string PackId => "default-hour-approvals-v1";

    public ViewDefinition GetView(string screenId) =>
        screenId switch
        {
            HourApprovalsScreens.Queue => new ViewDefinition(
                HourApprovalsScreens.Queue,
                [
                    new ColumnDef("hoursToGo", "hourApprovals.columns.hoursToGo", ColumnSource.Core, Visible: true, Order: 10, Format: "decimal"),
                    new ColumnDef("progress", "hourApprovals.columns.progress", ColumnSource.Core, Visible: true, Order: 20, Format: "percent"),
                    new ColumnDef("plannedStart", "hourApprovals.columns.plannedStart", ColumnSource.Core, Visible: false, Order: 30, Format: "date"),
                    new ColumnDef("plannedFinish", "hourApprovals.columns.plannedFinish", ColumnSource.Core, Visible: false, Order: 40, Format: "date"),
                    new ColumnDef("daysSinceLastSubmission", "hourApprovals.columns.daysSinceLastSubmission", ColumnSource.Computed, Visible: false, Order: 60, Format: "integer"),
                ]),
            _ => ViewDefinition.Empty(screenId),
        };

    public IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> GetRowExtensions(
        IReadOnlyList<Guid> taskIds) =>
        new Dictionary<Guid, IReadOnlyDictionary<string, object?>>();
}
