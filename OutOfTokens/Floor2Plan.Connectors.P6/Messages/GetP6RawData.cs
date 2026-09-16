using Akka.Actor;

namespace Floor2Plan.Connectors.P6.Messages
{
    public sealed record GetP6RawData:IP6Request
    {
        public IP6Paging Paging { get; set; } = new Paging();
        public IActorRef OriginalSender { get; set; }
    }
}
