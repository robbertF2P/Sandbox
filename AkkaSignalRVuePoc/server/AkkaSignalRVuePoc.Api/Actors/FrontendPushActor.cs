using Akka.Actor;
using AkkaSignalRVuePoc.Api.Hubs;
using AkkaSignalRVuePoc.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace AkkaSignalRVuePoc.Api.Actors;

public sealed class FrontendPushActor : ReceiveActor, IWithTimers
{
    private const string TimerKey = "frontend-message-push";

    private readonly IHubContext<LiveMessagesHub> _hubContext;
    private readonly ILogger<FrontendPushActor> _logger;
    private long _sequence;

    public FrontendPushActor(
        IHubContext<LiveMessagesHub> hubContext,
        ILogger<FrontendPushActor> logger)
    {
        _hubContext = hubContext;
        _logger = logger;

        ReceiveAsync<PushTick>(PublishMessageAsync);
    }

    public ITimerScheduler Timers { get; set; } = null!;

    protected override void PreStart()
    {
        Self.Tell(PushTick.Instance);
        Timers.StartPeriodicTimer(TimerKey, PushTick.Instance, TimeSpan.FromSeconds(5));
    }

    protected override void PostStop()
    {
        Timers.Cancel(TimerKey);
    }

    private async Task PublishMessageAsync(PushTick _)
    {
        var sequence = ++_sequence;
        var message = new PushMessage(
            Sequence: sequence,
            Text: $"Akka.NET actor heartbeat #{sequence}",
            SentAt: DateTimeOffset.UtcNow,
            Source: Self.Path.ToStringWithoutAddress());

        await _hubContext.Clients.All.SendAsync("actorMessage", message);
        _logger.LogInformation("Published actor message {Sequence} to SignalR clients", sequence);
    }

    private sealed class PushTick
    {
        public static readonly PushTick Instance = new();

        private PushTick()
        {
        }
    }
}
