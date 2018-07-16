using System;
using System.Threading;
using System.Threading.Tasks;

namespace GreenDonut
{
    public class TaskCacheItem<TKey, TValue>
        : IDisposable
    {
        private static readonly TimeSpan _timerPeriod =
            TimeSpan.FromMilliseconds(-1);
        private readonly object _sync = new object();
        private int _hits = 0;
        private bool _disposed;
        private Timer _expirationTimer;
        private TKey _key;
        private TaskCache<TKey, TValue> _owner;
        private TimeSpan _slidingExpiration;
        private Task<Result<TValue>> _value;

        internal TaskCacheItem(TaskCache<TKey, TValue> owner, TKey key,
            Task<Result<TValue>> value, TimeSpan slidingExpiration)
        {
            _owner = owner;
            _key = key;
            _value = value;
            _slidingExpiration = slidingExpiration;

            ResetExpirationTimer();
        }

        public int Hits
        {
            get
            {
                return _hits;
            }
        }

        public Task<Result<TValue>> Value
        {
            get
            {
                Interlocked.Increment(ref _hits);
                ResetExpirationTimer();

                return _value;
            }
        }

        public void ResetExpirationTimer()
        {
            Lock(() => _slidingExpiration > TimeSpan.Zero && !_disposed, () =>
            {
                if (_expirationTimer == null)
                {
                    _expirationTimer = new Timer(state =>
                    {
                        Lock(() => !_disposed, Dispose);
                    }, null, _slidingExpiration, _timerPeriod);
                }
                else
                {
                    _expirationTimer.Change(_slidingExpiration, _timerPeriod);
                }
            });
        }

        private void Lock(Func<bool> predicate, Action execute)
        {
            if (predicate())
            {
                lock (_sync)
                {
                    if (predicate())
                    {
                        execute();
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
                    _expirationTimer.Dispose();
                    _owner.Remove(_key);
                }

                _expirationTimer = null;
                _owner = null;

                _disposed = true;
            }
        }

        #endregion
    }
}
