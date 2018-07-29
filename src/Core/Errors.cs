using System;

namespace GreenDonut
{
    internal static class Errors
    {
        public static Exception CreateKeysAndValusMustMatch(int keysCount,
            int valuesCount)
        {
            var error = new Exception("Fetch should have returned exactly " +
                $"\"{keysCount}\" value(s) but instead returned " +
                $"\"{valuesCount}\" value(s).");

            return error;
        }
    }
}
