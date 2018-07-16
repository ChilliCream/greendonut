using System.Threading.Tasks;

namespace GreenDonut
{
    public delegate Task<Result<TValue>[]> FetchDataDelegate<TKey,
        TValue>(TKey[] keys);
}
