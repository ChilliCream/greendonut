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
        , IDispatchableDataLoader
        , IDisposable
    {
        private readonly object _sync = new object();
        private bool _disposed;
        private Task _batchDispatcher;
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
        protected DataLoaderBase(DataLoaderOptions<TKey> options)
            : this(options, new TaskCache<TKey, TValue>(
                options?.CacheSize ?? 1000,
                options?.SlidingExpiration ?? TimeSpan.Zero))
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
        protected abstract Task<IReadOnlyList<Result<TValue>>> Fetch(
            IReadOnlyList<TKey> keys);

        /// <inheritdoc />
        public Task<TValue> LoadAsync(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            TKey resolvedKey = _cacheKeyResolver(key);

            if (_options.Caching)
            {
                if (_cache.TryGetValue(resolvedKey,
                    out Task<TValue> cachedValue))
                {
                    return cachedValue;
                }
            }

            var promise = new TaskCompletionSource<TValue>();

            if (_options.Batching)
            {
                _buffer.TryAdd(resolvedKey, promise);
            }
            else
            {
                // note: must run in the background; do not await here.
                Task.Factory.StartNew(() => DispatchAsync(resolvedKey, promise),
                    TaskCreationOptions.DenyChildAttach);
            }

            if (_options.Caching)
            {
                _cache.TryAdd(resolvedKey, promise.Task);
            }

            return promise.Task;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TValue>> LoadAsync(
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
        public async Task<IReadOnlyList<TValue>> LoadAsync(
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

            var tasks = new Task<TValue>[keys.Count];
            var index = 0;

            foreach (TKey key in keys)
            {
                tasks[index++] = LoadAsync(key);
            }

            for (var i = 0; i < tasks.Length; i++)
            {
                if (tasks[i] is IAsyncResult result)
                {
                    result.AsyncWaitHandle.WaitOne();
                }
            }

            return await Task.WhenAll(tasks).ConfigureAwait(false);
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
            Task<TValue> value)
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
            var keys = new TKey[] { resolvedKey };
            IReadOnlyCollection<Result<TValue>> results = await Fetch(keys)
                .ConfigureAwait(false);

            if (results.Count == 1)
            {
                SetSingleResult(promise, results.First());
            }
            else
            {
                promise.SetException(
                    Errors.CreateMustHaveOneResult(results.Count));
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
                        var chunkSize = (int)Math.Ceiling(
                            (decimal)copy.Count / _options.MaxBatchSize);

                        for (var i = 0; i < chunkSize; i++)
                        {
                            TKey[] chunkedKeys = resolvedKeys
                                .Skip(i * _options.MaxBatchSize)
                                .Take(_options.MaxBatchSize)
                                .ToArray();
                            IReadOnlyList<Result<TValue>> values =
                                await Fetch(chunkedKeys).ConfigureAwait(false);

                            SetBatchResults(copy, chunkedKeys, values);
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
            IReadOnlyList<Result<TValue>> results)
        {
            if (keys.Count == results.Count)
            {
                for (var i = 0; i < buffer.Count; i++)
                {
                    SetSingleResult(buffer[keys[i]], results[i]);
                }
            }
            else
            {
                Exception error = Errors.CreateEveryKeyMustHaveAValue(
                    keys.Count, results.Count);

                for (var i = 0; i < buffer.Count; i++)
                {
                    buffer[keys[i]].SetException(error);
                }
            }
        }

        private void SetSingleResult(
            TaskCompletionSource<TValue> promise,
            Result<TValue> result)
        {
            if (result.IsError)
            {
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
                _batchDispatcher = Task.Factory.StartNew(async () =>
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
                    _delaySignal?.Set();
                    // todo: fix "A task may only be disposed if it is in a
                    //       completion state (RanToCompletion, Faulted or
                    //       Canceled)."
                    //_batchDispatcher?.Dispose();
                    (_cache as IDisposable)?.Dispose();
                    _stopBatching?.Dispose();
                    _delaySignal?.Dispose();
                }

                _batchDispatcher = null;
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
