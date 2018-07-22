using System;
using System.Collections.Generic;
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

        #region Clear

        [Fact(DisplayName = "Clear: Should not throw any exception")]
        public void ClearNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);

            // act
            Action verify = () => loader.Clear();

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Clear: Should remove anll entries from the cache")]
        public async Task ClearAllEntries()
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

            loader.Set("Foo", Task.FromResult(Result<string>.Resolve("Bar")));
            loader.Set("Bar", Task.FromResult(Result<string>.Resolve("Baz")));

            // act
            IDataLoader<string, string> result = loader.Clear();

            // assert
            IReadOnlyList<Result<string>> loadResult = await loader
                .LoadAsync("Foo", "Bar")
                .ConfigureAwait(false);

            Assert.Equal(loader, result);
            Assert.Collection(loadResult,
                v => Assert.True(v.IsError),
                v => Assert.True(v.IsError));
        }

        #endregion

        #region LoadAsync(string key)

        [Fact(DisplayName = "LoadAsync: Should throw an argument null exception for key")]
        public async Task LoadSingleKeyNull()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            string key = null;

            // act
            Func<Task<Result<string>>> verify = () => loader.LoadAsync(key);

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>("key", verify)
                .ConfigureAwait(false);
        }

        [Fact(DisplayName = "LoadAsync: Should not throw any exception")]
        public async Task LoadSingleNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>()
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            var key = "Foo";

            // act
            Func<Task<Result<string>>> verify = () => loader.LoadAsync(key);

            // assert
            Assert.Null(await Record.ExceptionAsync(verify)
                .ConfigureAwait(false));
        }

        [Fact(DisplayName = "LoadAsync: Should return one result")]
        public async Task LoadSingleResult()
        {
            // arrange
            Result<string> expectedResult = Result<string>.Resolve("Bar");
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new[] { expectedResult })
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            var key = "Foo";

            // act
            Result<string> loadResult = await loader.LoadAsync(key)
                .ConfigureAwait(false);

            // assert
            Assert.NotNull(loadResult);
            Assert.Equal(expectedResult.Value, loadResult.Value);
        }

        #endregion

        #region LoadAsync(params string[] keys)

        [Fact(DisplayName = "LoadAsync: Should throw an argument null exception for keys")]
        public async Task LoadParamsKeysNull()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async k =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            string[] keys = null;

            // act
            Func<Task<IReadOnlyList<Result<string>>>> verify = () =>
                loader.LoadAsync(keys);

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>("keys", verify)
                .ConfigureAwait(false);
        }

        [Fact(DisplayName = "LoadAsync: Should throw an argument out of range exception for keys")]
        public async Task LoadParamsZeroKeys()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async k =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            string[] keys = new string[0];

            // act
            Func<Task<IReadOnlyList<Result<string>>>> verify = () =>
                loader.LoadAsync(keys);

            // assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>("keys",
                verify).ConfigureAwait(false);
        }

        [Fact(DisplayName = "LoadAsync: Should not throw any exception")]
        public async Task LoadParamsNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async k =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>()
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            var keys = new string[] { "Foo" };

            // act
            Func<Task<IReadOnlyList<Result<string>>>> verify = () =>
                loader.LoadAsync(keys);

            // assert
            Assert.Null(await Record.ExceptionAsync(verify)
                .ConfigureAwait(false));
        }

        [Fact(DisplayName = "LoadAsync: Should return one result")]
        public async Task LoadParamsResult()
        {
            // arrange
            Result<string> expectedResult = Result<string>.Resolve("Bar");
            FetchDataDelegate<string, string> fetch = async k =>
                await Task.FromResult(new[] { expectedResult })
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            var keys = new string[] { "Foo" };

            // act
            IReadOnlyList<Result<string>> loadResult = await loader
                .LoadAsync(keys)
                .ConfigureAwait(false);

            // assert
            Assert.NotNull(loadResult);
            Assert.Collection(loadResult,
                r => Assert.Equal(expectedResult.Value, r.Value));
        }

        #endregion

        #region LoadAsync(IReadOnlyCollection<string> keys)

        [Fact(DisplayName = "LoadAsync: Should throw an argument null exception for keys")]
        public async Task LoadCollectionKeysNull()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async k =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            IReadOnlyCollection<string> keys = null;

            // act
            Func<Task<IReadOnlyList<Result<string>>>> verify = () =>
                loader.LoadAsync(keys);

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>("keys", verify)
                .ConfigureAwait(false);
        }

        [Fact(DisplayName = "LoadAsync: Should throw an argument out of range exception for keys")]
        public async Task LoadCollectionZeroKeys()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async k =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            IReadOnlyCollection<string> keys = new string[0];

            // act
            Func<Task<IReadOnlyList<Result<string>>>> verify = () =>
                loader.LoadAsync(keys);

            // assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>("keys",
                verify).ConfigureAwait(false);
        }

        [Fact(DisplayName = "LoadAsync: Should not throw any exception")]
        public async Task LoadCollectionNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async k =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>()
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            IReadOnlyCollection<string> keys = new string[] { "Foo" };

            // act
            Func<Task<IReadOnlyList<Result<string>>>> verify = () =>
                loader.LoadAsync(keys);

            // assert
            Assert.Null(await Record.ExceptionAsync(verify)
                .ConfigureAwait(false));
        }

        [Fact(DisplayName = "LoadAsync: Should return one result")]
        public async Task LoadCollectionResult()
        {
            // arrange
            Result<string> expectedResult = Result<string>.Resolve("Bar");
            FetchDataDelegate<string, string> fetch = async k =>
                await Task.FromResult(new[] { expectedResult })
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            IReadOnlyCollection<string> keys = new string[] { "Foo" };

            // act
            IReadOnlyList<Result<string>> loadResult = await loader
                .LoadAsync(keys)
                .ConfigureAwait(false);

            // assert
            Assert.NotNull(loadResult);
            Assert.Collection(loadResult,
                r => Assert.Equal(expectedResult.Value, r.Value));
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
