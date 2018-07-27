using System.Collections.Generic;
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
    /// </summary>
    /// <typeparam name="TKey">A key type</typeparam>
    /// <typeparam name="TValue">A value type</typeparam>
    public interface IDataLoader<TKey, TValue>
    {
        /// <summary>
        /// Empties the complete cache.
        /// </summary>
        /// <returns>Itself for chaining support.</returns>
        IDataLoader<TKey, TValue> Clear();

        /// <summary>
        /// Loads a single value by key. This call may return a cached value
        /// or enqueues this single request for bacthing if enabled.
        /// </summary>
        /// <param name="key">A unique key.</param>
        /// <returns>
        /// A single result which may contain a value or information about the
        /// error which may occurred during the call.
        /// </returns>
        Task<TValue> LoadAsync(TKey key);

        /// <summary>
        /// Loads multiple values by keys. This call may return cached values
        /// and enqueues requests which were not cached for bacthing if
        /// enabled.
        /// </summary>
        /// <param name="keys">A list of unique keys.</param>
        /// <returns>
        /// A list of values in the same order as the provided keys.
        /// </returns>
        Task<IReadOnlyList<TValue>> LoadAsync(params TKey[] keys);

        /// <summary>
        /// Loads multiple values by keys. This call may return cached values
        /// and enqueues requests which were not cached for bacthing if
        /// enabled.
        /// </summary>
        /// <param name="keys">A list of unique keys.</param>
        /// <returns>
        /// A list of values in the same order as the provided keys.
        /// </returns>
        Task<IReadOnlyList<TValue>> LoadAsync(IReadOnlyCollection<TKey> keys);

        /// <summary>
        /// Removes a single entry from the cache.
        /// </summary>
        /// <param name="key">A cache entry key.</param>
        /// <returns>Itself for chaining support.</returns>
        IDataLoader<TKey, TValue> Remove(TKey key);

        /// <summary>
        /// Adds a new entry to the cache if not already exists.
        /// </summary>
        /// <param name="key">A cache entry key.</param>
        /// <param name="value">A cache entry value.</param>
        /// <returns>Itself for chaining support.</returns>
        IDataLoader<TKey, TValue> Set(TKey key, Task<TValue> value);
    }
}
