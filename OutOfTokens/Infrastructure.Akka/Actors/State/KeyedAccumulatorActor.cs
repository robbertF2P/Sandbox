using Akka.Actor;
using Akka.Event;
using System.Collections.Generic;

namespace Infrastructure.Akka.Actors.State
{
    /// <summary>
    /// In-memory keyed aggregation — single writer, clear / append / query by key.
    /// </summary>
    public sealed class KeyedAccumulatorActor<TKey, TValue> : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private readonly KeyedAccumulatorState<TKey, TValue> _state = new();

        public KeyedAccumulatorActor()
        {
            Receive<Clear>(message =>
            {
                _ = message;
                _log.Debug("Clearing keyed accumulator");
                _state.Clear();
            });

            Receive<Append>(message => _state.Append(message.Key, message.Items));

            Receive<Get>(message => Sender.Tell(new GetResult(message.Key, _state.Get(message.Key))));

            Receive<GetCounts>(message =>
            {
                _ = message;
                Sender.Tell(new CountsResult(_state.GetCounts()));
            });
        }

        public static Props Props()
        {
            return global::Akka.Actor.Props.Create(() => new KeyedAccumulatorActor<TKey, TValue>());
        }

        public sealed record Clear;

        public sealed record Append(TKey Key, IReadOnlyList<TValue> Items);

        public sealed record Get(TKey Key);

        public sealed record GetResult(TKey Key, IReadOnlyList<TValue> Items);

        public sealed record GetCounts;

        public sealed record CountsResult(IReadOnlyDictionary<TKey, int> Counts);
    }
}
