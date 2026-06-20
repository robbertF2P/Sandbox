using Akka.Actor;
using Platform.Serilog.Logging.Akka;

namespace AkkaSignalRVuePoc.Api.Tests.Actors;

internal static class ActorTestCorrelation
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    public static Task<TResponse> AskAsync<TResponse>(
        IActorRef actor,
        object message,
        string useCase = "Test",
        CancellationToken cancellationToken = default) =>
        actor.AskCorrelated<TResponse>(message, useCase, DefaultTimeout, cancellationToken);
}
