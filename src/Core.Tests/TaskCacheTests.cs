using System;
using System.Threading.Tasks;
using Xunit;

namespace GreenDonut
{
    public class TaskCacheTests
    {
        #region Constructor

        [Fact(DisplayName = "Constructor: Should not throw any exception")]
        public void ConstructorNoException()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;

            // act
            Action verify = () => new TaskCache<string, string>(cacheSize,
                slidingExpiration);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        #endregion

        #region Size

        [InlineData(0, 10)]
        [InlineData(1, 10)]
        [InlineData(10, 10)]
        [InlineData(100, 100)]
        [InlineData(1000, 1000)]
        [Theory(DisplayName = "Size: Should return the expected cache size")]
        public void Size(int cacheSize, int expectedCacheSize)
        {
            // arrange
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);

            // act
            var result = cache.Size;

            // assert
            Assert.Equal(expectedCacheSize, result);
        }

        #endregion

        #region SlidingExpirartion

        [InlineData(0, 0)]
        [InlineData(1, 1)]
        [InlineData(10, 10)]
        [InlineData(100, 100)]
        [InlineData(1000, 1000)]
        [Theory(DisplayName = "SlidingExpirartion: Should return the expected sliding expiration")]
        public void SlidingExpirartion(
            int expirationInMilliseconds,
            int expectedExpirationInMilliseconds)
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan
                .FromMilliseconds(expirationInMilliseconds);
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);

            // act
            TimeSpan result = cache.SlidingExpirartion;

            // assert
            Assert.Equal(expectedExpirationInMilliseconds,
                result.TotalMilliseconds);
        }

        #endregion

        #region Usage

        [InlineData(new string[] { "Foo" }, 1)]
        [InlineData(new string[] { "Foo", "Bar" }, 2)]
        [InlineData(new string[] { "Foo", "Bar", "Baz" }, 3)]
        [InlineData(new string[] { "Foo", "Bar", "Baz", "Qux", "Quux", "Corge",
            "Grault", "Graply", "Waldo", "Fred", "Plugh", "xyzzy" }, 10)]
        [Theory(DisplayName = "Usage: Should return the expected cache usage")]
        public void Usage(string[] values, int expectedUsage)
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);

            foreach (var value in values)
            {
                cache.Add($"Key:{value}", Task.FromResult(value));
            }

            // act
            var result = cache.Usage;

            // assert
            Assert.Equal(expectedUsage, result);
        }

        #endregion

        #region Add

        [Fact(DisplayName = "Add: Should throw an argument null exception for key")]
        public void AddKeyNull()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            string key = null;
            var value = Task.FromResult("Foo");

            // act
            Action verify = () => cache.Add(key, value);

            // assert
            Assert.Throws<ArgumentNullException>("key", verify);
        }

        [Fact(DisplayName = "Add: Should throw an argument null exception for value")]
        public void AddValueNull()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            var key = "Foo";
            Task<string> value = null;

            // act
            Action verify = () => cache.Add(key, value);

            // assert
            Assert.Throws<ArgumentNullException>("value", verify);
        }

        [Fact(DisplayName = "Add: Should not throw any exception")]
        public void AddNoException()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            var key = "Foo";
            var value = Task.FromResult("Bar");

            // act
            Action verify = () => cache.Add(key, value);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Add: Should result in a new cache entry")]
        public async Task AddNewCacheEntry()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            var key = "Foo";
            var value = Task.FromResult("Bar");

            // act
            cache.Add(key, value);

            // assert
            string expected = await value.ConfigureAwait(false);
            string actual = await cache.GetAsync(key).ConfigureAwait(false);

            Assert.Equal(expected, actual);
        }

        [Fact(DisplayName = "Add: Should result in 'Bar'")]
        public async Task AddTwice()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            var key = "Foo";
            var first = Task.FromResult("Bar");
            var second = Task.FromResult("Baz");

            // act
            cache.Add(key, first);
            cache.Add(key, second);
            cache.Add("Bar", second);

            // assert
            string expected = await first.ConfigureAwait(false);
            string actual = await cache.GetAsync(key).ConfigureAwait(false);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Clear

        [Fact(DisplayName = "Clear: Should not throw any exception")]
        public void ClearNoException()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);

            // act
            Action verify = () => cache.Clear();

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Clear: Should clear empty cache")]
        public void ClearEmptyCache()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);

            // act
            cache.Clear();

            // assert
            Assert.Equal(0, cache.Usage);
        }

        [Fact(DisplayName = "Clear: Should remove all entries from the cache")]
        public void ClearAllEntries()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);

            cache.Add("Foo", Task.FromResult("Bar"));
            cache.Add("Bar", Task.FromResult("Baz"));

            // act
            cache.Clear();

            // assert
            Assert.Equal(0, cache.Usage);
        }

        #endregion

        #region GetAsync

        [Fact(DisplayName = "GetAsync: Should throw an argument null exception for key")]
        public async Task GetAsyncKeyNull()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            string key = null;

            // act
            Func<Task<string>> verify = () => cache.GetAsync(key);

            // assert
            await Assert.ThrowsAsync<ArgumentNullException>("key", verify)
                .ConfigureAwait(false);
        }

        [Fact(DisplayName = "GetAsync: Should return null")]
        public void GetAsyncNullResult()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            var key = "Foo";

            // act
            Task<string> result = cache.GetAsync(key);

            // assert
            Assert.Null(result);
        }

        [Fact(DisplayName = "GetAsync: Should return one result")]
        public async Task GetAsyncResult()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            var key = "Foo";
            var value = Task.FromResult("Bar");

            cache.Add(key, value);

            // act
            string actual = await cache.GetAsync(key)
                .ConfigureAwait(false);

            // assert
            string expected = await value.ConfigureAwait(false);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Remove

        [Fact(DisplayName = "Remove: Should throw an argument null exception for key")]
        public void RemoveKeyNull()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            string key = null;

            // act
            Action verify = () => cache.Remove(key);

            // assert
            Assert.Throws<ArgumentNullException>("key", verify);
        }

        [Fact(DisplayName = "Remove: Should not throw any exception")]
        public void RemoveNoException()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            var key = "Foo";

            // act
            Action verify = () => cache.Remove(key);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Remove: Should remove an existing entry")]
        public void RemoveEntry()
        {
            // arrange
            int cacheSize = 10;
            TimeSpan slidingExpiration = TimeSpan.Zero;
            var cache = new TaskCache<string, string>(cacheSize,
                slidingExpiration);
            var key = "Foo";

            cache.Add(key, Task.FromResult("Bar"));

            // act
            cache.Remove(key);

            // assert
            Task<string> actual = cache.GetAsync(key);
            
            Assert.Null(actual);
        }

        #endregion
    }
}
