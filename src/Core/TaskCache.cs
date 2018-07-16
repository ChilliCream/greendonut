using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenDonut
{
    public class TaskCache<TKey, TValue>
        : IDisposable
    {
        private readonly object _sync = new object();
        private bool _disposed;
        private Dictionary<TKey, TaskCacheItem<TKey, TValue>> _map;
        private TimeSpan _slidingExpiration;

        public TaskCache(TimeSpan slidingExpiration)
        {
            _slidingExpiration = slidingExpiration;
            _map = new Dictionary<TKey, TaskCacheItem<TKey, TValue>>();
        }

        public void Clear()
        {
            Dictionary<TKey, TaskCacheItem<TKey, TValue>> oldMap;

            if (_map.Count > 0)
            {
                lock (_sync)
                {
                    if (_map.Count > 0)
                    {
                        oldMap = _map;
                        _map = new Dictionary<TKey, TaskCacheItem<TKey,
                            TValue>>();

                        Task.Run(() =>
                        {
                            foreach (TKey key in oldMap.Keys)
                            {
                                oldMap[key].Dispose();
                            }

                            oldMap.Clear();
                        });
                    }
                }
            }
        }

        public Task<Result<TValue>> Get(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return (_map.TryGetValue(key,
                    out TaskCacheItem<TKey, TValue> item))
                ? item.Value
                : default;
        }

        public void Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_map.ContainsKey(key))
            {
                lock (_sync)
                {
                    if (_map.ContainsKey(key))
                    {
                        var newMap = new Dictionary<TKey, TaskCacheItem<TKey,
                            TValue>>(_map);

                        newMap.Remove(key);
                        _map = newMap;
                    }
                }
            }
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

            if (!_map.ContainsKey(key))
            {
                lock (_sync)
                {
                    if (!_map.ContainsKey(key))
                    {
                        var newMap = new Dictionary<TKey, TaskCacheItem<TKey,
                            TValue>>(_map);
                        var item = new TaskCacheItem<TKey, TValue>(this, key,
                            value, _slidingExpiration);

                        newMap.Add(key, item);
                        _map = newMap;
                    }
                }
            }
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
                }

                _map = null;

                _disposed = true;
            }
        }

        #endregion
    }
}
