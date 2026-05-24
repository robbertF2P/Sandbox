using Akka.Actor;
using Akka.Hosting;
using AkkaSignalRVuePoc.Api.Models;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.Actors;

namespace AkkaSignalRVuePoc.Api.Endpoints;

public static class MessageEndpoints
{
    private static readonly TimeSpan AskTimeout = TimeSpan.FromSeconds(5);

    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/messages")
            .WithTags("Messages");

        group.MapPost("/", async (
            PostLiveMessageRequest request,
            IRequiredActor<LiveMessageRootActor> rootActor,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return Results.BadRequest(new { Error = "Text is required." });
            }

            try
            {
                var message = await rootActor.ActorRef.Ask<PushMessage>(
                    new PublishLiveMessageCommand(request.Text),
                    AskTimeout,
                    cancellationToken);

                return Results.Ok(message);
            }
            catch (AskTimeoutException)
            {
                return Results.Problem(
                    detail: "Timed out while waiting for the actor system to publish the message.",
                    statusCode: StatusCodes.Status504GatewayTimeout);
            }
            catch (Exception exception) when (exception.InnerException is ArgumentException argumentException)
            {
                return Results.BadRequest(new { Error = argumentException.Message });
            }
        })
            .WithName("PostLiveMessage")
            .WithSummary("Publish a live message through the Akka.NET actor system");

        return app;
    }
}
