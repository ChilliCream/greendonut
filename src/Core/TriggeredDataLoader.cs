using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenDonut
{
    /// <summary>
    /// A <see cref="TriggeredDataLoader{TKey, TValue}"/> creates a public API
    /// for loading data from a particular data back-end with unique keys such
    /// as the `id` column of a SQL table or document name in a MongoDB
    /// database, given a batch loading function. -- facebook
    ///
    /// Each <c>DataLoader</c> instance contains a unique memoized cache. Use
    /// caution when used in long-lived applications or those which serve many
    /// users with different access permissions and consider creating a new
    /// instance per web request. -- facebook
    ///
    /// This <see cref="TriggeredDataLoader{TKey, TValue}"/> in specific must
    /// be triggered manually in order to fire batch requests.
    /// </summary>
    /// <typeparam name="TKey">A key type</typeparam>
    /// <typeparam name="TValue">A value type</typeparam>
    public class TriggeredDataLoader<TKey, TValue>
        : DataLoaderBase<TKey, TValue>
    {
        private readonly FetchDataDelegate<TKey, TValue> _fetch;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TriggeredDataLoader{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="fetch">
        /// A delegate to fetch data batches which will be invoked every time
        /// when trying to setup a new batch request.
        /// </param>
        /// <param name="options">
        /// An options object to configure the behavior of this particular
        /// <see cref="TriggeredDataLoader{TKey, TValue}"/>.
        /// </param>
        public TriggeredDataLoader(
            FetchDataDelegate<TKey, TValue> fetch,
            DataLoaderOptions<TKey> options)
                : base(options)
        {
            _fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));
        }

        /// <summary>
        /// Dispatches one or more batch requests.
        /// </summary>
        /// <returns></returns>
        public Task DispatchAsync()
        {
            return DispatchBatchAsync();
        }

        /// <inheritdoc />
        protected override Task<IReadOnlyList<Result<TValue>>> Fetch(
            IReadOnlyList<TKey> keys)
        {
            return _fetch(keys);
        }
    }
}
