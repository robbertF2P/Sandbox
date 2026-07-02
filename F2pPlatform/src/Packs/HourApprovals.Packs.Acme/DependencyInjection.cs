using HourApprovals.Application.Ports;
using Microsoft.Extensions.DependencyInjection;
using Platform.Shared.View;

namespace HourApprovals.Packs.Acme;

public sealed class AcmeHourApprovalsPack : IHourApprovalsCustomizationPack
{
    public string PackId => "acme-hour-approvals-v1";

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
                    new ColumnDef("sapCostElement", "packs.acme-hour-approvals-v1.columns.sapCostElement", ColumnSource.Extension, Visible: true, Order: 40),
                ]),
            _ => ViewDefinition.Empty(screenId),
        };

    public IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> GetRowExtensions(
        IReadOnlyList<Guid> taskIds)
    {
        Dictionary<Guid, IReadOnlyDictionary<string, object?>> result = new(taskIds.Count);

        foreach (Guid taskId in taskIds)
        {
            result[taskId] = new Dictionary<string, object?>
            {
                ["sapCostElement"] = $"CE-{taskId.ToString()[..4].ToUpperInvariant()}",
            };
        }

        return result;
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddAcmeHourApprovalsPack(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IHourApprovalsCustomizationPack, AcmeHourApprovalsPack>();
        return services;
    }
}
