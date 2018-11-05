using System;

namespace GreenDonut
{
    /// <summary>
    /// Represents a single value which could contain a valid value or an
    /// error.
    /// </summary>
    /// <typeparam name="TValue">A value type</typeparam>
    public interface IResult<out TValue>
    {
        /// <summary>
        /// Gets an error if it is an error; otherwise null.
        /// </summary>
        Exception Error { get; }

        /// <summary>
        /// Gets a value indicating whether the result is an error.
        /// </summary>
        bool IsError { get; }

        /// <summary>
        /// Gets the value if not an error; otherwise null.
        /// </summary>
        TValue Value { get; }
    }
}
