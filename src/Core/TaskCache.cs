using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GreenDonut
{
    internal class TaskCache<TKey, TValue>
        : ITaskCache<TKey, TValue>
        , IDisposable
    {
        private const int _minimumCacheSize = 10;
        private readonly ConcurrentDictionary<TKey, CacheEntry> _cache =
            new ConcurrentDictionary<TKey, CacheEntry>();
        private CancellationTokenSource _dispose;
        private bool _disposed;
        private readonly LinkedList<TKey> _ranking = new LinkedList<TKey>();
        private readonly object _sync = new object();

        public TaskCache(int size, TimeSpan slidingExpiration)
        {
            Size = (_minimumCacheSize > size) ? _minimumCacheSize : size;
            SlidingExpirartion = slidingExpiration;

            StartExpiredEntryDetectionCycle();
        }

        public int Size { get; }

        public TimeSpan SlidingExpirartion { get; }

        public int Usage => _cache.Count;

        public void Clear()
        {
            lock (_sync)
            {
                _ranking.Clear();
                _cache.Clear();
            }
        }

        public void Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (_sync)
            {
                if (_cache.TryRemove(key, out CacheEntry entry))
                {
                    _ranking.Remove(entry.Rank);
                }
            }
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

                    if (_cache.TryAdd(entry.Key, entry))
                    {
                        EnsureCacheSizeDoesNotExceed();
                        _ranking.AddFirst(entry.Rank);
                        added = true;
                    }
                });

            return added;
        }

        public bool TryGetValue(TKey key, out Task<TValue> value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var exists = false;
            Task<TValue> cachedValue = null;

            lock (_sync)
            {
                if (_cache.TryGetValue(key, out CacheEntry entry))
                {
                    TouchEntry(entry);
                    cachedValue = entry.Value;
                    exists = true;
                }
            }

            value = cachedValue;

            return exists;
        }

        private void EnsureCacheSizeDoesNotExceed()
        {
            if (_cache.Count > Size)
            {
                TKey key = _ranking.Last.Value;

                if (_cache.TryRemove(key, out CacheEntry entry))
                {
                    _ranking.Remove(entry.Rank);
                }
            }
        }

        private void TouchEntry(CacheEntry entry)
        {
            entry.LastTouched = DateTimeOffset.UtcNow;

            if (_ranking.First != entry.Rank)
            {
                _ranking.Remove(entry.Rank);
                _ranking.AddFirst(entry.Rank);
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

        /// <inheritdoc/>
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
