using Akka.Actor;
using Floor2Plan.Connectors.P6.Actors;
using Floor2Plan.Connectors.P6.Api.Models;
using System.Collections.Generic;

namespace Floor2Plan.Connectors.P6.Messages
{
    public interface IP6Request
    {
        IP6Paging Paging { get; set; }
        IActorRef OriginalSender { get; set; }
    };

    public interface IP6Paging
    {
        IEnumerable<P6ProjectRecord> Projects { get; set; } //todo a little doubtful
        int Offset { get; set; }
        P6WorkflowKind Kind { get; set; }
    }
}