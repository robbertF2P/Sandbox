using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Akka.Actors.State
{
    public sealed class KeyedAccumulatorState<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> _records = new();

        public void Clear()
        {
            _records.Clear();
        }

        public void Append(TKey key, IReadOnlyList<TValue> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            if (!_records.TryGetValue(key, out var existing))
            {
                existing = new List<TValue>();
                _records[key] = existing;
            }

            existing.AddRange(items);
        }

        public IReadOnlyList<TValue> Get(TKey key)
        {
            return _records.TryGetValue(key, out var existing)
                ? existing.ToArray()
                : [];
        }

        public IReadOnlyDictionary<TKey, int> GetCounts()
        {
            return _records.ToDictionary(x => x.Key, x => x.Value.Count);
        }
    }
}
