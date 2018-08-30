using System;

namespace GreenDonut
{
    internal static class Errors
    {
        public static InvalidOperationException CreateKeysAndValusMustMatch(
            int keysCount,
            int valuesCount)
        {
            var error = new InvalidOperationException("Fetch should have " +
                $"returned exactly \"{keysCount}\" value(s) but instead " +
                $"returned \"{valuesCount}\" value(s).");

            return error;
        }
    }
}
