using System;

namespace GreenDonut
{
    internal static class Defaults
    {
        public static readonly int CacheSize = 1000;
        public static readonly TimeSpan BatchRequestDelay = TimeSpan
            .FromMilliseconds(50);
        public static readonly int MinimumCacheSize = 10;
        public static readonly TimeSpan SlidingExpiration = TimeSpan.Zero;
    }
}
