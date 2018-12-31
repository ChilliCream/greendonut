using System;
using System.Threading.Tasks;
using GreenDonut.FakeDataLoaders;
using Xunit;

namespace GreenDonut
{
    public class DataLoaderBaseTests
    {
        #region Constructor()

        [Fact(DisplayName = "Constructor: Should not throw any exception")]
        public void ConstructorA()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);

            // act
            Action verify = () => new EmptyConstructor();

            // assert
            Assert.Null(Record.Exception(verify));
        }

        #endregion

        #region Constructor(cache)

        [Fact(DisplayName = "Constructor: Should throw an argument null exception for cache")]
        public void ConstructorBCacheNull()
        {
            // arrange
            TaskCache<string> cache = null;

            // act
            Action verify = () => new CacheConstructor(cache);

            // assert
            Assert.Throws<ArgumentNullException>("cache", verify);
        }

        [Fact(DisplayName = "Constructor: Should not throw any exception")]
        public void ConstructorBNoException()
        {
            // arrange
            var cache = new TaskCache<string>(10, TimeSpan.Zero);

            // act
            Action verify = () => new CacheConstructor(cache);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        #endregion
    }
}
