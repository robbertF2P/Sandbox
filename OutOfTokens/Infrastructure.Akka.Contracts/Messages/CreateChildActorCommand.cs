using Akka.Actor;

namespace Infrastructure.Akka.Contracts.Messages
{
    public sealed record CreateChildActorCommand(string Name, Props Props);
}
