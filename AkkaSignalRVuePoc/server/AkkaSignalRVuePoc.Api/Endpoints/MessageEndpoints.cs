using AkkaSignalRVuePoc.Api.Models;
using AkkaSignalRVuePoc.Api.Services;
using AkkaSignalRVuePoc.Contracts.Interfaces;

namespace AkkaSignalRVuePoc.Api.Endpoints;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/messages")
            .WithTags("Messages");

        group.MapPost("/", (PostLiveMessageRequest request, IActorSystemCommandFacade commandFacade) =>
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Results.BadRequest(new { Error = "Text is required." });
            }

            commandFacade.SendLiveMessage(request.Text);
            return Results.Accepted((string?)null, new MessageAcceptedResponse());
        })
            .WithName("PostLiveMessage")
            .WithSummary("Publish a live message through the Akka.NET actor system");

        return app;
    }
}
