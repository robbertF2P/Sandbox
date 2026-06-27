using HourApprovals.Application.Ports;
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
                    new ColumnDef("hoursToGo", "Hours to go", ColumnSource.Core, Visible: true, Order: 10),
                    new ColumnDef("progress", "Progress", ColumnSource.Core, Visible: true, Order: 20, Format: "percent"),
                    new ColumnDef("plannedStart", "Planned start", ColumnSource.Core, Visible: true, Order: 30, Format: "date"),
                    new ColumnDef("plannedFinish", "Planned finish", ColumnSource.Core, Visible: true, Order: 40, Format: "date"),
                    new ColumnDef("sapCostElement", "SAP cost element", ColumnSource.Extension, Visible: true, Order: 50),
                ]),
            _ => ViewDefinition.Empty(screenId),
        };

    public IReadOnlyDictionary<string, object?> GetRowExtensions(Guid taskId) =>
        new Dictionary<string, object?>
        {
            ["sapCostElement"] = $"CE-{taskId.ToString()[..4].ToUpperInvariant()}",
        };
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
