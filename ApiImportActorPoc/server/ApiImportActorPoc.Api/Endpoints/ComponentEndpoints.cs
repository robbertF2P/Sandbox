using ApiImportActorPoc.Contracts.Models;
using ApiImportActorPoc.Core.Templates;

namespace ApiImportActorPoc.Api.Endpoints;

public static class ComponentEndpoints
{
    public static IEndpointRouteBuilder MapComponentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/components").WithTags("Components");

        group.MapPatch("/{componentId:int}/template", async (
            int componentId,
            SetComponentTemplateRequest request,
            ComponentTemplateService service,
            CancellationToken cancellationToken) =>
        {
            var component = await service.SetTemplateAsync(componentId, request.IsTemplate, cancellationToken);
            return component is null ? Results.NotFound() : Results.Ok(component);
        })
        .WithName("SetComponentTemplate")
        .WithSummary("Mark or unmark a component as a reusable template");

        group.MapPost("/{templateComponentId:int}/instantiate", async (
            int templateComponentId,
            InstantiateComponentFromTemplateRequest request,
            ComponentTemplateService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await service.InstantiateAsync(templateComponentId, request, cancellationToken);
                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new { error = exception.Message });
            }
        })
        .WithName("InstantiateComponentFromTemplate")
        .WithSummary("Create a new component from a template with open assignments and budgeted hours");

        return app;
    }
}
