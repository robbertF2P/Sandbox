using Akka.Actor;
using Akka.DependencyInjection;
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

    public static Props Props(DependencyResolver resolver) =>
        resolver.Props<SignalRHubPushActor>();

    public static Props Props(
        IHubContext<LiveMessagesHub> hubContext,
        ILogger<SignalRHubPushActor> logger) =>
        Akka.Actor.Props.Create(() => new SignalRHubPushActor(hubContext, logger));

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
