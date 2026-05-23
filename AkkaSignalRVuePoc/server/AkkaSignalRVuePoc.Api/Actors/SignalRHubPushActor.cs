using Akka.Actor;
using Akka.Event;
using AkkaSignalRVuePoc.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Actors;

public sealed class SignalRHubPushActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IHubContext<LiveMessagesHub> _hubContext;

    public SignalRHubPushActor(IHubContext<LiveMessagesHub> hubContext)
    {
        _hubContext = hubContext;

        ReceiveAsync<PublishActorMessage>(PublishAsync);
    }

    public static Props Props(IHubContext<LiveMessagesHub> hubContext) =>
        Akka.Actor.Props.Create(() => new SignalRHubPushActor(hubContext));

    private async Task PublishAsync(PublishActorMessage message)
    {
        await _hubContext.Clients.All.SendAsync("actorMessage", message.Message);
        _log.Info(
            "Published actor message {Sequence} to SignalR clients",
            message.Message.Sequence);
    }
}
