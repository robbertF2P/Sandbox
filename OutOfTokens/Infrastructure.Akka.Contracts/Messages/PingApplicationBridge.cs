namespace Infrastructure.Akka.Contracts.Messages
{
    public sealed record PingApplicationBridge(string Text) : IActorCommand;
}
