using ApiImportActorPoc.Contracts.Interfaces;
using ApiImportActorPoc.Contracts.Models.Import;

namespace ApiImportActorPoc.Api.Endpoints;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/import").WithTags("Import");

        group.MapPost("/", async (ProjectImportPayload payload, IActorSystemCommandFacade facade) =>
        {
            var result = await facade.StartImportAsync(payload);
            if (!result.Accepted)
            {
                return Results.BadRequest(result);
            }

            return Results.Accepted($"/api/import/{result.SessionId}", result);
        })
        .WithName("StartImport")
        .WithSummary("Start importing a project structure into memory");

        group.MapGet("/{sessionId:guid}/model", async (Guid sessionId, IActorSystemCommandFacade facade) =>
        {
            var result = await facade.GetImportModelAsync(sessionId);
            return result.Found ? Results.Ok(result.Model) : Results.NotFound();
        })
        .WithName("GetImportModel")
        .WithSummary("Get the in-memory project model built by actors");

        group.MapPost("/{sessionId:guid}/persist", async (Guid sessionId, IActorSystemCommandFacade facade) =>
        {
            var result = await facade.PersistImportAsync(sessionId);
            return result.Success
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("PersistImport")
        .WithSummary("Persist the in-memory model to EF Core database");

        return app;
    }
}
