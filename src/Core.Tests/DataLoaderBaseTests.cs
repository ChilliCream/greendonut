using System;
using System.Collections.Generic;
using System.Linq;
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
                await Task.FromResult(new IResult<string>[0])
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
            TaskCache<string, string> cache = null;

            // act
            Action verify = () => new CacheConstructor(cache);

            // assert
            Assert.Throws<ArgumentNullException>("cache", verify);
        }

        [Fact(DisplayName = "Constructor: Should not throw any exception")]
        public void ConstructorBNoException()
        {
            // arrange
            var cache = new TaskCache<string, string>(10, TimeSpan.Zero);

            // act
            Action verify = () => new CacheConstructor(cache);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        #endregion

        #region bugfix - Allow Empty Lists To be loaded


        [Fact(DisplayName = "Confirm EmptyListDataLoader works as expected")]
        public async Task TestEmptyListDataLoader() {
            var strings = new [] {"item1", "item2", "item3"};
            var dataLoader = new EmptyListDataLoader();

            var loadTask = Task.WhenAll(dataLoader.LoadAsync(strings));
            await dataLoader.DispatchAsync();
            var result = loadTask.Result;

            Assert.Collection(result, m => Assert.True(strings.All(s => m.Contains(s))));
        }
        
        [Fact(DisplayName = "LoadAsync should allow empty lists as parameter")]
        public async Task AllowEmptyListOnReadOnlyListAsync() {
            var dataLoader = new EmptyListDataLoader();

            var loadTask = Task.WhenAll(dataLoader.LoadAsync(new List<string>()));
            await dataLoader.DispatchAsync();
            var result = loadTask.Result;

            Assert.Collection(result, m => Assert.True(m.Count == 0));
        }
        
        [Fact(DisplayName = "LoadAsync should allow empty lists as parameter")]
        public async Task AllowEmptyListOnArrayAsync() {
            var dataLoader = new EmptyListDataLoader();

            var loadTask = Task.WhenAll(dataLoader.LoadAsync(new string[0]));
            await dataLoader.DispatchAsync();
            var result = loadTask.Result;

            Assert.Collection(result, m => Assert.True(m.Count == 0));
        }
        
        #endregion
    }
}
