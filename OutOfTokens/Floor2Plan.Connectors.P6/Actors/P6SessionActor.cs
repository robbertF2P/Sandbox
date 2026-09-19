using Akka.Actor;
using Floor2Plan.Connectors.P6.Api;
using Infrastructure.Akka.Actors.Session;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Owns the P6 REST session: login, logout, idle timeout, and dispatching authenticated work to child actors.
    /// </summary>
    public static class P6SessionActor
    {
        public const string ActorName = "p6-session";

        public static Props Props(IP6RestApi api, P6AuthOptions authOptions)
        {
            return SessionGateActor.Props(new P6SessionGateBehavior(api, authOptions));
        }
    }
}
