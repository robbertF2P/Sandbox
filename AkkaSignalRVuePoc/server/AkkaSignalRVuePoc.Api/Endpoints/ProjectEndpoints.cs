using AkkaSignalRVuePoc.Api.Models;
using AkkaSignalRVuePoc.Api.Services;

namespace AkkaSignalRVuePoc.Api.Endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects");

        group.MapGet("/", async (IActorSystemCommandFacade facade, CancellationToken cancellationToken) =>
        {
            var projects = await facade.GetProjectsAsync(cancellationToken);
            return Results.Ok(projects.Select(CatalogModelMapper.ToApiModel));
        })
            .WithName("ListProjects")
            .WithSummary("List all projects");

        group.MapGet("/{id:guid}", async (Guid id, IActorSystemCommandFacade facade, CancellationToken cancellationToken) =>
        {
            var project = await facade.GetProjectAsync(id, cancellationToken);
            return project is null
                ? Results.NotFound()
                : Results.Ok(CatalogModelMapper.ToApiModel(project));
        })
            .WithName("GetProject")
            .WithSummary("Get a project by id");

        group.MapPost("/", async (
            CreateProjectRequest request,
            IActorSystemCommandFacade facade,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new { Error = "Name is required." });
            }

            if (request.OrganisationId == Guid.Empty)
            {
                return Results.BadRequest(new { Error = "OrganisationId is required." });
            }

            var response = await facade.CreateProjectAsync(
                request.OrganisationId,
                request.Name,
                request.Description,
                cancellationToken);

            return response switch
            {
                { OrganisationExists: false } => Results.NotFound(new
                {
                    Error = $"Organisation '{request.OrganisationId}' was not found."
                }),
                { Project: { } project } => Results.Created(
                    $"/api/projects/{project.Id}",
                    CatalogModelMapper.ToApiModel(project)),
                _ => Results.BadRequest(new { Error = "Project could not be created." })
            };
        })
            .WithName("CreateProject")
            .WithSummary("Create a project");

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProjectRequest request,
            IActorSystemCommandFacade facade,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) && request.Description is null)
            {
                return Results.BadRequest(new { Error = "At least one of Name or Description must be provided." });
            }

            var response = await facade.UpdateProjectAsync(
                id,
                request.Name,
                request.Description,
                cancellationToken);

            return response switch
            {
                { Exists: false } => Results.NotFound(),
                { Project: { } project } => Results.Ok(CatalogModelMapper.ToApiModel(project)),
                _ => Results.BadRequest(new { Error = "Project could not be updated." })
            };
        })
            .WithName("UpdateProject")
            .WithSummary("Update a project");

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IActorSystemCommandFacade facade,
            CancellationToken cancellationToken) =>
        {
            var response = await facade.DeleteProjectAsync(id, cancellationToken);
            return response.Exists ? Results.NoContent() : Results.NotFound();
        })
            .WithName("DeleteProject")
            .WithSummary("Delete a project");

        return app;
    }
}
