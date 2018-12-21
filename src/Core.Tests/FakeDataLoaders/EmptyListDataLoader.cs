using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenDonut.FakeDataLoaders
{
    internal class EmptyListDataLoader
        : DataLoaderBase<string, string>
    {
        protected override async Task<IReadOnlyList<IResult<string>>> Fetch(IReadOnlyList<string> keys)
        {
            return await Task.FromResult(keys.Select(k => Result<string>.Resolve(k)).ToList());
        }
    }
}