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

        Assert.False(view.IsVisible("plannedStart"));
        Assert.False(view.IsVisible("plannedFinish"));
        Assert.Empty(pack.GetRowExtensions(Guid.NewGuid()));
    }

    [Fact]
    public void AcmePack_ShowsPlannedDates_AndSapExtensionColumn()
    {
        var pack = new AcmeHourApprovalsPack();
        ViewDefinition view = pack.GetView(HourApprovalsScreens.Queue);
        var taskId = Guid.Parse("11111111-1111-1111-1111-111111111101");

        Assert.True(view.IsVisible("plannedStart"));
        Assert.True(view.IsVisible("plannedFinish"));
        Assert.True(view.IsVisible("sapCostElement"));

        IReadOnlyDictionary<string, object?> extensions = pack.GetRowExtensions(taskId);
        Assert.Equal("CE-1111", extensions["sapCostElement"]);
    }
}
