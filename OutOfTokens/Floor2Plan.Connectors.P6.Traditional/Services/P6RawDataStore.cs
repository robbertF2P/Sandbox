using Floor2Plan.Connectors.P6.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace Floor2Plan.Connectors.P6.Traditional.Services
{
    /// <summary>
    /// In-memory staging for records fetched during a sync run.
    /// </summary>
    public sealed class P6RawDataStore
    {
        private readonly Dictionary<P6EntityKind, List<P6BaseRecord>> _records = new();
        private readonly object _lock = new();

        public void Clear()
        {
            lock (_lock)
            {
                _records.Clear();
            }
        }

        public void Append(P6EntityKind kind, IReadOnlyList<P6BaseRecord> records)
        {
            if (records is not { Count: > 0 })
            {
                return;
            }

            lock (_lock)
            {
                if (!_records.TryGetValue(kind, out var bucket))
                {
                    bucket = [];
                    _records[kind] = bucket;
                }

                bucket.AddRange(records);
            }
        }

        public IReadOnlyList<P6BaseRecord> Get(P6EntityKind kind)
        {
            lock (_lock)
            {
                return _records.TryGetValue(kind, out var bucket)
                    ? bucket.ToArray()
                    : [];
            }
        }

        public IReadOnlyDictionary<P6EntityKind, int> GetCounts()
        {
            lock (_lock)
            {
                return _records.ToDictionary(pair => pair.Key, pair => pair.Value.Count);
            }
        }

        public int GetCount(P6EntityKind kind)
        {
            lock (_lock)
            {
                return _records.TryGetValue(kind, out var bucket) ? bucket.Count : 0;
            }
        }
    }
}
