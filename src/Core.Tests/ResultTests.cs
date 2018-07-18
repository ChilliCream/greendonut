using System;
using Xunit;

namespace GreenDonut
{
    public class ResultTests
    {
        #region Reject

        [Fact(DisplayName = "Reject: Should throw an argument null exception for errorMessage")]
        public void RejectStringNull()
        {
            // arrange
            string errorMessage = null;

            // act
            Action verify = () => Result<object>.Reject(errorMessage);

            // assert
            Assert.Throws<ArgumentNullException>("errorMessage", verify);
        }

        [Fact(DisplayName = "Reject: Should not throw any exception")]
        public void RejectStringNotNull()
        {
            // arrange
            string errorMessage = "Foo";

            // act
            Action verify = () => Result<object>.Reject(errorMessage);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Reject: Should return a rejected Result")]
        public void Reject()
        {
            // arrange
            string errorMessage = "Foo";

            // act
            Result<string> result = Result<string>.Reject(errorMessage);

            // assert
            Assert.NotNull(result);
            Assert.Equal("Foo", result.ErrorMessage);
            Assert.True(result.IsError);
            Assert.Null(result.Value);
        }

        #endregion

        #region Resolve

        [Fact(DisplayName = "Resolve: Should throw an argument null exception for value")]
        public void ResolveValueNull()
        {
            // arrange
            string value = null;

            // act
            Action verify = () => Result<string>.Resolve(value);

            // assert
            Assert.Throws<ArgumentNullException>("value", verify);
        }

        [Fact(DisplayName = "Resolve: Should not throw any exception")]
        public void ResolveValueNotNull()
        {
            // arrange
            string value = "Foo";

            // act
            Action verify = () => Result<string>.Resolve(value);

            // assert
            Assert.Null(Record.Exception(verify));
        }

        [Fact(DisplayName = "Resolve: Should return a resolved Result")]
        public void Resolve()
        {
            // arrange
            string value = "Foo";

            // act
            Result<string> result = Result<string>.Resolve(value);

            // assert
            Assert.NotNull(result);
            Assert.Null(result.ErrorMessage);
            Assert.False(result.IsError);
            Assert.Equal("Foo", result.Value);
        }

        #endregion
    }
}
