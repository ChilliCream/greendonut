using System;

namespace GreenDonut
{
    /// <summary>
    /// A wrapper for a single value which could contain a valid value or an
    /// error.
    /// </summary>
    /// <typeparam name="TValue">A value type</typeparam>
    public class Result<TValue>
    {
        private Result() { }

        /// <summary>
        /// Gets an error if it is an error; otherwise null.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the result is an error.
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Gets the value if not an error; otherwise null.
        /// </summary>
        public TValue Value { get; private set; }

        /// <summary>
        /// Creates a new error result.
        /// </summary>
        /// <param name="error">An arbitrary error.</param>
        /// <returns>An error result.</returns>
        public static Result<TValue> Reject(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            var result = new Result<TValue>();

            result.Error = error;
            result.IsError = true;

            return result;
        }

        /// <summary>
        /// Creates a new value result.
        /// </summary>
        /// <param name="value">An arbitrary value.</param>
        /// <returns>A value result.</returns>
        public static Result<TValue> Resolve(TValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var result = new Result<TValue>();

            result.Value = value;
            result.IsError = false;

            return result;
        }
    }
}
