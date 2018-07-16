using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GreenDonut
{
    /// <summary>
    /// A <see cref="DataLoader"/> creates a public API for loading data from a
    /// particular data back-end with unique keys such as the `id` column of a
    /// SQL table or document name in a MongoDB database, given a batch loading
    /// function. -- facebook
    ///
    /// Each `DataLoader` instance contains a unique memoized cache.Use caution
    /// when used in long-lived applications or those which serve many users
    /// with different access permissions and consider creating a new instance
    /// per web request. -- facebook
    ///
    /// This <see cref="DataLoader"/> in specific runs a single background
    /// thread which one by one fires batch requests if the buffer is filled.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class DataLoader<TKey, TValue>
        : IDisposable
    {
        private readonly object _sync = new object(); 
        private bool _disposed;
        private Task _batchDispatcher;
        private TaskCompletionBuffer<TKey, TValue> _buffer;
        private TaskCache<TKey, TValue> _cache;
        private FetchDataDelegate<TKey, TValue> _fetch;
        private DataLoaderOptions<TKey, TValue> _options;
        private CancellationTokenSource _stopBatching;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataLoader{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="fetch">A batch data fetch delegate. Will be called
        /// every time when trying to setup a new batch request.</param>
        /// <param name="options"><see cref="DataLoader{TKey, TValue}"/>
        /// options.</param>
        public DataLoader(FetchDataDelegate<TKey, TValue> fetch,
            DataLoaderOptions<TKey, TValue> options)
        {
            _fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _buffer = new TaskCompletionBuffer<TKey, TValue>();
            _cache = new TaskCache<TKey, TValue>(_options.SlidingExpiration);

            StartAsyncBatchDispatching();
        }

        public DataLoader<TKey, TValue> Clear()
        {
            _cache.Clear();

            return this;
        }

        public Task<Result<TValue>> LoadAsync(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_options.DisableCaching)
            {
                Task<Result<TValue>> cachedValue = _cache.Get(key);

                if (cachedValue != null)
                {
                    return cachedValue;
                }
            }

            var promise = new TaskCompletionSource<Result<TValue>>();

            if (_options.DisableBatching)
            {
                // note: must run in the background; do not await here.
                Task.Run(() => DispatchAsync(key, promise));
            }
            else
            {
                _buffer.TryAdd(key, promise);
            }

            if (!_options.DisableCaching)
            {
                _cache.Set(key, promise.Task);
            }

            return promise.Task;
        }

        public Task<Result<TValue>[]> LoadAsync(TKey[] keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(keys),
                    "There must be at least one key");
            }

            return Task.WhenAll(keys.Select(LoadAsync));
        }

        public DataLoader<TKey, TValue> Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            _cache.Remove(key);

            return this;
        }

        public DataLoader<TKey, TValue> Set(TKey key,
            Task<Result<TValue>> value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (_cache.Get(key) == null)
            {
                _cache.Set(key, value);
            }

            return this;
        }

        private TaskCompletionBuffer<TKey, TValue> CopyAndClearBuffer()
        {
            lock (_sync)
            {
                TaskCompletionBuffer<TKey, TValue> copy = _buffer;

                _buffer = new TaskCompletionBuffer<TKey, TValue>();

                return copy;
            }
        }

        private async Task DispatchAsync(TKey key,
            TaskCompletionSource<Result<TValue>> promise)
        {
            TKey[] keys = new [] { key };
            Result<TValue>[] values = await _fetch(keys)
                .ConfigureAwait(false);

            promise.SetResult(values[0]);
        }

        private async Task DispatchBatchAsync()
        {
            TaskCompletionBuffer<TKey, TValue> copy = CopyAndClearBuffer();
            TKey[] keys = copy.Keys.ToArray();

            if (_options.MaxBatchSize > 0 &&
                copy.Count > _options.MaxBatchSize)
            {
                int count = (int)Math.Ceiling(
                    (decimal)copy.Count / _options.MaxBatchSize);

                for (int i = 0; i < count; i++)
                {
                    TKey[] keysBatch = keys
                        .Skip(i * _options.MaxBatchSize)
                        .Take(_options.MaxBatchSize)
                        .ToArray();
                    Result<TValue>[] values = await _fetch(keysBatch)
                        .ConfigureAwait(false);

                    SetBatchResults(copy, keysBatch, values);
                }
            }
            else
            {
                Result<TValue>[] values = await _fetch(keys)
                    .ConfigureAwait(false);

                SetBatchResults(copy, keys, values);
            }
        }


        private void SetBatchResults(
            TaskCompletionBuffer<TKey, TValue> buffer,
            TKey[] keys,
            Result<TValue>[] values)
        {
            for (int i = 0; i < buffer.Count; i++)
            {
                buffer[keys[i]].SetResult(values[i]);
            }
        }

        private void StartAsyncBatchDispatching()
        {
            if (!_options.DisableBatching)
            {
                _stopBatching = new CancellationTokenSource();
                _batchDispatcher = Task.Run(async () =>
                {
                    while (!_stopBatching.IsCancellationRequested)
                    {
                        if (_options.BatchRequestDelay > TimeSpan.Zero ||
                            _buffer.Count == 0)
                        {
                            await Task.Delay(_options.BatchRequestDelay)
                                .ConfigureAwait(false);
                        }
                        else
                        {
                            await DispatchBatchAsync().ConfigureAwait(false);
                        }
                    }
                });
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
                    _stopBatching?.Cancel();
                    _batchDispatcher?.Dispose();
                    _cache?.Dispose();
                    _stopBatching?.Dispose();
                }

                _batchDispatcher = null;
                _buffer = null;
                _fetch = null;
                _cache = null;
                _options = null;
                _stopBatching = null;

                _disposed = true;
            }
        }

        #endregion
    }
}
