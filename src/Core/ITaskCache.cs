using System;
using System.Threading.Tasks;

namespace GreenDonut
{
    /// <summary>
    /// A cache which stores <see cref="Task{Result{TValue}}"/>.
    /// </summary>
    /// <typeparam name="TKey">A key type</typeparam>
    /// <typeparam name="TValue">A value type</typeparam>
    public interface ITaskCache<TKey, TValue>
    {
        /// <summary>
        /// Gets the maximum size of the cache.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets the sliding expiration for the cache entries.
        /// </summary>
        TimeSpan SlidingExpirartion { get; }

        /// <summary>
        /// Gets the count of the entries inside the cache.
        /// </summary>
        int Usage { get; }

        /// <summary>
        /// Adds a single entry to the cache if it is not already there.
        /// </summary>
        /// <param name="key">A cache entry key.</param>
        /// <param name="value">A cache entry value.</param>
        void Add(TKey key, Task<Result<TValue>> value);

        /// <summary>
        /// Clears the complete cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets a single entry from the cache.
        /// </summary>
        /// <param name="key">A cache entry key.</param>
        /// <returns>A single cache entry value.</returns>
        Task<Result<TValue>> Get(TKey key);

        /// <summary>
        /// Removes a specific entry from the cache.
        /// </summary>
        /// <param name="key">A cache entry key.</param>
        void Remove(TKey key);
    }
}
