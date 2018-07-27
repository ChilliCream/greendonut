using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GreenDonut
{
    public class DataLoaderTests
    {
        #region Constructor(fetch)

        [Fact(DisplayName = "Constructor: Should throw an argument null exception for fetch")]
        public void ConstructorAFetchNull()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = null;

            // act
            Action verify = () => new DataLoader<string, string>(fetch);

            // assert
            Assert.Throws<ArgumentNullException>("fetch", verify);
        }

        [Fact(DisplayName = "Constructor: Should not throw any exception")]
        public void ConstructorANoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);

            // act
            Action verify = () => new DataLoader<string, string>(fetch);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        #endregion

        #region Constructor(fetch, options)

        [Fact(DisplayName = "Constructor: Should throw an argument null exception for fetch")]
        public void ConstructorBFetchNull()
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
        public void ConstructorBOptionsNull()
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
        public void ConstructorBNoException()
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

        [Fact(DisplayName = "Clear: Should remove all entries from the cache")]
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

            loader.Set("Foo", Task.FromResult("Bar"));
            loader.Set("Bar", Task.FromResult("Baz"));

            // act
            IDataLoader<string, string> result = loader.Clear();

            // assert
            Func<Task> verify = () => loader.LoadAsync("Foo", "Bar");

            Assert.Equal(loader, result);
            await Assert.ThrowsAsync<Exception>(verify).ConfigureAwait(false);
        }

        #endregion

        #region DispatchAsync

        [Fact(DisplayName = "DispatchAsync: Should not throw any exception")]
        public async Task DispatchAsyncNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);

            // act
            Func<Task> verify = () => loader.DispatchAsync();

            // assert
            Assert.Null(await Record.ExceptionAsync(verify)
                .ConfigureAwait(false));
        }

        [Fact(DisplayName = "DispatchAsync: Should do nothing if batching is disabled")]
        public async Task DispatchAsyncNoBatching()
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

            // this would block if batching would be enabled
            string loadResult = await loader.LoadAsync("Foo")
                .ConfigureAwait(false);

            // act
            await loader.DispatchAsync().ConfigureAwait(false);

            // assert
            Assert.Equal(expectedResult.Value, loadResult);
        }

        [Fact(DisplayName = "DispatchAsync: Should do a manual dispatch if auto dispatching is disabled")]
        public async Task DispatchAsyncManual()
        {
            // arrange
            Result<string> expectedResult = Result<string>.Resolve("Bar");
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new[] { expectedResult })
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>
            {
                AutoDispatching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);

            Task<string> loadResult = loader.LoadAsync("Foo");

            // act
            await loader.DispatchAsync().ConfigureAwait(false);

            // assert
            Assert.Equal(expectedResult.Value,
                await loadResult.ConfigureAwait(false));
        }

        [Fact(DisplayName = "DispatchAsync: Should interrupt the 10 minutes delay if auto dispatching is enabled")]
        public async Task DispatchAsyncAuto()
        {
            // arrange
            Result<string> expectedResult = Result<string>.Resolve("Bar");
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new[] { expectedResult })
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>
            {
                BatchRequestDelay = TimeSpan.FromMinutes(10)
            };
            var loader = new DataLoader<string, string>(options, fetch);

            await Task.Delay(10);

            Task<string> loadResult = loader.LoadAsync("Foo");

            // act
            await loader.DispatchAsync().ConfigureAwait(false);

            // assert
            Assert.Equal(expectedResult.Value,
                await loadResult.ConfigureAwait(false));
        }

        #endregion

        #region Dispose

        [Fact(DisplayName = "Dispose: Should dispose and not throw any exception")]
        public void DisposeNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new Result<string>[0])
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);

            // act
            Action verify = () => loader.Dispose();

            // assert
            Assert.Null(Record.Exception(verify));
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
            Func<Task<string>> verify = () => loader.LoadAsync(key);

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>("key", verify)
                .ConfigureAwait(false);
        }

        [Fact(DisplayName = "LoadAsync: Should not throw any exception")]
        public async Task LoadSingleNoException()
        {
            // arrange
            FetchDataDelegate<string, string> fetch = async keys =>
                await Task.FromResult(new[] { Result<string>.Resolve("Bar") })
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>()
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            var key = "Foo";

            // act
            Func<Task<string>> verify = () => loader.LoadAsync(key);

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
            string loadResult = await loader.LoadAsync(key)
                .ConfigureAwait(false);

            // assert
            Assert.Equal(expectedResult.Value, loadResult);
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
            Func<Task<IReadOnlyList<string>>> verify = () =>
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
            var keys = new string[0];

            // act
            Func<Task<IReadOnlyList<string>>> verify = () =>
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
                await Task.FromResult(new[] { Result<string>.Resolve("Bar") })
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>()
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            var keys = new string[] { "Foo" };

            // act
            Func<Task<IReadOnlyList<string>>> verify = () =>
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
            IReadOnlyList<string> loadResult = await loader
                .LoadAsync(keys)
                .ConfigureAwait(false);

            // assert
            Assert.Collection(loadResult,
                r => Assert.Equal(expectedResult.Value, r));
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
            List<string> keys = null;

            // act
            Func<Task<IReadOnlyList<string>>> verify = () =>
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
            var keys = new List<string>();

            // act
            Func<Task<IReadOnlyList<string>>> verify = () =>
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
                await Task.FromResult(new[] { Result<string>.Resolve("Bar") })
                    .ConfigureAwait(false);
            var options = new DataLoaderOptions<string>()
            {
                Batching = false
            };
            var loader = new DataLoader<string, string>(options, fetch);
            var keys = new List<string> { "Foo" };

            // act
            Func<Task<IReadOnlyList<string>>> verify = () =>
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
            var keys = new List<string> { "Foo" };

            // act
            IReadOnlyList<string> loadResult = await loader
                .LoadAsync(keys)
                .ConfigureAwait(false);

            // assert
            Assert.Collection(loadResult,
                v => Assert.Equal(expectedResult.Value, v));
        }

        [Fact(DisplayName = "LoadAsync: Should throw one exception for not existing value regarding key 'Qux'")]
        public async Task LoadAutoDispatching()
        {
            // arrange
            Result<string> expectedResult = Result<string>.Resolve("Bar");
            var repository = new Dictionary<string, string>
            {
                { "Foo", "Bar" },
                { "Bar", "Baz" },
                { "Baz", "Foo" }
            };
            FetchDataDelegate<string, string> fetch = async k =>
            {
                var values = new List<Result<string>>();

                foreach (var key in k)
                {
                    if (repository.ContainsKey(key))
                    {
                        values.Add(Result<string>.Resolve(repository[key]));
                    }
                    else
                    {
                        var error = new Exception($"Value for key \"{key}\" " +
                            "not found");

                        values.Add(Result<string>.Reject(error));
                    }
                }

                return await Task.FromResult(values).ConfigureAwait(false);
            };
            var options = new DataLoaderOptions<string>();
            var loader = new DataLoader<string, string>(options, fetch);
            var keys = new List<string> { "Foo", "Bar", "Baz", "Qux" };

            // act
            Func<Task> verify = () => loader.LoadAsync(keys);

            // assert
            await Assert.ThrowsAsync<Exception>(verify).ConfigureAwait(false);
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

            loader.Set("Foo", Task.FromResult("Bar"));

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

            loader.Set(key, Task.FromResult("Bar"));

            // act
            IDataLoader<string, string> result = loader.Remove(key);

            // assert
            Task<string> loadResult = loader.LoadAsync(key);

            Assert.Equal(loader, result);
            Assert.NotNull(loadResult.Exception);
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
            var value = Task.FromResult("Foo");

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
            Task<string> value = null;

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
            var value = Task.FromResult("Bar");

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
            var value = Task.FromResult("Bar");

            // act
            IDataLoader<string, string> result = loader.Set(key, value);

            // assert
            string loadResult = await loader.LoadAsync(key)
                .ConfigureAwait(false);

            Assert.Equal(loader, result);
            Assert.Equal(value.Result, loadResult);
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
            var first = Task.FromResult("Bar");
            var second = Task.FromResult("Baz");

            // act
            loader.Set(key, first);
            loader.Set(key, second);

            // assert
            string loadResult = await loader.LoadAsync(key)
                .ConfigureAwait(false);
            
            Assert.Equal(first.Result, loadResult);
        }

        #endregion
    }
}
