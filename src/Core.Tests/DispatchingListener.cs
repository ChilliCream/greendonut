using System;
using System.Collections.Generic;
using Microsoft.Extensions.DiagnosticAdapter;

namespace GreenDonut
{
    public class DispatchingListener
    {
        public readonly List<string> Keys = new List<string>();
        public readonly Dictionary<string, IResult<string>> Values =
            new Dictionary<string, IResult<string>>();
        public readonly Dictionary<string, Exception> Errors =
            new Dictionary<string, Exception>();

        [DiagnosticName("ExecuteBatchRequest")]
        public void OnExecuteBatchRequest() { }

        [DiagnosticName("ExecuteBatchRequest.Start")]
        public void OnExecuteBatchRequestStart(
            IReadOnlyList<string> keys)
        {
            Keys.AddRange(keys);
        }

        [DiagnosticName("ExecuteBatchRequest.Stop")]
        public void OnExecuteBatchRequestStop(
            IReadOnlyList<string> keys,
            IReadOnlyList<IResult<string>> results)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                Values[keys[i]] = results[i];
            }
        }

        [DiagnosticName("Error")]
        public void OnError(string key, Exception exception)
        {
            Errors.Add(key, exception);
        }
    }
}
