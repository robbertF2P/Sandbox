using Akka.Actor;

namespace Floor2Plan.Connectors.P6.Messages
{
    /// <summary>
    /// Asks the P6 connector for the project catalog used to build the connector configuration screen.
    /// </summary>
    public sealed record GetP6Projects: IP6Request
    {
        public IP6Paging Paging { get; set; } = new Paging();
        public IActorRef OriginalSender { get; set; }
    }
}
