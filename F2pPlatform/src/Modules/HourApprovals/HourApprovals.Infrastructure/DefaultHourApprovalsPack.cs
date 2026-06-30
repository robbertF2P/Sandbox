using HourApprovals.Application.Ports;
using Platform.Shared.View;

namespace HourApprovals.Infrastructure;

public sealed class DefaultHourApprovalsPack : IHourApprovalsCustomizationPack
{
    public const string DefaultPackId = "default-hour-approvals-v1";

    public string PackId => DefaultPackId;

    public ViewDefinition GetView(string screenId) =>
        screenId switch
        {
            HourApprovalsScreens.Queue => new ViewDefinition(
                HourApprovalsScreens.Queue,
                [
                    new ColumnDef("assignedUser", "hourApprovals.columns.assignedUser", ColumnSource.Core, Visible: true, Order: 5),
                    new ColumnDef("hoursToGo", "hourApprovals.columns.hoursToGo", ColumnSource.Core, Visible: true, Order: 10, Format: "decimal"),
                    new ColumnDef("plannedStart", "hourApprovals.columns.plannedStart", ColumnSource.Core, Visible: true, Order: 20, Format: "date"),
                    new ColumnDef("plannedFinish", "hourApprovals.columns.plannedFinish", ColumnSource.Core, Visible: true, Order: 30, Format: "date"),
                ]),
            _ => ViewDefinition.Empty(screenId),
        };

    public IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> GetRowExtensions(
        IReadOnlyList<Guid> taskIds) =>
        new Dictionary<Guid, IReadOnlyDictionary<string, object?>>();
}
