using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DiagnosticAdapter;

namespace GreenDonut
{
    public class DispatchingListener
    {
        public readonly ConcurrentQueue<string> Keys =
            new ConcurrentQueue<string>();
        public readonly ConcurrentDictionary<string, IResult<string>> Values =
            new ConcurrentDictionary<string, IResult<string>>();
        public readonly ConcurrentDictionary<string, Exception> Errors =
            new ConcurrentDictionary<string, Exception>();

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
            IReadOnlyList<IResult<string>> results)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                Values.TryAdd(keys[i], results[i]);
            }
        }

        [DiagnosticName("Error")]
        public void OnError(string key, Exception exception)
        {
            Errors.TryAdd(key, exception);
        }
    }
}
