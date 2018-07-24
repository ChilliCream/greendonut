using System;
using Xunit;

namespace GreenDonut
{
    public class DataLoaderOptionsTests
    {
        #region Constructor

        [Fact(DisplayName = "Constructor: Should not throw any exception")]
        public void ConstructorNoException()
        {
            // act
            Action verify = () => new DataLoaderOptions<string>();

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Constructor: Should set all properties")]
        public void ConstructorAllProps()
        {
            // act
            var options = new DataLoaderOptions<string>
            {
                AutoDispatching = false,
                Batching = false,
                BatchRequestDelay = TimeSpan.FromSeconds(1),
                CacheKeyResolver = k => k,
                CacheSize = 1,
                Caching = false,
                MaxBatchSize = 1,
                SlidingExpiration = TimeSpan.FromSeconds(10)
            };

            // assert
            Assert.False(options.AutoDispatching);
            Assert.False(options.Batching);
            Assert.Equal(TimeSpan.FromSeconds(1), options.BatchRequestDelay);
            Assert.NotNull(options.CacheKeyResolver);
            Assert.Equal(1, options.CacheSize);
            Assert.False(options.Caching);
            Assert.Equal(1, options.MaxBatchSize);
            Assert.Equal(TimeSpan.FromSeconds(10), options.SlidingExpiration);
        }

        #endregion
    }
}
