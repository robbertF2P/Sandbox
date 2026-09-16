using Akka.Actor;
using Akka.Event;
using Floor2Plan.Connectors.P6.Api.Models;
using System.Collections.Generic;
using System.Linq;

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

        private readonly Dictionary<P6EntityKind, List<P6BaseRecord>> _records = new ();
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public P6RawDataStoreActor()
        {
            Receive<ClearRawData>(_ =>
            {
                _log.Debug("Clearing P6 raw data from previous run");
                _records.Clear();
            });

            Receive<AppendRawData>(message =>
            {
                if (message.Records.Count == 0)
                {
                    return;
                }

                if (!_records.TryGetValue(message.Kind, out var existing))
                {
                    existing = new List<P6BaseRecord>();
                    _records[message.Kind] = existing;
                }

                existing.AddRange(message.Records);
            });

            Receive<GetRawData>(message =>
            {
                var records = _records.TryGetValue(message.Kind, out var existing)
                    ? existing.ToArray()
                    : [];
                Sender.Tell(new RawData(message.Kind, records));
            });

            Receive<GetRawDataCounts>(_ =>
            {
                Sender.Tell(new RawDataCounts(_records.ToDictionary(x => x.Key, x => x.Value.Count)));
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
