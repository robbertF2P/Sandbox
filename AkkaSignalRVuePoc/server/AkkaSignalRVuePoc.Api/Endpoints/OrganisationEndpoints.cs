using AkkaSignalRVuePoc.Api.Models;
using AkkaSignalRVuePoc.Contracts.Interfaces;

namespace AkkaSignalRVuePoc.Api.Endpoints;

public static class OrganisationEndpoints
{
    public static IEndpointRouteBuilder MapOrganisationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/organisations")
            .WithTags("Organisations");

        group.MapGet("/", async (IActorSystemCommandFacade facade, CancellationToken cancellationToken) =>
        {
            var organisations = await facade.GetOrganisationsAsync(cancellationToken);
            return Results.Ok(organisations.Select(CatalogModelMapper.ToApiModel));
        })
            .WithName("ListOrganisations")
            .WithSummary("List all organisations");

        group.MapGet("/{id:guid}", async (Guid id, IActorSystemCommandFacade facade, CancellationToken cancellationToken) =>
        {
            var organisation = await facade.GetOrganisationAsync(id, cancellationToken);
            return organisation is null
                ? Results.NotFound()
                : Results.Ok(CatalogModelMapper.ToApiModel(organisation));
        })
            .WithName("GetOrganisation")
            .WithSummary("Get an organisation by id");

        group.MapGet("/{id:guid}/projects", async (
            Guid id,
            IActorSystemCommandFacade facade,
            CancellationToken cancellationToken) =>
        {
            var response = await facade.GetProjectsForOrganisationAsync(id, cancellationToken);
            return response.OrganisationExists
                ? Results.Ok(response.Projects.Select(CatalogModelMapper.ToApiModel))
                : Results.NotFound();
        })
            .WithName("ListOrganisationProjects")
            .WithSummary("List projects for an organisation");

        group.MapPost("/", async (
            CreateOrganisationRequest request,
            IActorSystemCommandFacade facade,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest(new { Error = "Name is required." });
            }

            var organisation = await facade.CreateOrganisationAsync(request.Name, cancellationToken);
            var apiModel = CatalogModelMapper.ToApiModel(organisation);
            return Results.Created($"/api/organisations/{apiModel.Id}", apiModel);
        })
            .WithName("CreateOrganisation")
            .WithSummary("Create an organisation");

        return app;
    }
}
