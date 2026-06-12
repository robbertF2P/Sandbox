using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Core.Import;

namespace ApiImportActorPoc.Api.Tests.Import;

public sealed class ComponentPersistOrdererTests
{
    [Fact]
    public void OrderTemplatesFirst_PlacesTemplatesBeforeSiblingsAtEveryLevel()
    {
        var model = new ProjectModel(
            1,
            "MV Test",
            [
                Component(10, "Regular block", false, [
                    Component(11, "Child regular", false),
                    Component(12, "Child template", true)
                ]),
                Component(20, "Template block", true),
                Component(30, "Another regular", false)
            ],
            []);

        var ordered = ComponentPersistOrderer.OrderTemplatesFirst(model);

        Assert.Equal([20, 10, 30], ordered.Components.Select(component => component.Id));
        Assert.Equal([12, 11], ordered.Components[1].ChildComponents.Select(component => component.Id));
    }

    private static ComponentModel Component(
        int id,
        string name,
        bool isTemplate,
        IReadOnlyList<ComponentModel>? children = null,
        IReadOnlyList<ActivityModel>? activities = null) =>
        new(id, name, isTemplate, children ?? [], activities ?? [], new Dictionary<string, string>());
}
