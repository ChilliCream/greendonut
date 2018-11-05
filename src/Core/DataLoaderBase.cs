using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Each <c>DataLoader</c> instance contains a unique memorized cache. Use
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
        , IDispatchableDataLoader
        , IDisposable
    {
        private readonly object _sync = new object();
        private bool _disposed;
        private TaskCompletionBuffer<TKey, TValue> _buffer;
        private ITaskCache<TKey, TValue> _cache;
        private readonly Func<TKey, TKey> _cacheKeyResolver;
        private AutoResetEvent _delaySignal;
        private DataLoaderOptions<TKey> _options;
        private CancellationTokenSource _stopBatching;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataLoaderBase{TKey, TValue}"/> class.
        /// </summary>
        protected DataLoaderBase()
            : this(new DataLoaderOptions<TKey>())
        { }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataLoaderBase{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="cache">
        /// A cache instance for <c>Tasks</c>.
        /// </param>
        /// <exception cref="cache">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        protected DataLoaderBase(ITaskCache<TKey, TValue> cache)
            : this(new DataLoaderOptions<TKey>(), cache)
        { }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataLoaderBase{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="options">
        /// A configuration for <c>DataLoaders</c>.
        /// </param>
        /// <exception cref="options">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        protected DataLoaderBase(DataLoaderOptions<TKey> options)
            : this(options, new TaskCache<TKey, TValue>(
                options?.CacheSize ?? Defaults.CacheSize,
                options?.SlidingExpiration ??
                    Defaults.SlidingExpiration))
        { }

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
        /// <exception cref="options">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        /// <exception cref="cache">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
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

            StartAsyncBackgroundDispatching();
        }

        /// <inheritdoc />
        public IDataLoader<TKey, TValue> Clear()
        {
            _cache.Clear();

            return this;
        }

        /// <inheritdoc />
        public async Task DispatchAsync()
        {
            if (_options.Batching)
            {
                if (_options.AutoDispatching)
                {
                    // this line is for interrupting the delay to wait before
                    // auto dispatching sends the next batch. this is like an
                    // implicit dispatch.
                    _delaySignal.Set();
                }
                else
                {
                    // this is the way for doing a explicit dispatch when auto
                    // dispatching is disabled.
                    await DispatchBatchAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Gets a delegate used for data fetching. The results will be stored
        /// in a memory store to decrease round-trips to the server and improve
        /// overall performance.
        /// </summary>
        protected abstract Task<IReadOnlyList<IResult<TValue>>> Fetch(
            IReadOnlyList<TKey> keys);

        /// <inheritdoc />
        public Task<TValue> LoadAsync(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (_sync)
            {
                TKey resolvedKey = _cacheKeyResolver(key);

                if (_options.Caching && _cache.TryGetValue(resolvedKey,
                    out Task<TValue> cachedValue))
                {
                    DispatchingDiagnostics.RecordCachedValue(resolvedKey,
                        cachedValue);

                    return cachedValue;
                }

                var promise = new TaskCompletionSource<TValue>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

                if (_options.Batching)
                {
                    if (!_buffer.TryAdd(resolvedKey, promise) &&
                        _buffer.TryGetValue(resolvedKey,
                            out TaskCompletionSource<TValue> value))
                    {
                        promise.TrySetCanceled();
                        promise = value;
                    }
                }
                else
                {
                    // note: must run in the background; do not await here.
                    Task.Factory.StartNew(
                        () => DispatchAsync(resolvedKey, promise),
                        TaskCreationOptions.RunContinuationsAsynchronously);
                }

                if (_options.Caching)
                {
                    _cache.TryAdd(resolvedKey, promise.Task);
                }

                return promise.Task;
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<TValue>> LoadAsync(params TKey[] keys)
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

            return LoadInternalAsync(keys);
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<TValue>> LoadAsync(
            IReadOnlyCollection<TKey> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            if (keys.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(keys),
                    "There must be at least one key");
            }

            return LoadInternalAsync(keys);
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
        public IDataLoader<TKey, TValue> Set(TKey key, Task<TValue> value)
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

            _cache.TryAdd(resolvedKey, value);

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
            TaskCompletionSource<TValue> promise)
        {
            var resolvedKeys = new TKey[] { resolvedKey };
            Activity activity = DispatchingDiagnostics
                .StartBatching(resolvedKeys);
            IReadOnlyList<IResult<TValue>> results =
                await Fetch(resolvedKeys).ConfigureAwait(false);

            if (results.Count == 1)
            {
                SetSingleResult(promise, resolvedKey, results.First());
            }
            else
            {
                Exception error = Errors.CreateKeysAndValuesMustMatch(1,
                    results.Count);

                DispatchingDiagnostics.RecordError(resolvedKey, error);
                promise.SetException(error);
            }

            DispatchingDiagnostics.StopBatching(activity, resolvedKeys,
                results);
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
                        var chunkSize = (int)Math.Ceiling(
                            (decimal)copy.Count / _options.MaxBatchSize);

                        for (var i = 0; i < chunkSize; i++)
                        {
                            TKey[] chunkedKeys = resolvedKeys
                                .Skip(i * _options.MaxBatchSize)
                                .Take(_options.MaxBatchSize)
                                .ToArray();

                            await FetchInternalAsync(copy, chunkedKeys)
                                .ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await FetchInternalAsync(copy, resolvedKeys)
                            .ConfigureAwait(false);
                    }
                });
        }

        private async Task FetchInternalAsync(
            IDictionary<TKey, TaskCompletionSource<TValue>> bufferedPromises,
            IReadOnlyList<TKey> resolvedKeys)
        {
            Activity activity = DispatchingDiagnostics
                .StartBatching(resolvedKeys);
            IReadOnlyList<IResult<TValue>> results =
                await Fetch(resolvedKeys).ConfigureAwait(false);

            SetBatchResults(bufferedPromises, resolvedKeys, results);
            DispatchingDiagnostics.StopBatching(activity, resolvedKeys,
                results);
        }

        private async Task<IReadOnlyList<TValue>> LoadInternalAsync(
            TKey[] keys)
        {
            return await Task.WhenAll(keys.Select(LoadAsync))
                .ConfigureAwait(false);
        }

        private async Task<IReadOnlyList<TValue>> LoadInternalAsync(
            IReadOnlyCollection<TKey> keys)
        {
            var index = 0;
            var tasks = new Task<TValue>[keys.Count];

            foreach (TKey key in keys)
            {
                tasks[index++] = LoadAsync(key);
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private void SetBatchResults(
            IDictionary<TKey, TaskCompletionSource<TValue>> bufferedPromises,
            IReadOnlyList<TKey> resolvedKeys,
            IReadOnlyList<IResult<TValue>> results)
        {
            if (resolvedKeys.Count == results.Count)
            {
                for (var i = 0; i < resolvedKeys.Count; i++)
                {
                    SetSingleResult(bufferedPromises[resolvedKeys[i]],
                        resolvedKeys[i], results[i]);
                }
            }
            else
            {
                Exception error = Errors.CreateKeysAndValuesMustMatch(
                    resolvedKeys.Count, results.Count);

                for (var i = 0; i < resolvedKeys.Count; i++)
                {
                    DispatchingDiagnostics.RecordError(resolvedKeys[i], error);
                    bufferedPromises[resolvedKeys[i]].SetException(error);
                }
            }
        }

        private void SetSingleResult(
            TaskCompletionSource<TValue> promise,
            TKey resolvedKey,
            IResult<TValue> result)
        {
            if (result.IsError)
            {
                DispatchingDiagnostics.RecordError(resolvedKey, result.Error);
                promise.SetException(result.Error);
            }
            else
            {
                promise.SetResult(result.Value);
            }
        }

        private void StartAsyncBackgroundDispatching()
        {
            if (_options.AutoDispatching && _options.Batching)
            {
                // here we removed the lock because we take care that this
                // function is called once within the constructor.
                _delaySignal = new AutoResetEvent(true);
                _stopBatching = new CancellationTokenSource();
                Task.Factory.StartNew(async () =>
                {
                    while (!_stopBatching.IsCancellationRequested)
                    {
                        _delaySignal
                            .WaitOne(_options.BatchRequestDelay);
                        await DispatchBatchAsync()
                            .ConfigureAwait(false);
                    }
                }, TaskCreationOptions.LongRunning);
            }
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
                    _stopBatching?.Cancel();
                    _delaySignal?.Set();
                    (_cache as IDisposable)?.Dispose();
                    _stopBatching?.Dispose();
                    _delaySignal?.Dispose();
                }

                _buffer = null;
                _cache = null;
                _delaySignal = null;
                _options = null;
                _stopBatching = null;

                _disposed = true;
            }
        }

        #endregion
    }
}
