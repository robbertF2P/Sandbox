using Akka.Actor;

namespace Infrastructure.Akka.Contracts.Messages
{
    public sealed record InitializeActorSystem(Props ApplicationBridgeProps);
}
