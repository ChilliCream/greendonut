using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.DiagnosticAdapter;

namespace GreenDonut
{
    public class TestListener
    {
        public readonly ConcurrentQueue<KeyValuePair<object, Exception>>
            BatchErrors =
                new ConcurrentQueue<KeyValuePair<object, Exception>>();
        public readonly ConcurrentQueue<object> BatchKeys =
            new ConcurrentQueue<object>();
        public readonly ConcurrentQueue<KeyValuePair<object, object>>
            BatchEntries =
                new ConcurrentQueue<KeyValuePair<object, object>>();
        public readonly ConcurrentQueue<KeyValuePair<object, object>>
            CachedEntries =
                new ConcurrentQueue<KeyValuePair<object, object>>();
        public readonly ConcurrentQueue<KeyValuePair<object, object>>
            Entries =
                new ConcurrentQueue<KeyValuePair<object, object>>();
        public readonly ConcurrentQueue<KeyValuePair<object, Exception>>
            Errors =
                new ConcurrentQueue<KeyValuePair<object, Exception>>();
        public readonly ConcurrentQueue<object> Keys =
            new ConcurrentQueue<object>();

        [DiagnosticName("GreenDonut.BatchError")]
        public void OnBatchError(
            IReadOnlyList<object> keys,
            Exception exception)
        {
            BatchErrors.Enqueue(
                new KeyValuePair<object, Exception>(keys, exception));
        }

        [DiagnosticName("GreenDonut.CachedValue")]
        public void OnCachedValue(object key, object value)
        {
            CachedEntries.Enqueue(new KeyValuePair<object, object>(key, value));
        }

        [DiagnosticName("GreenDonut.Error")]
        public void OnError(object key, Exception exception)
        {
            Errors.Enqueue(
                new KeyValuePair<object, Exception>(key, exception));
        }

        [DiagnosticName("GreenDonut.ExecuteBatchRequest")]
        public void OnExecuteBatchRequest() { }

        [DiagnosticName("GreenDonut.ExecuteBatchRequest.Start")]
        public void OnExecuteBatchRequestStart(
            IReadOnlyList<object> keys)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                BatchKeys.Enqueue(keys[i]);
            }
        }

        [DiagnosticName("GreenDonut.ExecuteBatchRequest.Stop")]
        public void OnExecuteBatchRequestStop(
            IReadOnlyList<object> keys,
            IReadOnlyList<object> values)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                BatchEntries.Enqueue(
                    new KeyValuePair<object, object>(keys[i], values[i]));
            }
        }

        [DiagnosticName("GreenDonut.ExecuteSingleRequest")]
        public void OnExecuteSingleRequest() { }

        [DiagnosticName("GreenDonut.ExecuteSingleRequest.Start")]
        public void OnExecuteSingleRequestStart(object key)
        {
            Keys.Enqueue(key);
        }

        [DiagnosticName("GreenDonut.ExecuteSingleRequest.Stop")]
        public void OnExecuteSingleRequestStop(
            object key,
            IReadOnlyCollection<object> values)
        {
            Entries.Enqueue(new KeyValuePair<object, object>(key, values));
        }
    }
}
