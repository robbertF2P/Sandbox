using AkkaSignalRVuePoc.Api.Models;
using AkkaSignalRVuePoc.Api.Services;

namespace AkkaSignalRVuePoc.Api.Endpoints;

public static class OrganisationEndpoints
{
    public static IEndpointRouteBuilder MapOrganisationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organisations")
            .WithTags("Organisations");

        group.MapGet("/", (InMemoryCatalogStore store) => store.GetOrganisations())
            .WithName("ListOrganisations")
            .WithSummary("List all organisations");

        group.MapGet("/{id:guid}", (Guid id, InMemoryCatalogStore store) =>
            store.GetOrganisation(id) is { } organisation
                ? Results.Ok(organisation)
                : Results.NotFound())
            .WithName("GetOrganisation")
            .WithSummary("Get an organisation by id");

        group.MapGet("/{id:guid}/projects", (Guid id, InMemoryCatalogStore store) =>
            store.GetOrganisation(id) is null
                ? Results.NotFound()
                : Results.Ok(store.GetProjectsForOrganisation(id)))
            .WithName("ListOrganisationProjects")
            .WithSummary("List projects for an organisation");

        group.MapPost("/", (CreateOrganisationRequest request, InMemoryCatalogStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new { Error = "Name is required." });
            }

            var organisation = store.CreateOrganisation(request);
            return Results.Created($"/api/organisations/{organisation.Id}", organisation);
        })
            .WithName("CreateOrganisation")
            .WithSummary("Create an organisation");

        return app;
    }
}
