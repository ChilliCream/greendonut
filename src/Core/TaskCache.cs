using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace GreenDonut
{
    internal class TaskCache<TKey, TValue>
        : ITaskCache<TKey, TValue>
        , IDisposable
    {
        private ImmutableDictionary<TKey, CacheEntry> _cache =
            ImmutableDictionary<TKey, CacheEntry>.Empty;
        private CancellationTokenSource _dispose;
        private bool _disposed;
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
                    _cache = _cache.Remove(key);
                    _first = _ranking.First;
                });
        }

        public bool TryAdd(TKey key, Task<TValue> value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var added = false;

            _sync.Lock(
                () => !_cache.ContainsKey(key),
                () =>
                {
                    var entry = new CacheEntry(key, value);

                    ClearSpaceForNewEntry();
                    _ranking.AddFirst(entry.Rank);
                    _cache = _cache.Add(entry.Key, entry);
                    _first = entry.Rank;
                    added = true;
                });

            return added;
        }

        public bool TryGetValue(TKey key, out Task<TValue> value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_cache.TryGetValue(key, out CacheEntry entry))
            {
                TouchEntry(entry);
                value = entry.Value;

                return true;
            }

            value = null;

            return false;
        }

        private void TouchEntry(CacheEntry entry)
        {
            lock (_sync)
            {
                entry.LastTouched = DateTimeOffset.UtcNow;

                if (_first != entry.Rank)
                {
                    _ranking.Remove(entry.Rank);
                    _ranking.AddFirst(entry.Rank);
                    _first = entry.Rank;
                }
            }
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
                Task.Factory.StartNew(async () =>
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
                            await Task.Delay(10, _dispose.Token)
                                .ConfigureAwait(false);
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }

        private class CacheEntry
        {
            public CacheEntry(TKey key, Task<TValue> value)
            {
                Key = key;
                LastTouched = DateTimeOffset.UtcNow;
                Rank = new LinkedListNode<TKey>(key);
                Value = value;
            }

            public TKey Key { get; }

            public DateTimeOffset LastTouched { get; set; }

            public LinkedListNode<TKey> Rank { get; }

            public Task<TValue> Value { get; }
        }

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Clear();
                    _dispose?.Cancel();
                    _dispose?.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
