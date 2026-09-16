using Akka.Actor;

namespace Infrastructure.Akka.Contracts
{
    public static class ActorSystemAccess
    {
        public static IActorRef FrontDesk { get; private set; } = ActorRefs.Nobody;

        public static IActorRef ApplicationBridge { get; private set; } = ActorRefs.Nobody;

        public static IActorRef Customize { get; private set; } = ActorRefs.Nobody;

        public static bool IsInitialized => FrontDesk != ActorRefs.Nobody;

        public static void Initialize(IActorRef frontDesk, IActorRef applicationBridge, IActorRef customize)
        {
            FrontDesk = frontDesk;
            ApplicationBridge = applicationBridge;
            Customize = customize;
        }
    }
}
