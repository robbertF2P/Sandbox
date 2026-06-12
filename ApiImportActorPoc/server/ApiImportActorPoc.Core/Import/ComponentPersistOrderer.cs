using ApiImportActorPoc.Contracts.Models;

namespace ApiImportActorPoc.Core.Import;

public static class ComponentPersistOrderer
{
    public static ProjectModel OrderTemplatesFirst(ProjectModel model) =>
        model with { Components = OrderComponents(model.Components) };

    private static IReadOnlyList<ComponentModel> OrderComponents(IReadOnlyList<ComponentModel> components) =>
        components
            .OrderByDescending(component => component.IsTemplate)
            .ThenBy(component => component.Name, StringComparer.OrdinalIgnoreCase)
            .Select(component => component with { ChildComponents = OrderComponents(component.ChildComponents) })
            .ToList();
}
