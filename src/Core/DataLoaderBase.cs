using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GreenDonut
{
    /// <summary>
    /// A <c>DataLoader</c> creates a public API for loading data from a
    /// particular data back-end with unique keys such as the `id` column of a
    /// SQL table or document name in a MongoDB database, given a batch loading
    /// function. -- facebook
    ///
    /// Each <c>DataLoader</c> instance contains a unique memoized cache. Use
    /// caution when used in long-lived applications or those which serve many
    /// users with different access permissions and consider creating a new
    /// instance per web request. -- facebook
    ///
    /// This is an abstraction for all kind of <c>DataLoaders</c>.
    /// </summary>
    /// <typeparam name="TKey">A key type</typeparam>
    /// <typeparam name="TValue">A value type</typeparam>
    public abstract class DataLoaderBase<TKey, TValue>
        : IDataLoader<TKey, TValue>
        , IDisposable
    {
        private readonly object _sync = new object(); 
        private bool _disposed;
        private Task _batchDispatcher;
        private TaskCompletionBuffer<TKey, TValue> _buffer;
        private ITaskCache<TKey, TValue> _cache;
        private readonly Func<TKey, TKey> _cacheKeyResolver;
        private DataLoaderOptions<TKey> _options;
        private CancellationTokenSource _stopBatching;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataLoaderBase{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="options">
        /// A configuration for <c>DataLoaders</c>.
        /// </param>
        protected DataLoaderBase(DataLoaderOptions<TKey> options)
        {
            _options = options ??
                throw new ArgumentNullException(nameof(options));
            _buffer = new TaskCompletionBuffer<TKey, TValue>();
            _cache = new TaskCache<TKey, TValue>(
                _options.CacheSize,
                _options.SlidingExpiration);
            _cacheKeyResolver = _options.CacheKeyResolver ??
                ((TKey key) => key);
;

            StartAsyncBackgroundDispatching();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataLoaderBase{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="options">
        /// A configuration for <c>DataLoaders</c>.
        /// </param>
        /// <param name="cache">
        /// A cache instance for <c>Tasks</c>.
        /// </param>
        protected DataLoaderBase(DataLoaderOptions<TKey> options,
            ITaskCache<TKey, TValue> cache)
        {
            _options = options ??
                throw new ArgumentNullException(nameof(options));
            _buffer = new TaskCompletionBuffer<TKey, TValue>();
            _cache = cache ??
                throw new ArgumentNullException(nameof(cache));
            _cacheKeyResolver = _options.CacheKeyResolver ??
                ((TKey key) => key);
;

            StartAsyncBackgroundDispatching();
        }

        /// <inheritdoc />
        public IDataLoader<TKey, TValue> Clear()
        {
            _cache.Clear();

            return this;
        }

        /// <summary>
        /// Dispatches one or more batch requests.
        /// </summary>
        public async Task DispatchAsync()
        {
            if (!_options.AutoDispatching && _options.Batching)
            {
                await DispatchBatchAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets a delegate used for data fetching. The results will be stored
        /// in a memory store to decrease round-trips to the server and improve
        /// overall performance.
        /// </summary>
        protected abstract Task<IReadOnlyList<Result<TValue>>> Fetch(
            IReadOnlyList<TKey> keys);

        /// <inheritdoc />
        public Task<Result<TValue>> LoadAsync(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            TKey resolvedKey = _cacheKeyResolver(key);

            if (_options.Caching)
            {
                Task<Result<TValue>> cachedValue = _cache.Get(resolvedKey);

                if (cachedValue != null)
                {
                    return cachedValue;
                }
            }

            var promise = new TaskCompletionSource<Result<TValue>>();

            if (_options.Batching)
            {
                _buffer.TryAdd(resolvedKey, promise);
            }
            else
            {
                // note: must run in the background; do not await here.
                Task.Run(() => DispatchAsync(resolvedKey, promise));
            }

            if (_options.Caching)
            {
                _cache.Add(resolvedKey, promise.Task);
            }

            return promise.Task;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Result<TValue>>> LoadAsync(
            params TKey[] keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Length < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(keys),
                    "There must be at least one key");
            }

            return await Task.WhenAll(keys.Select(LoadAsync))
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Result<TValue>>> LoadAsync(
            IEnumerable<TKey> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Count() < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(keys),
                    "There must be at least one key");
            }

            return await Task.WhenAll(keys.Select(LoadAsync))
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IDataLoader<TKey, TValue> Remove(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            TKey resolvedKey = _cacheKeyResolver(key);

            _cache.Remove(resolvedKey);

            return this;
        }

        /// <inheritdoc />
        public IDataLoader<TKey, TValue> Set(
            TKey key,
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

            TKey resolvedKey = _cacheKeyResolver(key);

            if (_cache.Get(resolvedKey) == null)
            {
                _cache.Add(resolvedKey, value);
            }

            return this;
        }

        private TaskCompletionBuffer<TKey, TValue> CopyAndClearBuffer()
        {
            TaskCompletionBuffer<TKey, TValue> copy = _buffer;

            _buffer = new TaskCompletionBuffer<TKey, TValue>();

            return copy;
        }

        private async Task DispatchAsync(
            TKey resolvedKey,
            TaskCompletionSource<Result<TValue>> promise)
        {
            var keys = new TKey[] { resolvedKey };
            IReadOnlyCollection<Result<TValue>> values = await Fetch(keys)
                .ConfigureAwait(false);

            if (values.Count == 1)
            {
                promise.SetResult(values.First());
            }
            else
            {
                var error = "Fetch should have returned exactly one result " +
                    $"but instead returned \"{values.Count}\" results.";

                promise.SetResult(Result<TValue>.Reject(error));
            }
        }

        private Task DispatchBatchAsync()
        {
            return _sync.LockAsync(
                () => !_buffer.IsEmpty,
                async () =>
                {
                    TaskCompletionBuffer<TKey, TValue> copy =
                        CopyAndClearBuffer();
                    TKey[] resolvedKeys = copy.Keys.ToArray();

                    if (_options.MaxBatchSize > 0 &&
                        copy.Count > _options.MaxBatchSize)
                    {
                        var count = (int)Math.Ceiling(
                            (decimal)copy.Count / _options.MaxBatchSize);

                        for (var i = 0; i < count; i++)
                        {
                            TKey[] keysBatch = resolvedKeys
                                .Skip(i * _options.MaxBatchSize)
                                .Take(_options.MaxBatchSize)
                                .ToArray();
                            IReadOnlyList<Result<TValue>> values =
                                await Fetch(keysBatch).ConfigureAwait(false);

                            SetBatchResults(copy, keysBatch, values);
                        }
                    }
                    else
                    {
                        IReadOnlyList<Result<TValue>> values =
                            await Fetch(resolvedKeys).ConfigureAwait(false);

                        SetBatchResults(copy, resolvedKeys, values);
                    }
                });
        }

        private void SetBatchResults(
            TaskCompletionBuffer<TKey, TValue> buffer,
            IReadOnlyList<TKey> keys,
            IReadOnlyList<Result<TValue>> values)
        {
            if (keys.Count == values.Count)
            {
                for (var i = 0; i < buffer.Count; i++)
                {
                    buffer[keys[i]].SetResult(values[i]);
                }
            }
            else
            {
                var error = "Fetch should have returned exactly " +
                    $"\"{keys.Count}\" result(s) but instead returned " +
                    $"\"{values.Count}\" result(s).";
                var result = Result<TValue>.Reject(error);

                for (var i = 0; i < buffer.Count; i++)
                {
                    buffer[keys[i]].SetResult(result);
                }
            }
        }

        private void StartAsyncBackgroundDispatching()
        {
            if (_options.AutoDispatching && _options.Batching)
            {
                _sync.Lock(
                    () => _batchDispatcher == null,
                    () =>
                    {
                        _stopBatching = new CancellationTokenSource();
                        _batchDispatcher = Task.Run(async () =>
                        {
                            while (!_stopBatching.IsCancellationRequested)
                            {
                                if (_buffer.Count == 0 &&
                                    _options.BatchRequestDelay > TimeSpan.Zero)
                                {
                                    await Task.Delay(
                                        _options.BatchRequestDelay)
                                            .ConfigureAwait(false);
                                }
                                else
                                {
                                    await DispatchBatchAsync()
                                        .ConfigureAwait(false);
                                }
                            }
                        });
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
                    (_cache as IDisposable)?.Dispose();
                    _stopBatching?.Dispose();
                }

                _batchDispatcher = null;
                _buffer = null;
                _cache = null;
                _options = null;
                _stopBatching = null;

                _disposed = true;
            }
        }

        #endregion
    }
}
