using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenDonut.FakeDataLoaders
{
    internal class CacheConstructor
        : DataLoaderBase<string, string>
    {
        internal CacheConstructor(TaskCache<string, string> cache)
            : base(cache)
        { }

        protected override Task<IReadOnlyList<Result<string>>> Fetch(
            IReadOnlyList<string> keys)
        {
            throw new NotImplementedException();
        }
    }
}
