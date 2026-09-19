using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api.Models;
using Infrastructure.Akka.Actors.State;
using System.Collections.Generic;

namespace Floor2Plan.Connectors.P6.Actors
{
    /// <summary>
    /// Owns the raw records collected during a P6 synchronization run.
    /// </summary>
    /// <remarks>
    /// State lives in the actor and is only reachable through messages, so the run data
    /// needs no synchronization. Records are held in memory until the staging shapes settle.
    /// </remarks>
    public sealed class P6RawDataStoreActor : ReceiveActor
    {
        public const string ActorName = "p6-raw-data-store";

        private readonly KeyedAccumulatorState<P6EntityKind, P6BaseRecord> _state = new();
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public P6RawDataStoreActor()
        {
            Receive<ClearRawData>(_ =>
            {
                _log.Debug("Clearing P6 raw data from previous run");
                _state.Clear();
            });

            Receive<AppendRawData>(message => _state.Append(message.Kind, message.Records));

            Receive<GetRawData>(message =>
            {
                Sender.Tell(new RawData(message.Kind, _state.Get(message.Kind)));
            });

            Receive<GetRawDataCounts>(_ =>
            {
                Sender.Tell(new RawDataCounts(_state.GetCounts()));
            });
        }

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new P6RawDataStoreActor());
        }

        public sealed record ClearRawData;

        public sealed record AppendRawData(P6EntityKind Kind, IReadOnlyList<P6BaseRecord> Records);

        public sealed record GetRawData(P6EntityKind Kind);

        public sealed record RawData(P6EntityKind Kind, IReadOnlyList<P6BaseRecord> Records);

        public sealed record GetRawDataCounts;

        public sealed record RawDataCounts(IReadOnlyDictionary<P6EntityKind, int> Counts)
        {
            public int GetCount(P6EntityKind kind)
            {
                return Counts.GetValueOrDefault(kind, 0);
            }
        }
    }
}
