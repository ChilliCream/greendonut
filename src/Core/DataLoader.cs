using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenDonut
{
    /// <summary>
    /// A <see cref="DataLoader{TKey, TValue}"/> creates a public API for
    /// loading data from a particular data back-end with unique keys such as
    /// the `id` column of a SQL table or document name in a MongoDB database,
    /// given a batch loading function. -- facebook
    ///
    /// Each `DataLoader` instance contains a unique memoized cache.Use caution
    /// when used in long-lived applications or those which serve many users
    /// with different access permissions and consider creating a new instance
    /// per web request. -- facebook
    ///
    /// This <see cref="DataLoader{TKey, TValue}"/> in specific runs a single
    /// background thread which one by one fires batch requests if the buffer
    /// is filled.
    /// </summary>
    /// <typeparam name="TKey">A key type</typeparam>
    /// <typeparam name="TValue">A value type</typeparam>
    public class DataLoader<TKey, TValue>
        : DataLoaderBase<TKey, TValue>
    {
        private FetchDataDelegate<TKey, TValue> _fetch;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="DataLoader{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="fetch">
        /// A delegate to fetch data batches which will be invoked every time
        /// when trying to setup a new batch request.
        /// </param>
        /// <param name="options">
        /// An options object to configure the behavior of this particular
        /// <see cref="DataLoader{TKey, TValue}"/>.
        /// </param>
        public DataLoader(
            FetchDataDelegate<TKey, TValue> fetch,
            DataLoaderOptions<TKey> options)
                : base(options)
        {
            _fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));

            StartAsyncBatchDispatching();
        }

        /// <inheritdoc />
        protected override Task<IReadOnlyList<Result<TValue>>> Fetch(
            IReadOnlyList<TKey> keys)
        {
            DispatchBatchAsync();

            return _fetch(keys);
        }
    }
}
