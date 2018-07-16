using System;

namespace GreenDonut
{
    /// <summary>
    /// Represents <see cref="DataLoader"/> options.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class DataLoaderOptions<TKey, TValue>
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataLoaderOptions{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="cacheKeyResolver"></param>
        public DataLoaderOptions(Func<TValue, TKey> cacheKeyResolver)
        {
            BatchRequestDelay = TimeSpan.Zero;
            CacheKeyResolver = cacheKeyResolver ??
                throw new ArgumentNullException(nameof(cacheKeyResolver));
            SlidingExpiration = TimeSpan.Zero;
        }

        /// <summary>
        /// Gets or sets the time period to wait if the
        /// <see cref="TaskCompletionBuffer{TKey, TValue}"/> is empty before
        /// trying to setup another batch request. This property takes only
        /// effect if <see cref="DisableBatching"/> is set to <c>false</c>.
        /// The default is <see cref="TimeSpan.Zero"/> which means no delay at
        /// all.
        /// </summary>
        public TimeSpan BatchRequestDelay { get; set; }

        public Func<TValue, TKey> CacheKeyResolver { get; private set; }

        public bool DisableBatching { get; set; }

        public bool DisableCaching { get; set; }

        public int MaxBatchSize { get; set; }

        public TimeSpan SlidingExpiration { get; set; }
    }
}
