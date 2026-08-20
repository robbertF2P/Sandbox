using AkkaAspirePoc.Api.Actors;
using AkkaAspirePoc.Api.Actors.Messages;
using AkkaAspirePoc.Data;
using Microsoft.AspNetCore.Mvc;

namespace AkkaAspirePoc.Api.Endpoints;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos").WithTags("Todos");

        group.MapGet("/", async (TodoActorFacade facade, CancellationToken cancellationToken) =>
        {
            var result = await facade.GetAllAsync(cancellationToken);
            return Results.Ok(result.Todos);
        });

        group.MapPost("/", async ([FromBody] CreateTodoRequest request, TodoActorFacade facade, CancellationToken cancellationToken) =>
        {
            var result = await facade.CreateAsync(request.Title, cancellationToken);
            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.Error });
            }

            return Results.Created($"/api/todos/{result.TodoId}", new { id = result.TodoId });
        });

        group.MapPost("/{id:guid}/complete", async (Guid id, TodoActorFacade facade, CancellationToken cancellationToken) =>
        {
            var result = await facade.CompleteAsync(id, cancellationToken);
            return result.Success
                ? Results.NoContent()
                : Results.NotFound(new { error = result.Error });
        });

        return app;
    }
}

public sealed record CreateTodoRequest(string Title);
