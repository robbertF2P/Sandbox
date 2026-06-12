using ApiImportActorPoc.Api.Services;

namespace ApiImportActorPoc.Api.Endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        group.MapGet("/", async (ProjectQueryService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetSummariesAsync(cancellationToken)))
            .WithName("ListProjects")
            .WithSummary("List persisted vessel projects");

        group.MapGet("/{projectId:int}", async (
            int projectId,
            ProjectQueryService service,
            CancellationToken cancellationToken) =>
        {
            var project = await service.GetProjectAsync(projectId, cancellationToken);
            return project is null ? Results.NotFound() : Results.Ok(project);
        })
        .WithName("GetProject")
        .WithSummary("Get a vessel project with nested components, activities, and assignments");

        group.MapGet("/{projectId:int}/export", async (
            int projectId,
            ProjectQueryService service,
            CancellationToken cancellationToken) =>
        {
            var payload = await service.GetImportPayloadAsync(projectId, cancellationToken);
            return payload is null ? Results.NotFound() : Results.Ok(payload);
        })
        .WithName("ExportProject")
        .WithSummary("Export project in import payload format (round-trip test)");

        return app;
    }
}
