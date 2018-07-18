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
        IDataLoader<TKey, TValue> Clear();

        Task<Result<TValue>> LoadAsync(TKey key);

        Task<IReadOnlyCollection<Result<TValue>>> LoadAsync(
            IReadOnlyCollection<TKey> keys);

        IDataLoader<TKey, TValue> Remove(TKey key);

        IDataLoader<TKey, TValue> Set(TKey key, Task<Result<TValue>> value);
    }
}
