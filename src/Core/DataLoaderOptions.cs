using System;

namespace GreenDonut
{
    /// <summary>
    /// An options object to configure the behavior for data loaders.
    /// </summary>
    /// <typeparam name="TKey">A key type</typeparam>
    public class DataLoaderOptions<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataLoaderOptions{TKey}"/> class.
        /// </summary>
        /// <param name="cacheKeyResolver"></param>
        public DataLoaderOptions()
        {
            Batching = true;
            BatchRequestDelay = TimeSpan.Zero;
            CacheSize = 1000;
            Caching = true;
            SlidingExpiration = TimeSpan.Zero;
        }

        public bool AutoDispatching { get; set; }

        public bool Batching { get; set; }

        /// <summary>
        /// Gets or sets the time period to wait if the
        /// <see cref="TaskCompletionBuffer{TKey, TValue}"/> is empty before
        /// trying to setup another batch request. This property takes only
        /// effect if <see cref="Batching"/> is set to <c>true</c>. The default
        /// is <see cref="TimeSpan.Zero"/> which means no delay at all.
        /// </summary>
        public TimeSpan BatchRequestDelay { get; set; }

        public Func<TKey, TKey> CacheKeyResolver { get; set; }

        public int CacheSize { get; set; }

        public bool Caching { get; set; }

        public int MaxBatchSize { get; set; }

        public TimeSpan SlidingExpiration { get; set; }
    }
}
