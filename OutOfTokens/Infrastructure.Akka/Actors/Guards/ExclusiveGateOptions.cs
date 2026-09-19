using Akka.Actor;
using System;

namespace Infrastructure.Akka.Actors.Guards
{
    public sealed class ExclusiveGateOptions
    {
        public required Action<object, IActorRef> OnBegin { get; init; }

        public Func<object, object> BuildRejection { get; init; }
    }
}
