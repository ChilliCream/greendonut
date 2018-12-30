using System;
using System.Threading.Tasks;

namespace GreenDonut
{
    /// <summary>
    /// A bunch of convenient <see cref="IDataLoader{TKey, TValue}"/> extension
    /// methods.
    /// </summary>
    public static class DataLoaderExtensions
    {
        /// <summary>
        /// Adds a new entry to the cache if not already exists.
        /// </summary>
        /// <param name="dataLoader">A data loader instance.</param>
        /// <param name="key">A cache entry key.</param>
        /// <param name="value">A cache entry value.</param>
        /// <exception cref="dataLoader">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        /// <exception cref="key">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        public static void Set(
            this IDataLoader dataLoader,
            object key,
            object value)
        {
            if (dataLoader == null)
            {
                throw new ArgumentNullException(nameof(dataLoader));
            }

            dataLoader.Set(key, Task.FromResult(value));
        }

        /// <summary>
        /// Adds a new entry to the cache if not already exists.
        /// </summary>
        /// <typeparam name="TKey">A key type.</typeparam>
        /// <typeparam name="TValue">A value type.</typeparam>
        /// <param name="dataLoader">A data loader instance.</param>
        /// <param name="key">A cache entry key.</param>
        /// <param name="value">A cache entry value.</param>
        /// <exception cref="dataLoader">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        /// <exception cref="key">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        public static void Set<TKey, TValue>(
            this IDataLoader<TKey, TValue> dataLoader,
            TKey key,
            TValue value)
        {
            if (dataLoader == null)
            {
                throw new ArgumentNullException(nameof(dataLoader));
            }

            dataLoader.Set(key, Task.FromResult(value));
        }
    }
}
