using Akka.Actor;
using AkkaSignalRVuePoc.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Actors;

public sealed class SignalRHubPushActor : ReceiveActor
{
    public SignalRHubPushActor(
        IHubContext<LiveMessagesHub> hubContext,
        ILogger<SignalRHubPushActor> logger)
    {
        ReceiveAsync<PublishActorMessage>(message => PublishAsync(hubContext, logger, message));
    }

    private static async Task PublishAsync(
        IHubContext<LiveMessagesHub> hubContext,
        ILogger logger,
        PublishActorMessage message)
    {
        await hubContext.Clients.All.SendAsync("actorMessage", message.Message);
        logger.LogInformation(
            "Published actor message {Sequence} to SignalR clients",
            message.Message.Sequence);
    }
}
