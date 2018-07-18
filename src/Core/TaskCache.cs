using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace GreenDonut
{
    internal class TaskCache<TKey, TValue>
        : IDisposable
    {
        private ImmutableDictionary<TKey, CacheEntry> _cache =
            ImmutableDictionary<TKey, CacheEntry>.Empty;
        private CancellationTokenSource _dispose;
        private bool _disposed;
        private Task _expiredEntryDetectionCycle;
        private LinkedListNode<TKey> _first;
        private readonly LinkedList<TKey> _ranking = new LinkedList<TKey>();
        private readonly object _sync = new object();

        public TaskCache(int size, TimeSpan slidingExpiration)
        {
            Size = (size < 10) ? 10 : size;
            SlidingExpirartion = slidingExpiration;

            StartExpiredEntryDetectionCycle();
        }

        public int Size { get; }

        public TimeSpan SlidingExpirartion { get; }

        public int Usage => _cache.Count;

        public void Clear()
        {
            _sync.Lock(
                () => _cache.Count > 0,
                () =>
                {
                    _ranking.Clear();
                    _cache = _cache.Clear();
                    _first = null;
                });
        }

        public Task<Result<TValue>> Get(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_cache.TryGetValue(key, out CacheEntry entry))
            {
                TouchEntry(entry);
            }

            return entry?.Value;
        }

        public void Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _sync.Lock(
                () => _cache.ContainsKey(key),
                () =>
                {
                    _ranking.Remove(_cache[key].Rank);
                    _cache.Remove(key);
                    _first = _ranking.First;
                });
        }

        public void Set(TKey key, Task<Result<TValue>> value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _sync.Lock(
                () => !_cache.ContainsKey(key),
                () =>
                {
                    CacheEntry entry = new CacheEntry(key, value);

                    ClearSpaceForNewEntry();
                    _ranking.AddFirst(entry.Rank);
                    _cache = _cache.SetItem(entry.Key, entry);
                    _first = entry.Rank;
                });
        }

        private void TouchEntry(CacheEntry entry)
        {
            _sync.Lock(
                () => _first != entry.Rank,
                () =>
                {
                    entry.LastTouched = DateTimeOffset.UtcNow;
                    _ranking.Remove(entry.Rank);
                    _ranking.AddFirst(entry.Rank);
                    _first = entry.Rank;
                });
        }

        private void ClearSpaceForNewEntry()
        {
            if (_cache.Count >= Size)
            {
                LinkedListNode<TKey> entry = _ranking.Last;

                _cache = _cache.Remove(entry.Value);
                _ranking.Remove(entry);
            }
        }

        private void StartExpiredEntryDetectionCycle()
        {
            if (SlidingExpirartion > TimeSpan.Zero)
            {
                _dispose = new CancellationTokenSource();
                _expiredEntryDetectionCycle = Task.Run(async () =>
                {
                    while (!_dispose.Token.IsCancellationRequested)
                    {
                        DateTimeOffset removeAfter = DateTimeOffset.UtcNow
                            .Subtract(SlidingExpirartion);

                        if (_ranking.Last != null &&
                            _cache.TryGetValue(_ranking.Last.Value,
                                out CacheEntry entry) &&
                            removeAfter > entry.LastTouched)
                        {
                            Remove(entry.Key);
                        }
                        else
                        {
                            await Task.Delay(10).ConfigureAwait(false);
                        }
                    }
                });
            }
        }

        private class CacheEntry
        {
            public CacheEntry(TKey key, Task<Result<TValue>> value)
            {
                Key = key;
                LastTouched = DateTimeOffset.UtcNow;
                Rank = new LinkedListNode<TKey>(key);
                Value = value;
            }

            public TKey Key { get; }

            public DateTimeOffset LastTouched { get; set; }

            public LinkedListNode<TKey> Rank { get; }

            public Task<Result<TValue>> Value { get; }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Clear();
                    _dispose?.Cancel();
                    _expiredEntryDetectionCycle?.Dispose();
                    _dispose?.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
