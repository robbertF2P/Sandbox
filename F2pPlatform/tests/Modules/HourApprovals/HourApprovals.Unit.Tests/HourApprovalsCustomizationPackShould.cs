using HourApprovals.Application.Ports;
using HourApprovals.Infrastructure;
using HourApprovals.Packs.Acme;
using Platform.Shared.View;

namespace HourApprovals.Unit.Tests;

[Trait("Module", "HourApprovals")]
public sealed class HourApprovalsCustomizationPackShould
{
    [Fact]
    public void DefaultPack_HidesPlannedDates_AndHasNoExtensions()
    {
        var pack = new DefaultHourApprovalsPack();
        ViewDefinition view = pack.GetView(HourApprovalsScreens.Queue);
        var taskId = Guid.NewGuid();

        Assert.False(view.IsVisible("plannedStart"));
        Assert.False(view.IsVisible("plannedFinish"));
        Assert.Empty(pack.GetRowExtensions([taskId]));
    }

    [Fact]
    public void AcmePack_UsesLabelKeys_AndBatchExtensions()
    {
        var pack = new AcmeHourApprovalsPack();
        ViewDefinition view = pack.GetView(HourApprovalsScreens.Queue);
        var taskId = Guid.Parse("11111111-1111-1111-1111-111111111101");

        ColumnDef? plannedStart = view.FindColumn("plannedStart");
        Assert.NotNull(plannedStart);
        Assert.Equal("hourApprovals.columns.plannedStart", plannedStart.LabelKey);

        ColumnDef? sapColumn = view.FindColumn("sapCostElement");
        Assert.NotNull(sapColumn);
        Assert.Equal("packs.acme-hour-approvals-v1.columns.sapCostElement", sapColumn.LabelKey);

        Assert.True(view.IsVisible("daysSinceLastSubmission"));

        IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> extensions =
            pack.GetRowExtensions([taskId, Guid.NewGuid()]);

        Assert.Equal("CE-1111", extensions[taskId]["sapCostElement"]);
        Assert.Equal(2, extensions.Count);
    }
}
