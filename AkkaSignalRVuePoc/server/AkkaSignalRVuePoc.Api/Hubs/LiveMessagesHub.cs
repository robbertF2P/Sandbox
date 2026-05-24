using Akka.Actor;
using Akka.Hosting;
using AkkaSignalRVuePoc.Contracts.Messages;
using AkkaSignalRVuePoc.Core.Actors;
using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Hubs;

public sealed class LiveMessagesHub : Hub
{
    private readonly IRequiredActor<LiveMessageRootActor> _rootActor;

    public LiveMessagesHub(IRequiredActor<LiveMessageRootActor> rootActor)
    {
        _rootActor = rootActor;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("serverMessage", new PushMessage(
            Sequence: 0,
            Text: "Connected to the SignalR hub. Waiting for Akka.NET actor messages...",
            SentAt: DateTimeOffset.UtcNow,
            Source: "SignalR Hub"));

        await base.OnConnectedAsync();
    }

    public Task PostMessage(string text)
    {
        _rootActor.ActorRef.Tell(new PublishLiveMessageCommand(text));
        return Task.CompletedTask;
    }
}
