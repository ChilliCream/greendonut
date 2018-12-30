using System;
using System.Threading.Tasks;

namespace GreenDonut
{
    /// <summary>
    /// A cache which stores <see cref="Task{TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">A key type.</typeparam>
    /// <typeparam name="TValue">A value type.</typeparam>
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
        /// Clears the complete cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Removes a specific entry from the cache.
        /// </summary>
        /// <param name="key">A cache entry key.</param>
        /// <exception cref="key">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        void Remove(TKey key);

        /// <summary>
        /// Tries to add a single entry to the cache. It does nothing if the
        /// cache entry exists already.
        /// </summary>
        /// <param name="key">A cache entry key.</param>
        /// <param name="value">A cache entry value.</param>
        /// <exception cref="key">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        /// <exception cref="value">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        /// <returns>
        /// A value indicating whether the add was successful.
        /// </returns>
        bool TryAdd(TKey key, Task<TValue> value);

        /// <summary>
        /// Tries to gets a single entry from the cache.
        /// </summary>
        /// <param name="key">A cache entry key.</param>
        /// <param name="value">A single cache entry value.</param>
        /// <exception cref="key">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        /// <returns>
        /// A value indicating whether the get request returned a entry.
        /// </returns>
        bool TryGetValue(TKey key, out Task<TValue> value);
    }
}
