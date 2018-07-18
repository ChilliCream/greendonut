using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenDonut
{
    public delegate Task<IReadOnlyList<Result<TValue>>> FetchDataDelegate
        <TKey, TValue>(IReadOnlyList<TKey> keys);
}
