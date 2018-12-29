using System;

namespace GreenDonut
{
    /// <summary>
    /// A wrapper for a single value which could contain a valid value or any
    /// error.
    /// </summary>
    /// <typeparam name="TValue">A value type</typeparam>
    public class Result<TValue>
    {
        private Result() { }

        /// <summary>
        /// Gets an error if <see cref="IsError"/> is <c>true</c>;
        /// otherwise null.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the result is an error.
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Gets the value if <see cref="IsError"/> is <c>false</c>;
        /// otherwise null.
        /// </summary>
        public TValue Value { get; private set; }

        /// <summary>
        /// Creates a new error result.
        /// </summary>
        /// <param name="error">An arbitrary error.</param>
        /// <exception cref="error">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        /// <returns>An error result.</returns>
        [Obsolete("This method is deprecated and will be removed in the next major release; " +
            "use instead implicit conversion. E.g. Result<string> foo = new Exception(\"Bar\");")]
        public static Result<TValue> Reject(Exception error)
        {
            return error;
        }

        /// <summary>
        /// Creates a new value result.
        /// </summary>
        /// <param name="value">An arbitrary value.</param>
        /// <returns>A value result.</returns>
        [Obsolete("This method is deprecated and will be removed in the next major release; " +
            "use instead implicit conversion. E.g. Result<string> foo = \"Bar\";")]
        public static Result<TValue> Resolve(TValue value)
        {
            return value;
        }

        /// <summary>
        /// Creates a new error result.
        /// </summary>
        /// <param name="error">An arbitrary error.</param>
        /// <exception cref="error">
        /// Throws an <see cref="ArgumentNullException"/> if <c>null</c>.
        /// </exception>
        public static implicit operator Result<TValue>(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            return new Result<TValue>
            {
                Error = error,
                IsError = true
            };
        }

        /// <summary>
        /// Creates a new value result.
        /// </summary>
        /// <param name="value">An arbitrary value.</param>
        public static implicit operator Result<TValue>(TValue value)
        {
            return new Result<TValue>
            {
                Value = value,
                IsError = false
            };
        }
    }
}
