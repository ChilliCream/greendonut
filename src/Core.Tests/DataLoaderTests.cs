using System;
using System.Threading.Tasks;
using Xunit;

namespace GreenDonut
{
    public class DataLoaderTests
    {
        #region Constructor

        [Fact(DisplayName = "Constructor: Should throw an argument null exception for fetch")]
        public void ConstructorFetchNull()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = null;
            var options = new DataLoaderOptions<string>();

            // act
            Action verify = () => new DataLoader<string, string>(options,
                fetch);

            // assert
            Assert.Throws<ArgumentNullException>("fetch", verify);
        }

        [Fact(DisplayName = "Constructor: Should throw an argument null exception for options")]
        public void ConstructorOptionsNull()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            DataLoaderOptions<string> options = null;

            // act
            Action verify = () => new DataLoader<string, string>(options,
                fetch);

            // assert
            Assert.Throws<ArgumentNullException>("options", verify);
        }

        [Fact(DisplayName = "Constructor: Should not throw any exception")]
        public void ConstructorNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();

            // act
            Action verify = () => new DataLoader<string, string>(options,
                fetch);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        #endregion

        #region Remove

        [Fact(DisplayName = "Remove: Should throw an argument null exception for key")]
        public void RemoveKeyNull()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            string key = null;

            loader.Set("Foo", Task.FromResult(Result<string>.Resolve("Foo")));

            // act
            Action verify = () => loader.Remove(key);

            // assert
            Assert.Throws<ArgumentNullException>("key", verify);
        }

        [Fact(DisplayName = "Remove: Should not throw any exception")]
        public void RemoveNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            var key = "Foo";

            // act
            Action verify = () => loader.Remove(key);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Remove: Should remove an existing entry")]
        public async Task RemoveEntry()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            var key = "Foo";

            loader.Set(key, Task.FromResult(Result<string>.Resolve("Bar")));

            // act
            IDataLoader<string, string> result = loader.Remove(key);

            // assert
            Result<string> loadResult = await loader.LoadAsync(key)
                .ConfigureAwait(false);

            Assert.Equal(loader, result);
            Assert.NotNull(loadResult);
        }

        #endregion

        #region Set

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
            var value = Task.FromResult(Result<string>.Resolve("Foo"));

            // act
            Action verify = () => loader.Set(key, value);

            // assert
            Assert.Throws<ArgumentNullException>("key", verify);
        }

        [Fact(DisplayName = "Set: Should throw an argument null exception for value")]
        public void SetValueNull()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            var key = "Foo";
            Task<Result<string>> value = null;

            // act
            Action verify = () => loader.Set(key, value);

            // assert
            Assert.Throws<ArgumentNullException>("value", verify);
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
            var value = Task.FromResult(Result<string>.Resolve("Bar"));

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
            var value = Task.FromResult(Result<string>.Resolve("Bar"));

            // act
            IDataLoader<string, string> result = loader.Set(key, value);

            // assert
            Result<string> loadResult = await loader.LoadAsync(key)
                .ConfigureAwait(false);

            Assert.Equal(loader, result);
            Assert.NotNull(loadResult);
            Assert.Equal(loadResult.Value, value.Result.Value);
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
            var first = Task.FromResult(Result<string>.Resolve("Bar"));
            var second = Task.FromResult(Result<string>.Resolve("Baz"));

            // act
            loader.Set(key, first);
            loader.Set(key, second);

            // assert
            Result<string> loadResult = await loader.LoadAsync(key)
                .ConfigureAwait(false);

            Assert.NotNull(loadResult);
            Assert.Equal(loadResult.Value, first.Result.Value);
        }

        #endregion
    }
}
