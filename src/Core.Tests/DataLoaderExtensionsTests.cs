using System;
using System.Threading.Tasks;
using Xunit;

namespace GreenDonut
{
    public class DataLoaderExtensionsTests
    {
        #region Set

        [Fact(DisplayName = "Set: Should throw an argument null exception for dataLoader")]
        public void SetDataLoaderNull()
        {
            // arrange
            IDataLoader<string, string> loader = null;
            string key = "Foo";
            var value = "Bar";

            // act
            Action verify = () => loader.Set(key, value);

            // assert
            Assert.Throws<ArgumentNullException>("dataLoader", verify);
        }

        [Fact(DisplayName = "Set: Should throw an argument null exception for key")]
        public void SetKeyNull()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            string key = null;
            var value = "Bar";

            // act
            Action verify = () => loader.Set(key, value);

            // assert
            Assert.Throws<ArgumentNullException>("key", verify);
        }

        [Fact(DisplayName = "Set: Should not throw any exception")]
        public void SetNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            var key = "Foo";
            var value = "Bar";

            // act
            Action verify = () => loader.Set(key, value);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Set: Should result in a new cache entry")]
        public async Task SetNewCacheEntry()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            var key = "Foo";
            var value = "Bar";

            // act
            IDataLoader<string, string> result = loader.Set(key, value);

            // assert
            var loadResult = await loader.LoadAsync(key).ConfigureAwait(false);

            Assert.Equal(loader, result);
            Assert.Equal(value, loadResult);
        }

        [Fact(DisplayName = "Set: Should result in 'Bar'")]
        public async Task SetTwice()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            var key = "Foo";
            var first = "Bar";
            var second = "Baz";

            // act
            loader.Set(key, first);
            loader.Set(key, second);

            // assert
            var loadResult = await loader.LoadAsync(key).ConfigureAwait(false);
            
            Assert.Equal(first, loadResult);
        }

        #endregion
    }
}
