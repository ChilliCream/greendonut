using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenDonut
{
    /// <summary>
    /// A delegate for data fetching.
    /// </summary>
    /// <typeparam name="TKey">A key type.</typeparam>
    /// <typeparam name="TValue">A value type.</typeparam>
    /// <param name="keys">A list of keys.</param>
    /// <returns>
    /// A list of values which must be in the same order as the provided keys.
    /// </returns>
    public delegate Task<IReadOnlyList<IResult<TValue>>> FetchDataDelegate
        <TKey, TValue>(IReadOnlyList<TKey> keys);
}
