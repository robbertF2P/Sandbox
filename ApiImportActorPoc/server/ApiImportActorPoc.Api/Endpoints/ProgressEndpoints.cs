using ApiImportActorPoc.Api.Services;

namespace ApiImportActorPoc.Api.Endpoints;

public static class ProgressEndpoints
{
    public static IEndpointRouteBuilder MapProgressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Progress");

        group.MapGet("/{projectId:int}/progress", async (
            int projectId,
            ProgressQueryService service,
            CancellationToken cancellationToken) =>
        {
            var progress = await service.GetProjectProgressAsync(projectId, cancellationToken);
            return progress is null ? Results.NotFound() : Results.Ok(progress);
        })
        .WithName("GetProjectProgress")
        .WithSummary("Budgeted vs worked hours rolled up from assignments to project");

        return app;
    }
}
