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
                    new ColumnDef("hoursToGo", "Hours to go", ColumnSource.Core, Visible: true, Order: 10),
                    new ColumnDef("progress", "Progress", ColumnSource.Core, Visible: true, Order: 20, Format: "percent"),
                    new ColumnDef("plannedStart", "Planned start", ColumnSource.Core, Visible: false, Order: 30, Format: "date"),
                    new ColumnDef("plannedFinish", "Planned finish", ColumnSource.Core, Visible: false, Order: 40, Format: "date"),
                ]),
            _ => ViewDefinition.Empty(screenId),
        };

    public IReadOnlyDictionary<string, object?> GetRowExtensions(Guid taskId) =>
        new Dictionary<string, object?>();
}
