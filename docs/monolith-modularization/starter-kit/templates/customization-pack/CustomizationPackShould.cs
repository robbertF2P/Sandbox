using <Context>.Application.Ports;
using <Context>.Infrastructure;
using <Context>.Packs.<Client>;
using Platform.Shared.View;

namespace <Context>.Unit.Tests;

[Trait("Module", "<Context>")]
public sealed class <Client><Context>PackShould
{
    [Fact]
    public void DefaultPack_HasExpectedBaseline()
    {
        var pack = new Default<Context>Pack();
        ViewDefinition view = pack.GetView(<Context>Screens.Queue);

        Assert.NotEmpty(view.Columns);
        Assert.Empty(pack.GetRowExtensions([Guid.NewGuid()]));
    }

    [Fact]
    public void ClientPack_UsesPackLabelKeys_AndBatchExtensions()
    {
        var pack = new <Client><Context>Pack();
        ViewDefinition view = pack.GetView(<Context>Screens.Queue);
        var entityId = Guid.Parse("11111111-1111-1111-1111-111111111101");

        Assert.Equal("<pack-id>", pack.PackId);
        Assert.NotEmpty(view.Columns);

        IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> extensions =
            pack.GetRowExtensions([entityId, Guid.NewGuid()]);

        Assert.Equal(2, extensions.Count);
        Assert.True(extensions.ContainsKey(entityId));
    }
}
