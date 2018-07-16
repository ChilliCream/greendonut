using System;

namespace GreenDonut
{
    public class Result<TValue>
    {
        public Result(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new ArgumentNullException(nameof(errorMessage));
            }

            ErrorMessage = errorMessage;
            IsError = true;
        }

        public Result(TValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
            IsError = false;
        }

        public string ErrorMessage { get; private set; }

        public bool IsError { get; private set; }

        public TValue Value { get; private set; }
    }
}
