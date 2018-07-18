using System;

namespace GreenDonut
{
    internal static class SynchronizationExtensions
    {
        public static void Lock(
            this object sync,
            Func<bool> predicate,
            Action execute)
        {
            if (predicate())
            {
                lock (sync)
                {
                    if (predicate())
                    {
                        execute();
                    }
                }
            }
        }

        public static TResult Lock<TResult>(
            this object sync,
            Func<bool> predicate,
            Func<TResult> execute)
        {
            if (predicate())
            {
                lock (sync)
                {
                    if (predicate())
                    {
                        return execute();
                    }
                }
            }

            return default;
        }
    }
}
