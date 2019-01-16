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
        public readonly ConcurrentDictionary<object, Exception> BatchErrors =
            new ConcurrentDictionary<object, Exception>();
        public readonly ConcurrentQueue<object> Keys =
            new ConcurrentQueue<object>();
        public readonly ConcurrentDictionary<object, object> Values =
            new ConcurrentDictionary<object, object>();

        public TestListener()
            : base("GreenDonut")
        { }

        [DiagnosticName("BatchError")]
        public void OnBatchError(object key, Exception exception)
        {
            BatchErrors.TryAdd(key, exception);
        }

        [DiagnosticName("ExecuteBatchRequest")]
        public void OnExecuteBatchRequest() { }

        [DiagnosticName("ExecuteBatchRequest.Start")]
        public void OnExecuteBatchRequestStart(
            IReadOnlyList<object> keys)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                Keys.Enqueue(keys[i]);
            }
        }

        [DiagnosticName("ExecuteBatchRequest.Stop")]
        public void OnExecuteBatchRequestStop(
            IReadOnlyList<object> keys,
            IReadOnlyList<object> values)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                Values.TryAdd(keys[i], values[i]);
            }
        }
    }
}
