using AkkaSignalRVuePoc.Api.Models;
using AkkaSignalRVuePoc.Api.Services;

namespace AkkaSignalRVuePoc.Api.Endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects");

        group.MapGet("/", (InMemoryCatalogStore store) => store.GetProjects())
            .WithName("ListProjects")
            .WithSummary("List all projects");

        group.MapGet("/{id:guid}", (Guid id, InMemoryCatalogStore store) =>
            store.GetProject(id) is { } project
                ? Results.Ok(project)
                : Results.NotFound())
            .WithName("GetProject")
            .WithSummary("Get a project by id");

        group.MapPost("/", (CreateProjectRequest request, InMemoryCatalogStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new { Error = "Name is required." });
            }

            if (request.OrganisationId == Guid.Empty)
            {
                return Results.BadRequest(new { Error = "OrganisationId is required." });
            }

            var project = store.CreateProject(request);
            return project is null
                ? Results.NotFound(new { Error = $"Organisation '{request.OrganisationId}' was not found." })
                : Results.Created($"/api/projects/{project.Id}", project);
        })
            .WithName("CreateProject")
            .WithSummary("Create a project");

        return app;
    }
}
