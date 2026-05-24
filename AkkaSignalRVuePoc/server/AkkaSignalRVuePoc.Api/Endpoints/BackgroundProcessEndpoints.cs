using AkkaSignalRVuePoc.Api.Models;
using AkkaSignalRVuePoc.Api.Services;

namespace AkkaSignalRVuePoc.Api.Endpoints;

public static class BackgroundProcessEndpoints
{
    public static IEndpointRouteBuilder MapBackgroundProcessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/background-process")
            .WithTags("Background Process");

        group.MapPost("/start", (IActorSystemCommandFacade commandFacade) =>
        {
            commandFacade.StartBackgroundProcess();
            return Results.Accepted((string?)null, new MessageAcceptedResponse());
        })
            .WithName("StartBackgroundProcess")
            .WithSummary("Start a long-running background process through the actor system");

        return app;
    }
}
