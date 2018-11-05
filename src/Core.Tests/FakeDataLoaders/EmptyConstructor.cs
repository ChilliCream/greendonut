using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GreenDonut.FakeDataLoaders
{
    internal class EmptyConstructor
        : DataLoaderBase<string, string>
    {
        internal EmptyConstructor()
            : base()
        { }

        protected override Task<IReadOnlyList<IResult<string>>> Fetch(
            IReadOnlyList<string> keys)
        {
            throw new NotImplementedException();
        }
    }
}
