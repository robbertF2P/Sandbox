using <Context>.Application.Ports;
using Platform.Shared.View;

namespace <Context>.Packs.<Client>;

public sealed class <Client><Context>Pack : I<Context>CustomizationPack
{
    public string PackId => "<pack-id>";

    public ViewDefinition GetView(string screenId) =>
        screenId switch
        {
            <Context>Screens.Queue => new ViewDefinition(
                <Context>Screens.Queue,
                [
                    // Core columns — toggle visibility/order per tenant.
                    new ColumnDef(
                        "hoursToGo",
                        "<context>.columns.hoursToGo",
                        ColumnSource.Core,
                        Visible: true,
                        Order: 10,
                        Format: "decimal"),

                    // Extension column — value from GetRowExtensions.
                    // new ColumnDef(
                    //     "exampleField",
                    //     "packs.<pack-id>.columns.exampleField",
                    //     ColumnSource.Extension,
                    //     Visible: true,
                    //     Order: 50),
                ]),
            _ => ViewDefinition.Empty(screenId),
        };

    public IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, object?>> GetRowExtensions(
        IReadOnlyList<Guid> entityIds)
    {
        Dictionary<Guid, IReadOnlyDictionary<string, object?>> result = new(entityIds.Count);

        foreach (Guid entityId in entityIds)
        {
            result[entityId] = new Dictionary<string, object?>
            {
                // ["exampleField"] = null,
            };
        }

        return result;
    }
}
