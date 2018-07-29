using System;

namespace GreenDonut
{
    internal class Errors
    {
        public static Exception CreateMustHaveOneResult(int resultsCount)
        {
            var error = new Exception("Fetch should have returned exactly " +
                $"one result but instead returned \"{resultsCount}\" " +
                "results.");

            return error;
        }

        public static Exception CreateEveryKeyMustHaveAValue(int keysCount,
            int resultsCount)
        {
            var error = new Exception("Fetch should have returned exactly " +
                $"\"{keysCount}\" result(s) but instead returned " +
                $"\"{resultsCount}\" result(s).");

            return error;
        }
    }
}
