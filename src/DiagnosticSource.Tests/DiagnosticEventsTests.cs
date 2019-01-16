using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace GreenDonut
{
    public class DiagnosticEventsTests
    {
        [Fact(DisplayName = "ExecuteBatchRequest: Should record a batch request plus error")]
        public async Task ExecuteBatchRequest()
        {
            var listener = new TestListener();
            var observer = new TestObserver(listener);

            using (DiagnosticListener.AllListeners.Subscribe(observer))
            {
                // arrange
                FetchDataDelegate<string, string> fetch =
                    async (keys, cancellationToken) =>
                    {
                        var error = new Exception("Quux");

                        return await Task.FromResult(new Result<string>[]
                        {
                            error
                        }).ConfigureAwait(false);
                    };
                var options = new DataLoaderOptions<string>
                {
                    AutoDispatching = true
                };
                var loader = new DataLoader<string, string>(options, fetch);

                // act
                try
                {
                    await loader.LoadAsync("Foo").ConfigureAwait(false);
                }
                catch
                {
                }

                // assert
                Assert.Collection(listener.Keys,
                    (key) => Assert.Equal("Foo", key));
                Assert.Collection(listener.Values,
                    (item) =>
                    {
                        Assert.Equal("Foo", item.Key);
                        Assert.Null(item.Value);
                    });
                Assert.Collection(listener.BatchErrors,
                    (item) =>
                    {
                        Assert.Equal("Foo", item.Key);
                        Assert.Equal("Quux", item.Value.Message);
                    });
            }
        }
    }
}
