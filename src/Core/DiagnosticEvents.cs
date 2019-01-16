using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GreenDonut
{
    internal static class DiagnosticEvents
    {
        private const string _diagnosticSourceName = "GreenDonut";
        private const string _batchActivityName = "ExecuteBatchRequest";
        private const string _singleActivityName = "ExecuteSingleRequest";
        private const string _batchErrorEventName = "BatchError";
        private const string _cachedValueEventName = "CachedValue";
        private const string _errorEventName = "Error";

        private static readonly DiagnosticSource _source =
            new DiagnosticListener(_diagnosticSourceName);

        public static void ReceivedBatchError<TKey>(
            IReadOnlyList<TKey> keys,
            Exception exception)
        {
            var payload = new
            {
                Exception = exception,
                Keys = keys
            };

            if (_source.IsEnabled(_errorEventName, payload))
            {
                _source.Write(_errorEventName, payload);
            }
        }

        public static void ReceivedError<TKey>(
            TKey key,
            Exception exception)
        {
            var payload = new
            {
                Exception = exception,
                Key = key
            };

            if (_source.IsEnabled(_batchErrorEventName, payload))
            {
                _source.Write(_batchErrorEventName, payload);
            }
        }

        public static void ReceivedValueFromCache<TKey, TValue>(
            TKey key,
            object cacheKey,
            TValue value)
        {
            var payload = new
            {
                CacheKey = cacheKey,
                Key = key,
                Value = value
            };

            if (_source.IsEnabled(_cachedValueEventName, payload))
            {
                _source.Write(_cachedValueEventName, payload);
            }
        }

        public static Activity StartBatching<TKey>(
            IReadOnlyList<TKey> keys)
        {
            var payload = new
            {
                Keys = keys
            };

            if (_source.IsEnabled(_batchActivityName, payload))
            {
                var activity = new Activity(_batchActivityName);

                _source.StartActivity(activity, payload);

                return activity;
            }

            return null;
        }

        public static void StopBatching<TKey, TValue>(
            Activity activity,
            IReadOnlyList<TKey> keys,
            IReadOnlyList<Result<TValue>> results)
        {
            if (activity != null)
            {
                var payload = new
                {
                    Keys = keys,
                    Results = results
                };

                if (_source.IsEnabled(_batchActivityName, payload))
                {
                    _source.StopActivity(activity, payload);
                }
            }
        }

        public static Activity StartSingle<TKey>(TKey key)
        {
            var payload = new
            {
                Key = key
            };

            if (_source.IsEnabled(_singleActivityName, payload))
            {
                var activity = new Activity(_singleActivityName);

                _source.StartActivity(activity, payload);

                return activity;
            }

            return null;
        }

        public static void StopSingle<TKey, TValue>(
            Activity activity,
            TKey key,
            IReadOnlyList<Result<TValue>> results)
        {
            if (activity != null)
            {
                var payload = new
                {
                    Key = key,
                    Results = results
                };

                if (_source.IsEnabled(_singleActivityName, payload))
                {
                    _source.StopActivity(activity, payload);
                }
            }
        }
    }
}
