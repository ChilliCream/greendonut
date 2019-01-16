using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DiagnosticAdapter;

namespace GreenDonut
{
    public class TestListener
        : DiagnosticListener
    {
        public readonly ConcurrentDictionary<string, Exception> BatchErrors =
            new ConcurrentDictionary<string, Exception>();
        public readonly ConcurrentQueue<string> Keys =
            new ConcurrentQueue<string>();
        public readonly ConcurrentDictionary<string, Result<string>> Values =
            new ConcurrentDictionary<string, Result<string>>();

        public TestListener()
            : base("GreenDonut")
        { }

        [DiagnosticName("BatchError")]
        public void OnBatchError(string key, Exception exception)
        {
            BatchErrors.TryAdd(key, exception);
        }

        [DiagnosticName("ExecuteBatchRequest")]
        public void OnExecuteBatchRequest() { }

        [DiagnosticName("ExecuteBatchRequest.Start")]
        public void OnExecuteBatchRequestStart(
            IReadOnlyList<string> keys)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                Keys.Enqueue(keys[i]);
            }
        }

        [DiagnosticName("ExecuteBatchRequest.Stop")]
        public void OnExecuteBatchRequestStop(
            IReadOnlyList<string> keys,
            IReadOnlyList<Result<string>> results)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                Values.TryAdd(keys[i], results[i]);
            }
        }
    }
}
